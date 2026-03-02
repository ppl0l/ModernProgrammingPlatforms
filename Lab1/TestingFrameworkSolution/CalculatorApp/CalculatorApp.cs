using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CalculatorApp
{
    public class TaskManager
    {
        private List<TodoTask> tasks = new List<TaskManager.TodoTask>();
        private int nextId = 1;

        public class TodoTask
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public bool IsCompleted { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? DueDate { get; set; }
            public int Priority { get; set; }

            public override string ToString() => $"[{(IsCompleted ? "x" : " ")}] {Id}: {Title}";
        }

        public TodoTask AddTask(string title, string description = "", int priority = 3, DateTime? dueDate = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Заголовок не может быть пустым");

            if (priority < 1 || priority > 5)
                throw new ArgumentException("Приоритет должен быть от 1 до 5");

            var task = new TodoTask
            {
                Id = nextId++,
                Title = title,
                Description = description,
                IsCompleted = false,
                CreatedAt = DateTime.Now,
                DueDate = dueDate,
                Priority = priority
            };

            tasks.Add(task);
            return task;
        }

        public bool CompleteTask(int id)
        {
            var task = GetTask(id);
            if (task == null) return false;

            task.IsCompleted = true;
            return true;
        }

        public bool DeleteTask(int id)
        {
            var task = GetTask(id);
            if (task == null) return false;

            return tasks.Remove(task);
        }

        public TodoTask GetTask(int id) => tasks.FirstOrDefault(t => t.Id == id);

        public List<TodoTask> GetAllTasks() => tasks.ToList();

        public List<TodoTask> GetPendingTasks() => tasks.Where(t => !t.IsCompleted).ToList();

        public List<TodoTask> GetCompletedTasks() => tasks.Where(t => t.IsCompleted).ToList();

        public List<TodoTask> GetTasksByPriority(int minPriority) =>
            tasks.Where(t => t.Priority >= minPriority && !t.IsCompleted).ToList();

        public List<TodoTask> GetOverdueTasks() =>
            tasks.Where(t => !t.IsCompleted && t.DueDate.HasValue && t.DueDate.Value < DateTime.Now).ToList();

        public bool UpdateTaskPriority(int id, int newPriority)
        {
            if (newPriority < 1 || newPriority > 5)
                throw new ArgumentException("Приоритет должен быть от 1 до 5");

            var task = GetTask(id);
            if (task == null) return false;

            task.Priority = newPriority;
            return true;
        }

        public async Task<TodoTask> AddTaskAsync(string title, string description = "", int priority = 3)
        {
            await Task.Delay(50);
            return AddTask(title, description, priority);
        }

        public async Task<List<TodoTask>> SearchTasksAsync(string searchTerm)
        {
            await Task.Delay(30);
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<TodoTask>();

            return tasks.Where(t =>
                t.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public int TaskCount => tasks.Count;
        public int PendingCount => tasks.Count(t => !t.IsCompleted);
        public int CompletedCount => tasks.Count(t => t.IsCompleted);
    }

    public class Statistics
    {
        public static double GetCompletionRate(List<TaskManager.TodoTask> tasks)
        {
            if (tasks == null || tasks.Count == 0)
                throw new InvalidOperationException("Нет данных для расчета");

            return (double)tasks.Count(t => t.IsCompleted) / tasks.Count * 100;
        }

        public static double GetAveragePriority(List<TaskManager.TodoTask> tasks)
        {
            if (tasks == null || tasks.Count == 0)
                throw new InvalidOperationException("Нет данных для расчета");

            return tasks.Average(t => t.Priority);
        }

        public static Dictionary<int, int> GetPriorityDistribution(List<TaskManager.TodoTask> tasks)
        {
            return tasks.GroupBy(t => t.Priority)
                       .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}