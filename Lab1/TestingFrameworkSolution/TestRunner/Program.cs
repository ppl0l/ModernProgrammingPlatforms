using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TestingLibrary;

namespace TestRunner
{
    class Program
    {
        static async Task Main()
        {
            int maxParallel = 4;
            string dllPath = "CalculatorTests.dll";

            Console.WriteLine("СРАВНЕНИЕ ЭФФЕКТИВНОСТИ");

            var runner = new Runner();

            Console.WriteLine("\n[1] Запуск: ПОСЛЕДОВАТЕЛЬНО");
            var sw = Stopwatch.StartNew();
            await runner.RunAsync(dllPath, 1);
            sw.Stop();
            long seqTime = sw.ElapsedMilliseconds;

            Console.WriteLine($"\n[2] Запуск: ПАРАЛЛЕЛЬНО (MaxDegree = {maxParallel})");
            sw.Restart();
            await runner.RunAsync(dllPath, maxParallel);
            sw.Stop();
            long parTime = sw.ElapsedMilliseconds;

            Console.WriteLine("\n" + new string('=', 30));
            Console.WriteLine($"Последовательно: {seqTime} ms");
            Console.WriteLine($"Параллельно:     {parTime} ms");
            Console.WriteLine($"Ускорение:      {((double)seqTime / parTime):F2}x");
            Console.WriteLine(new string('=', 30));
        }
    }

    public class Runner
    {
        private readonly object _consoleLock = new object();
        private int _total, _passed, _failed;

        public async Task RunAsync(string dllPath, int maxDegreeOfParallelism)
        {
            _total = 0; _passed = 0; _failed = 0;
            var asm = Assembly.LoadFrom(dllPath);

            var testClasses = asm.GetTypes()
                .Where(t => t.GetMethods().Any(m => m.IsDefined(typeof(Test), false)))
                .ToList();

            using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
            var allTestTasks = new List<Task>();

            foreach (var type in testClasses)
            {
                var methods = type.GetMethods();
                var isShared = type.IsDefined(typeof(Shared), false);
                object sharedObj = isShared ? Activator.CreateInstance(type) : null;

                var beforeClass = methods.FirstOrDefault(m => m.IsDefined(typeof(BeforeClass), false));
                beforeClass?.Invoke(sharedObj ?? Activator.CreateInstance(type), null);

                var testMethods = methods.Where(m => m.IsDefined(typeof(Test), false)).ToList();

                foreach (var method in testMethods)
                {
                    var cases = method.GetCustomAttributes<TestCase>().ToList();
                    if (!cases.Any()) cases.Add(new TestCase { Data = null });

                    foreach (var tc in cases)
                    {
                        await semaphore.WaitAsync();
                        allTestTasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                object inst = sharedObj ?? Activator.CreateInstance(type);
                                await ExecuteTest(method, tc.Data, inst, type);
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }));
                    }
                }
            }

            await Task.WhenAll(allTestTasks);

            lock (_consoleLock)
            {
                Console.WriteLine($"\nИтог: Всего: {_total}, ОК: {_passed}, Провалено: {_failed}");
            }
        }

        private async Task ExecuteTest(MethodInfo m, object[] args, object instance, Type classType)
        {
            Interlocked.Increment(ref _total);
            string testName = $"{classType.Name}.{m.Name}" + (args != null ? $"({string.Join(",", args)})" : "");

            var befores = classType.GetMethods().Where(meth => meth.IsDefined(typeof(Before), false)).ToList();
            var afters = classType.GetMethods().Where(meth => meth.IsDefined(typeof(After), false)).ToList();
            var timeoutAttr = m.GetCustomAttribute<TimeoutAttribute>();

            try
            {
                foreach (var b in befores) b.Invoke(instance, null);

                Task testTask = Task.Run(async () =>
                {
                    if (m.ReturnType == typeof(Task) || m.ReturnType.BaseType == typeof(Task))
                        await (Task)m.Invoke(instance, args);
                    else
                        m.Invoke(instance, args);
                });

                if (timeoutAttr != null)
                {
                    if (await Task.WhenAny(testTask, Task.Delay(timeoutAttr.Milliseconds)) != testTask)
                        throw new Exception($"Превышен лимит времени ({timeoutAttr.Milliseconds}ms)");
                }
                else
                {
                    await testTask;
                }

                foreach (var a in afters) a.Invoke(instance, null);

                LogResult(testName, "OK");
                Interlocked.Increment(ref _passed);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _failed);
                var msg = ex.InnerException?.Message ?? ex.Message;
                LogResult(testName, $"ПРОВАЛ: {msg}");
            }
        }

        private void LogResult(string name, string status)
        {
            lock (_consoleLock)
            {
                Console.WriteLine($"  [{Thread.CurrentThread.ManagedThreadId:00}] {name,-40} -> {status}");
            }
        }
    }
}