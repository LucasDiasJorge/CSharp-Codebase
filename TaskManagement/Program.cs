using System;
using System.Collections.Generic;
using System.Linq;
using TaskManagement.Enums;
using TaskStatus = TaskManagement.Enums.TaskStatus;
class Program
{
    static void Main()
    {
        using (var db = new AppDbContext())
        {
            var task1 = new TaskModel
            {
                Title = "Fix login bug",
                Category = "Development",
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High,
                DueDate = DateTime.UtcNow.AddDays(3) // Convert to UTC
            };

            var task2 = new TaskModel
            {
                Title = "Design homepage",
                Category = "Design",
                Status = TaskStatus.Pending,
                Priority = TaskPriority.Medium,
                DueDate = DateTime.UtcNow.AddDays(5) // Convert to UTC
            };

            var task3 = new TaskModel
            {
                Title = "Write API docs",
                Category = "Documentation",
                Status = TaskStatus.Completed,
                Priority = TaskPriority.Low,
                DueDate = DateTime.UtcNow // Convert to UTC
            };

            var task4 = new TaskModel
            {
                Title = "Database migration",
                Category = "Development",
                Status = TaskStatus.Pending,
                Priority = TaskPriority.High,
                DueDate = DateTime.UtcNow.AddDays(2) // Convert to UTC
            };

            // Add the tasks to the database
            db.Tasks.AddRange(task1, task2, task3, task4);
            db.SaveChanges();  // Save the tasks to the database

            // 2️⃣ Get All Pending Tasks
            var pendingTasks = db.Tasks.Where(t => t.Status == TaskStatus.Pending).ToList();
            Console.WriteLine("Pending Tasks:");
            foreach (var task in pendingTasks)
                Console.WriteLine($"- {task.Title} (Due: {task.DueDate.ToShortDateString()})");

            // 3️⃣ Get High-Priority Tasks Ordered by Due Date
            var highPriorityTasks = db.Tasks
                .Where(t => t.Priority == TaskPriority.High)
                .OrderBy(t => t.DueDate)
                .ToList();
            Console.WriteLine("\nHigh-Priority Tasks Ordered by Due Date:");
            foreach (var task in highPriorityTasks)
                Console.WriteLine($"- {task.Title} (Due: {task.DueDate.ToShortDateString()})");

            // 4️⃣ Search Tasks by Title (Contains a Keyword)
            string keyword = "API";
            var searchResults = db.Tasks
                .AsEnumerable()  // This forces the query to execute in memory
                .Where(t => t.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Console.WriteLine($"\nSearch Results for '{keyword}':");
            foreach (var task in searchResults)
                Console.WriteLine($"- {task.Title}");

            // 5️⃣ Group Tasks by Category
            var groupedByCategory = db.Tasks.GroupBy(t => t.Category).ToList();
            Console.WriteLine("\nTasks Grouped by Category:");
            foreach (var group in groupedByCategory)
            {
                Console.WriteLine($"\n📂 {group.Key}:");
                foreach (var task in group)
                    Console.WriteLine($"- {task.Title}");
            }

            // 6️⃣ Count Tasks per Status
            var taskCounts = db.Tasks
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();
            Console.WriteLine("\nTask Counts per Status:");
            foreach (var entry in taskCounts)
                Console.WriteLine($"- {entry.Status}: {entry.Count}");

            // 7️⃣ Check If Any Task is Overdue
            bool hasOverdueTasks = db.Tasks.Any(t => t.DueDate < DateTime.UtcNow);
            Console.WriteLine($"\nAre there overdue tasks? {(hasOverdueTasks ? "Yes" : "No")}");

            // 8️⃣ Get the First Task That Needs Immediate Attention
            var urgentTask = db.Tasks.OrderBy(t => t.DueDate).FirstOrDefault();
            Console.WriteLine("\nUrgent Task:");
            Console.WriteLine(urgentTask != null ? $"- {urgentTask.Title} (Due: {urgentTask.DueDate.ToShortDateString()})" : "No urgent tasks!");
        }
    }
}