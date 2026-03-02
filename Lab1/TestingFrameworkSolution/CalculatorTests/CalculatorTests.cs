using System;
using System.Threading.Tasks;
using CalculatorApp;

namespace CalculatorTests
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Test : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TestCase : Attribute
    {
        public object[] Data { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class Before : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class After : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class BeforeClass : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class AfterClass : Attribute { }

    [AttributeUsage(AttributeTargets.Class)]
    public class Shared : Attribute { }

    public static class Check
    {
        public static void Equal(object a, object b)
        {
            if (a is double da && b is double db)
            {
                if (Math.Abs(da - db) > 0.0001)
                    throw new Exception($"{da} != {db}");
            }
            else if (!a.Equals(b))
                throw new Exception($"{a} != {b}");
        }

        public static void True(bool x) { if (!x) throw new Exception("не True"); }
        public static void False(bool x) { if (x) throw new Exception("не False"); }
        public static void Null(object x) { if (x != null) throw new Exception("не Null"); }
        public static void NotNull(object x) { if (x == null) throw new Exception("Null"); }

        public static void Contains<T>(T item, System.Collections.Generic.IEnumerable<T> list)
        {
            if (!System.Linq.Enumerable.Contains(list, item))
                throw new Exception("нет в списке");
        }
    }

    [Shared]
    public class TaskManagerTests
    {
        TaskManager manager;

        [BeforeClass]
        public void BeforeAll() { manager = new TaskManager(); }

        [Before]
        public void Before() { }

        [After]
        public void After() { }

        [AfterClass]
        public void AfterAll() { }

        [Test]
        public void AddTaskTest()
        {
            var task = manager.AddTask("Купить продукты", "Молоко, хлеб, яйца", 4);
            Check.Equal("Купить продукты", task.Title);
            Check.Equal("Молоко, хлеб, яйца", task.Description);
            Check.Equal(4, task.Priority);
            Check.False(task.IsCompleted);
            Check.NotNull(task.CreatedAt);
        }

        [Test]
        public void CompleteTaskTest()
        {
            var task = manager.AddTask("Написать отчет");
            Check.True(manager.CompleteTask(task.Id));
            Check.True(task.IsCompleted);
        }

        [Test]
        public void DeleteTaskTest()
        {
            var task = manager.AddTask("Временная задача");
            int initialCount = manager.TaskCount;

            Check.True(manager.DeleteTask(task.Id));
            Check.Equal(initialCount - 1, manager.TaskCount);
            Check.False(manager.DeleteTask(999));
        }

        [TestCase(Data = new object[] { "", "Задача без заголовка" })]
        [TestCase(Data = new object[] { "   ", "Задача из пробелов" })]
        public void AddTaskInvalidTitleTest(string title, string description)
        {
            try
            {
                manager.AddTask(title, description);
                Check.True(false);
            }
            catch (ArgumentException)
            {
                Check.True(true);
            }
        }

        [TestCase(Data = new object[] { 0 })]
        [TestCase(Data = new object[] { 6 })]
        public void AddTaskInvalidPriorityTest(int priority)
        {
            try
            {
                manager.AddTask("Задача", "Описание", priority);
                Check.True(false);
            }
            catch (ArgumentException)
            {
                Check.True(true);
            }
        }

        [Test]
        public void GetTasksByPriorityTest()
        {
            manager.AddTask("Низкий приоритет", priority: 1);
            manager.AddTask("Средний приоритет", priority: 3);
            manager.AddTask("Высокий приоритет", priority: 5);

            var highPriorityTasks = manager.GetTasksByPriority(4);
            Check.Equal(1, highPriorityTasks.Count);
            Check.Equal("Высокий приоритет", highPriorityTasks[0].Title);
        }

        [Test]
        public void UpdateTaskPriorityTest()
        {
            var task = manager.AddTask("Задача для обновления", priority: 2);
            Check.True(manager.UpdateTaskPriority(task.Id, 5));
            Check.Equal(5, task.Priority);

            try
            {
                manager.UpdateTaskPriority(task.Id, 10);
                Check.True(false);
            }
            catch (ArgumentException)
            {
                Check.True(true);
            }
        }

        [Test]
        public async Task AddTaskAsyncTest()
        {
            var task = await manager.AddTaskAsync("Асинхронная задача", "Сделать быстро", 5);
            Check.Equal("Асинхронная задача", task.Title);
            Check.Equal(5, task.Priority);
        }

        [Test]
        public async Task SearchTasksAsyncTest()
        {
            manager.AddTask("Купить молоко");
            manager.AddTask("Купить хлеб");
            manager.AddTask("Позвонить маме");

            var results = await manager.SearchTasksAsync("купить");
            Check.Equal(2, results.Count);
            Check.Contains("Купить молоко", results.Select(t => t.Title));
            Check.Contains("Купить хлеб", results.Select(t => t.Title));
        }

        [Test]
        public void OverdueTasksTest()
        {
            var pastDue = manager.AddTask("Просроченная задача", dueDate: DateTime.Now.AddDays(-2));
            var futureDue = manager.AddTask("Будущая задача", dueDate: DateTime.Now.AddDays(5));

            var overdue = manager.GetOverdueTasks();
            Check.Contains(pastDue, overdue);
            Check.False(overdue.Contains(futureDue));
        }
    }

    public class StatisticsTests
    {
        [Test]
        public void CompletionRateTest()
        {
            var manager = new TaskManager();
            manager.AddTask("Задача 1");
            manager.AddTask("Задача 2");
            manager.AddTask("Задача 3");

            var task = manager.GetTask(1);
            manager.CompleteTask(task.Id);

            var tasks = manager.GetAllTasks();
            double rate = Statistics.GetCompletionRate(tasks);
            Check.Equal(33.33333, rate);
        }

        [Test]
        public void AveragePriorityTest()
        {
            var manager = new TaskManager();
            manager.AddTask("Задача 1", priority: 1);
            manager.AddTask("Задача 2", priority: 3);
            manager.AddTask("Задача 3", priority: 5);

            var tasks = manager.GetAllTasks();
            double avg = Statistics.GetAveragePriority(tasks);
            Check.Equal(3.0, avg);
        }

        [Test]
        public void PriorityDistributionTest()
        {
            var manager = new TaskManager();
            manager.AddTask("Задача 1", priority: 3);
            manager.AddTask("Задача 2", priority: 3);
            manager.AddTask("Задача 3", priority: 5);

            var tasks = manager.GetAllTasks();
            var distribution = Statistics.GetPriorityDistribution(tasks);

            Check.Equal(2, distribution[3]);
            Check.Equal(1, distribution[5]);
        }

        [Test]
        public void EmptyListStatisticsTest()
        {
            var emptyList = new System.Collections.Generic.List<TaskManager.TodoTask>();

            try
            {
                Statistics.GetCompletionRate(emptyList);
                Check.True(false);
            }
            catch (InvalidOperationException)
            {
                Check.True(true);
            }

            try
            {
                Statistics.GetAveragePriority(emptyList);
                Check.True(false);
            }
            catch (InvalidOperationException)
            {
                Check.True(true);
            }
        }
    }

    public class TaskFilterTests
    {
        TaskManager manager;

        [Before]
        public void Before()
        {
            manager = new TaskManager();
            manager.AddTask("Купить молоко");
            manager.AddTask("Купить хлеб");
            manager.AddTask("Позвонить врачу");

            var task = manager.GetTask(1);
            manager.CompleteTask(task.Id);
        }

        [Test]
        public void GetPendingTasksTest()
        {
            var pending = manager.GetPendingTasks();
            Check.Equal(2, pending.Count);
            Check.True(pending.All(t => !t.IsCompleted));
        }

        [Test]
        public void GetCompletedTasksTest()
        {
            var completed = manager.GetCompletedTasks();
            Check.Equal(1, completed.Count);
            Check.True(completed.All(t => t.IsCompleted));
        }

        [Test]
        public void GetAllTasksTest()
        {
            var all = manager.GetAllTasks();
            Check.Equal(3, all.Count);
        }

        [Test]
        public void TaskCountsTest()
        {
            Check.Equal(3, manager.TaskCount);
            Check.Equal(2, manager.PendingCount);
            Check.Equal(1, manager.CompletedCount);
        }

        [Test]
        public void GetNonExistentTaskTest()
        {
            var task = manager.GetTask(999);
            Check.Null(task);
        }
    }
}