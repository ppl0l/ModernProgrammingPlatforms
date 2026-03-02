using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TestingLibrary;

namespace TestRunner
{
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("ТЕСТИРОВАНИЕ\n");

            try
            {
                var asm = Assembly.LoadFrom("CalculatorTests.dll");
                var testAttr = typeof(Test);
                var testCaseAttr = typeof(TestCase);
                var beforeAttr = typeof(Before);
                var afterAttr = typeof(After);
                var beforeClassAttr = typeof(BeforeClass);
                var afterClassAttr = typeof(AfterClass);
                var sharedAttr = typeof(Shared);

                var runner = new Runner();
                await runner.Run(asm, testAttr, testCaseAttr, beforeAttr, afterAttr,
                               beforeClassAttr, afterClassAttr, sharedAttr);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }

    public class Runner
    {
        Dictionary<Type, object> shared = new Dictionary<Type, object>();
        int total = 0, passed = 0, failed = 0;

        public async Task Run(Assembly asm, Type testAttr, Type testCaseAttr, Type beforeAttr,
            Type afterAttr, Type beforeClassAttr, Type afterClassAttr, Type sharedAttr)
        {
            var testClasses = asm.GetTypes()
                .Where(t => t.GetMethods().Any(m => m.GetCustomAttributes(testAttr, false).Any()))
                .ToList();

            if (!testClasses.Any())
            {
                Console.WriteLine("Тестов не найдено!");
                return;
            }

            foreach (var type in testClasses)
            {
                Console.WriteLine($"\n[{type.Name}]");

                object obj = null;
                if (sharedAttr != null && type.GetCustomAttribute(sharedAttr) != null)
                {
                    if (!shared.ContainsKey(type))
                        shared[type] = Activator.CreateInstance(type);
                    obj = shared[type];
                }

                var methods = type.GetMethods();
                var beforeClass = methods.FirstOrDefault(m => m.GetCustomAttributes(beforeClassAttr, false).Any());
                var afterClass = methods.FirstOrDefault(m => m.GetCustomAttributes(afterClassAttr, false).Any());
                var befores = methods.Where(m => m.GetCustomAttributes(beforeAttr, false).Any()).ToList();
                var afters = methods.Where(m => m.GetCustomAttributes(afterAttr, false).Any()).ToList();

                try
                {
                    beforeClass?.Invoke(obj ?? Activator.CreateInstance(type), null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в BeforeClass: {ex.InnerException?.Message ?? ex.Message}");
                    continue;
                }

                foreach (var method in methods.Where(m => m.GetCustomAttributes(testAttr, false).Any()))
                {
                    var inst = obj ?? Activator.CreateInstance(type);
                    var cases = method.GetCustomAttributes(testCaseAttr, false).ToList();

                    if (cases.Any())
                    {
                        foreach (var tc in cases)
                        {
                            var data = testCaseAttr.GetProperty("Data")?.GetValue(tc) as object[];
                            await RunTest(method, data, inst, befores, afters);
                        }
                    }
                    else
                    {
                        await RunTest(method, null, inst, befores, afters);
                    }
                }

                try
                {
                    afterClass?.Invoke(obj ?? Activator.CreateInstance(type), null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка в AfterClass: {ex.InnerException?.Message ?? ex.Message}");
                }
            }

            Console.WriteLine($"\n{"".PadLeft(40, '_')}");
            Console.WriteLine($"Всего тестов: {total}");
            Console.WriteLine($"Успешно: {passed}");
            Console.WriteLine($"Провалено: {failed}");
            Console.WriteLine($"{"".PadLeft(40, '_')}");
        }

        async Task RunTest(MethodInfo m, object[] par, object obj, List<MethodInfo> befores, List<MethodInfo> afters)
        {
            total++;
            var name = m.Name + (par != null ? $" ({string.Join(", ", par ?? Array.Empty<object>())})" : "");
            Console.Write($"  {name}... ");

            try
            {
                foreach (var b in befores) b.Invoke(obj, null);

                if (m.ReturnType == typeof(Task) ||
                    (m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)))
                {
                    await (Task)m.Invoke(obj, par);
                }
                else
                {
                    m.Invoke(obj, par);
                }

                foreach (var a in afters) a.Invoke(obj, null);

                Console.WriteLine("OK");
                passed++;
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException ?? ex;
                failed++;
                Console.WriteLine("ПРОВАЛ");

                var msg = inner.Message;
                if (msg.Contains("не True"))
                    Console.WriteLine($"    Ожидалось: true, но получили false");
                else if (msg.Contains("не False"))
                    Console.WriteLine($"    Ожидалось: false, но получили true");
                else if (msg.Contains("не Null"))
                    Console.WriteLine($"    Ожидалось: null, но получили значение");
                else if (msg.Contains("Null"))
                    Console.WriteLine($"    Ожидалось: не null, но получили null");
                else if (msg.Contains("!="))
                    Console.WriteLine($"    {msg}");
                else if (msg.Contains("нет в списке"))
                    Console.WriteLine($"    {msg}");
                else
                    Console.WriteLine($"    {msg}");
            }
        }
    }
}