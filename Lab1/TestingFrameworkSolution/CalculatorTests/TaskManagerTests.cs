using System;
using System.Linq;
using System.Threading.Tasks;
using TestingLibrary;
using CalculatorApp;

namespace CalculatorTests
{
    [Shared]
    public class TaskManagerTests
    {
        TaskManager mgr;

        [BeforeClass]
        public void Init() { mgr = new TaskManager(); }

        [Before]
        public void Setup() { }

        [After]
        public void Cleanup() { }

        [AfterClass]
        public void Done() { }

        [Test]
        public void AddTask()
        {
            var t = mgr.AddTask("Купить продукты", "Молоко, хлеб, яйца", 4);
            Check.Eq("Купить продукты", t.Title);
            Check.Eq("Молоко, хлеб, яйца", t.Description);
            Check.Eq(4, t.Priority);
            Check.False(t.IsCompleted);
            Check.NotNull(t.CreatedAt);
        }

        [Test]
        public void CompleteTask()
        {
            var t = mgr.AddTask("Написать отчет");
            Check.True(mgr.CompleteTask(t.Id));
            Check.True(t.IsCompleted);
        }

        [Test]
        public void DeleteTask()
        {
            var t = mgr.AddTask("Временная задача");
            int cnt = mgr.TaskCount;
            Check.True(mgr.DeleteTask(t.Id));
            Check.Eq(cnt - 1, mgr.TaskCount);
            Check.False(mgr.DeleteTask(999));
        }

        [TestCase(Data = new object[] { "", "Без заголовка" })]
        [TestCase(Data = new object[] { "   ", "Пробелы" })]
        public void AddTaskBadTitle(string title, string desc)
        {
            try
            {
                mgr.AddTask(title, desc);
                Check.True(false);
            }
            catch (ArgumentException)
            {
                Check.True(true);
            }
        }

        [TestCase(Data = new object[] { 0 })]
        [TestCase(Data = new object[] { 6 })]
        public void AddTaskBadPriority(int p)
        {
            try
            {
                mgr.AddTask("Задача", "Описание", p);
                Check.True(false);
            }
            catch (ArgumentException)
            {
                Check.True(true);
            }
        }

        [Test]
        public void GetByPriority()
        {
            mgr.AddTask("Низкий", priority: 1);
            mgr.AddTask("Средний", priority: 3);
            mgr.AddTask("Высокий", priority: 5);

            var high = mgr.GetTasksByPriority(4);
            Check.Eq(1, high.Count);
            Check.Eq("Высокий", high[0].Title);
        }

        [Test]
        public void UpdatePriority()
        {
            var t = mgr.AddTask("Обновить", priority: 2);
            Check.True(mgr.UpdateTaskPriority(t.Id, 5));
            Check.Eq(5, t.Priority);

            try
            {
                mgr.UpdateTaskPriority(t.Id, 10);
                Check.True(false);
            }
            catch (ArgumentException)
            {
                Check.True(true);
            }
        }

        [Test]
        public async Task AddTaskAsync()
        {
            var t = await mgr.AddTaskAsync("Асинхронная", "Быстро", 5);
            Check.Eq("Асинхронная", t.Title);
            Check.Eq(5, t.Priority);
        }

        [Test]
        public async Task SearchAsync()
        {
            mgr.AddTask("Купить молоко");
            mgr.AddTask("Купить хлеб");
            mgr.AddTask("Позвонить маме");

            var res = await mgr.SearchTasksAsync("купить");
            Check.Eq(2, res.Count);
            Check.Contains("Купить молоко", res.Select(t => t.Title));
            Check.Contains("Купить хлеб", res.Select(t => t.Title));
        }

        [Test]
        public void Overdue()
        {
            var past = mgr.AddTask("Просрочка", dueDate: DateTime.Now.AddDays(-2));
            var future = mgr.AddTask("Будущее", dueDate: DateTime.Now.AddDays(5));

            var overdue = mgr.GetOverdueTasks();
            Check.Contains(past, overdue);
            Check.False(overdue.Contains(future));
        }
    }
}