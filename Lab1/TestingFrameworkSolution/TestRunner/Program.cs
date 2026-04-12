using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Linq;
using CustomThreadPoolLib;
using TestingLibrary;

namespace TestRunner
{
    class Program
    {
        static void Main()
        {
            using var pool = new MyThreadPool(2, 5);

            pool.OnPoolChanged += (s, e) => Console.WriteLine($"[POOL_EVENT] {e.Message}. Потоков: {e.ThreadCount}");
            pool.OnTaskStarted += (s, e) => Console.WriteLine($"[TASK_EVENT] {e.Message}. В очереди: {e.QueueLength}");

            var runner = new NewRunner(pool);
            string dll = "CalculatorTests.dll";

            Console.WriteLine("ЗАПУСК ВСЕХ ВЫСОКОПРИОРИТЕТНЫХ ТЕСТОВ (Priority > 3)");
            runner.EnqueueFiltered(dll, m => {
                var attr = m.GetCustomAttribute<Priority>();
                return attr != null && attr.Level > 3;
            });

            Thread.Sleep(2000);

            Console.WriteLine("\nЗАПУСК ТЕСТОВ КАТЕГОРИИ 'ExpressionTree'");
            runner.EnqueueFiltered(dll, m => {
                var attr = m.GetCustomAttribute<Category>() ?? m.DeclaringType.GetCustomAttribute<Category>();
                return attr?.Name == "ExpressionTree";
            });

            Console.ReadLine();
        }
    }

    public class NewRunner
    {
        private readonly MyThreadPool _pool;
        public NewRunner(MyThreadPool pool) => _pool = pool;

        public void EnqueueFiltered(string dllPath, Func<MethodInfo, bool> filter)
        {
            var asm = Assembly.LoadFrom(dllPath);
            var types = asm.GetTypes().Where(t => t.GetMethods().Any(m => m.IsDefined(typeof(Test), false)));

            foreach (var type in types)
            {
                var methods = type.GetMethods().Where(m => m.IsDefined(typeof(Test), false) && filter(m));
                foreach (var m in methods)
                {
                    var sourceAttr = m.GetCustomAttribute<TestCaseSource>();
                    if (sourceAttr != null)
                    {
                        var sourceMethod = type.GetMethod(sourceAttr.MethodName, BindingFlags.Public | BindingFlags.Static);
                        var data = (IEnumerable<object[]>)sourceMethod.Invoke(null, null);
                        foreach (var @params in data)
                        {
                            _pool.Enqueue(() => ExecuteTest(type, m, @params));
                        }
                    }
                    else
                    {
                        _pool.Enqueue(() => ExecuteTest(type, m, null));
                    }
                }
            }
        }

        private void ExecuteTest(Type type, MethodInfo m, object[] args)
        {
            try
            {
                var instance = Activator.CreateInstance(type);
                m.Invoke(instance, args);
                Console.WriteLine($"[OK] {type.Name}.{m.Name}({string.Join(",", args ?? new object[0])})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FAIL] {type.Name}.{m.Name}: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}