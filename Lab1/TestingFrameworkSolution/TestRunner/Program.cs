using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using CustomThreadPoolLib;
using TestingLibrary;
using System.Linq;

namespace TestRunner
{
    class Program
    {
        static void Main()
        {
            using var pool = new MyThreadPool(minThreads: 2, maxThreads: 10, idleTimeoutMs: 3000);
            var runner = new NewRunner(pool);

            Console.WriteLine("ЗАПУСК МОДЕЛИРОВАНИЯ НАГРУЗКИ");

            Thread monitorThread = new Thread(() =>
            {
                while (true)
                {
                    Console.Title = $"Threads: {pool.CurrentThreadCount} | Queue: {pool.QueueLength}";
                    pool.HealthCheck(5000);
                    Thread.Sleep(1000);
                }
            })
            { IsBackground = true };
            monitorThread.Start();

            string dllPath = "CalculatorTests.dll";

            Console.WriteLine("\nЭТАП 1: Единичные подачи");
            for (int i = 0; i < 5; i++)
            {
                runner.EnqueueAllTests(dllPath);
                Thread.Sleep(500);
            }

            Console.WriteLine("\nЭТАП 2: ПИКОВАЯ НАГРУЗКА (взрыв задач)");
            for (int i = 0; i < 10; i++) runner.EnqueueAllTests(dllPath);

            Console.WriteLine("\nЭТАП 3: Пауза (ожидаем сжатия пула)");
            Thread.Sleep(5000);

            Console.WriteLine("\nЭТАП 4: Финальный залп");
            for (int i = 0; i < 5; i++) runner.EnqueueAllTests(dllPath);

            Console.WriteLine("\nНажмите Enter для завершения после выполнения всех тестов.");
            Console.ReadLine();
        }
    }

    public class NewRunner
    {
        private readonly MyThreadPool _pool;
        private int _total = 0;

        public NewRunner(MyThreadPool pool) => _pool = pool;

        public void EnqueueAllTests(string dllPath)
        {
            var asm = Assembly.LoadFrom(dllPath);
            var testClasses = asm.GetTypes()
                .Where(t => t.GetMethods().Any(m => m.IsDefined(typeof(Test), false)))
                .ToList();

            foreach (var type in testClasses)
            {
                var methods = type.GetMethods().Where(m => m.IsDefined(typeof(Test), false));
                foreach (var m in methods)
                {
                    Interlocked.Increment(ref _total);
                    _pool.Enqueue(() => ExecuteSingleTest(type, m));
                }
            }
        }

        private void ExecuteSingleTest(Type type, MethodInfo m)
        {
            try
            {
                var instance = Activator.CreateInstance(type);

                var initMethods = type.GetMethods()
                    .Where(meth => meth.IsDefined(typeof(BeforeClass), false) ||
                                   meth.IsDefined(typeof(Before), false));

                foreach (var init in initMethods)
                {
                    init.Invoke(instance, null);
                }

                if (m.ReturnType == typeof(Task))
                {
                    ((Task)m.Invoke(instance, null)).Wait();
                }
                else
                {
                    m.Invoke(instance, null);
                }

                Console.WriteLine($"  [ID:{Thread.CurrentThread.ManagedThreadId:00}] {type.Name}.{m.Name} -> OK");
            }
            catch (Exception ex)
            {
                var realException = ex.InnerException ?? ex;
                Console.WriteLine($"  [ID:{Thread.CurrentThread.ManagedThreadId:00}] {type.Name}.{m.Name} -> FAIL: {realException.Message}");
            }
        }
    }
}