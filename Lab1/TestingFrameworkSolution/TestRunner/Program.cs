using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TestRunner
{
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("ТЕСТИРОВАНИЕ\n");

            var asm = Assembly.LoadFrom("CalculatorTests.dll");
            var testAttr = asm.GetTypes().First(t => t.Name == "Test");
            var testCaseAttr = asm.GetTypes().FirstOrDefault(t => t.Name == "TestCase");
            var beforeAttr = asm.GetTypes().FirstOrDefault(t => t.Name == "Before");
            var afterAttr = asm.GetTypes().FirstOrDefault(t => t.Name == "After");
            var beforeClassAttr = asm.GetTypes().FirstOrDefault(t => t.Name == "BeforeClass");
            var afterClassAttr = asm.GetTypes().FirstOrDefault(t => t.Name == "AfterClass");
            var sharedAttr = asm.GetTypes().FirstOrDefault(t => t.Name == "Shared");

            var runner = new Runner();
            await runner.Run(asm, testAttr, testCaseAttr, beforeAttr, afterAttr,
                           beforeClassAttr, afterClassAttr, sharedAttr);
        }
    }

    public class Runner
    {
        Dictionary<Type, object> shared = new Dictionary<Type, object>();
        int total = 0, passed = 0, failed = 0;

        public async Task Run(Assembly asm, Type testAttr, Type testCaseAttr, Type beforeAttr,
            Type afterAttr, Type beforeClassAttr, Type afterClassAttr, Type sharedAttr)
        {
            var testClasses = asm.GetTypes().Where(t => t.GetMethods().Any(m => m.GetCustomAttributes(testAttr, false).Any())).ToList();

            foreach (var type in testClasses)
            {
                Console.WriteLine($"\n[{type.Name}]");

                object obj = null;
                if (sharedAttr != null && type.GetCustomAttribute(sharedAttr) != null)
                {
                    if (!shared.ContainsKey(type)) shared[type] = Activator.CreateInstance(type);
                    obj = shared[type];
                }

                var methods = type.GetMethods();
                var beforeClass = methods.FirstOrDefault(m => m.GetCustomAttributes(beforeClassAttr, false).Any());
                var afterClass = methods.FirstOrDefault(m => m.GetCustomAttributes(afterClassAttr, false).Any());
                var befores = methods.Where(m => m.GetCustomAttributes(beforeAttr, false).Any()).ToList();
                var afters = methods.Where(m => m.GetCustomAttributes(afterAttr, false).Any()).ToList();

                beforeClass?.Invoke(obj ?? Activator.CreateInstance(type), null);

                foreach (var method in methods.Where(m => m.GetCustomAttributes(testAttr, false).Any()))
                {
                    var testInstance = obj ?? Activator.CreateInstance(type);
                    var cases = method.GetCustomAttributes(testCaseAttr, false).ToList();

                    if (cases.Any())
                    {
                        foreach (var tc in cases)
                        {
                            var data = testCaseAttr.GetProperty("Data")?.GetValue(tc) as object[];
                            await RunTest(method, data, testInstance, befores, afters);
                        }
                    }
                    else await RunTest(method, null, testInstance, befores, afters);
                }

                afterClass?.Invoke(obj ?? Activator.CreateInstance(type), null);
            }

            Console.WriteLine($"\n________________________________");
            Console.WriteLine($"Всего тестов: {total}");
            Console.WriteLine($"Успешно: {passed}");
            Console.WriteLine($"Провалено: {failed}");
            Console.WriteLine($"________________________________");
        }

        async Task RunTest(MethodInfo m, object[] par, object obj, List<MethodInfo> befores, List<MethodInfo> afters)
        {
            total++;
            var name = m.Name + (par != null ? $" {string.Join(" ", par)}" : "");
            Console.Write($"  {name}... ");

            try
            {
                foreach (var b in befores) b.Invoke(obj, null);

                if (m.ReturnType == typeof(Task) || (m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)))
                    await (Task)m.Invoke(obj, par);
                else m.Invoke(obj, par);

                foreach (var a in afters) a.Invoke(obj, null);

                Console.WriteLine("OK");
                passed++;
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException ?? ex;
                failed++;
                Console.WriteLine("ПРОВАЛ");
                Console.WriteLine($"    {inner.Message}");
            }
        }
    }
}