using System;
using TestingLibrary;
using CalculatorApp;

namespace CalculatorTests
{
    public class StatisticsTests
    {
        [Test]
        public void CompletionRate()
        {
            var mgr = new TaskManager();
            mgr.AddTask("Задача 1");
            mgr.AddTask("Задача 2");
            mgr.AddTask("Задача 3");

            var t = mgr.GetTask(1);
            mgr.CompleteTask(t.Id);

            var tasks = mgr.GetAllTasks();
            double rate = Statistics.GetCompletionRate(tasks);
            Check.Eq(33.33333, rate);
        }

        [Test]
        public void AvgPriority()
        {
            var mgr = new TaskManager();
            mgr.AddTask("Задача 1", priority: 1);
            mgr.AddTask("Задача 2", priority: 3);
            mgr.AddTask("Задача 3", priority: 5);

            var tasks = mgr.GetAllTasks();
            double avg = Statistics.GetAveragePriority(tasks);
            Check.Eq(3.0, avg);
        }

        [Test]
        public void PriorityDist()
        {
            var mgr = new TaskManager();
            mgr.AddTask("Задача 1", priority: 3);
            mgr.AddTask("Задача 2", priority: 3);
            mgr.AddTask("Задача 3", priority: 5);

            var tasks = mgr.GetAllTasks();
            var dist = Statistics.GetPriorityDistribution(tasks);

            Check.Eq(2, dist[3]);
            Check.Eq(1, dist[5]);
        }

        [Test]
        public void EmptyList()
        {
            var empty = new System.Collections.Generic.List<TaskManager.TodoTask>();

            try
            {
                Statistics.GetCompletionRate(empty);
                Check.True(false);
            }
            catch (InvalidOperationException)
            {
                Check.True(true);
            }

            try
            {
                Statistics.GetAveragePriority(empty);
                Check.True(false);
            }
            catch (InvalidOperationException)
            {
                Check.True(true);
            }
        }
    }
}