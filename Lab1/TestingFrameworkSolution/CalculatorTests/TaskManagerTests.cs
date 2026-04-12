using System;
using System.Collections.Generic;
using System.Linq;
using TestingLibrary;
using CalculatorApp;

namespace CalculatorTests
{
    [Category("Core")]
    public class TaskManagerTests
    {
        TaskManager mgr = new TaskManager();

        public static IEnumerable<object[]> TaskDataGenerator()
        {
            yield return new object[] { "Срочно", 5 };
            yield return new object[] { "Обычная", 3 };
            yield return new object[] { "Фон", 1 };
        }

        [Test]
        [TestCaseSource(nameof(TaskDataGenerator))]
        [Priority(1)]
        public void ParametrizedAdd(string title, int priority)
        {
            var t = mgr.AddTask(title, priority: priority);
            Check.Eq(title, t.Title);
            Check.Eq(priority, t.Priority);
        }

        [Test]
        [Category("ExpressionTree")]
        public void ExpressionTest()
        {
            int a = 10;
            int b = 20;
            Check.That(() => a == b);
        }

        [Test]
        [Priority(5)]
        public void HighPriorityTest()
        {
            Check.True(true);
        }
    }
}