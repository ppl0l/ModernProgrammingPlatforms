using System.Linq;
using TestingLibrary;
using CalculatorApp;

namespace CalculatorTests
{
    public class TaskFilterTests
    {
        TaskManager mgr;

        [Before]
        public void Setup()
        {
            mgr = new TaskManager();
            mgr.AddTask("Купить молоко");
            mgr.AddTask("Купить хлеб");
            mgr.AddTask("Позвонить врачу");

            var t = mgr.GetTask(1);
            mgr.CompleteTask(t.Id);
        }

        [Test]
        public void Pending()
        {
            var p = mgr.GetPendingTasks();
            Check.Eq(2, p.Count);
            Check.True(p.All(t => !t.IsCompleted));
        }

        [Test]
        public void Completed()
        {
            var c = mgr.GetCompletedTasks();
            Check.Eq(1, c.Count);
            Check.True(c.All(t => t.IsCompleted));
        }

        [Test]
        public void All()
        {
            var all = mgr.GetAllTasks();
            Check.Eq(3, all.Count);
        }

        [Test]
        public void Counts()
        {
            Check.Eq(3, mgr.TaskCount);
            Check.Eq(2, mgr.PendingCount);
            Check.Eq(1, mgr.CompletedCount);
        }

        [Test]
        public void GetMissing()
        {
            var t = mgr.GetTask(999);
            Check.Null(t);
        }
    }
}