using System;
using System.Collections.Generic;
using System.Linq;
using TaskManagement.Enums;
using TaskStatus = TaskManagement.Enums.TaskStatus;

class Program
{
    static void Main()
    {
        List<TaskModel> tasks = new List<TaskModel>
        {
            new TaskModel { Id = 1, Title = "Fix login bug", Category = "Development", Status = TaskStatus.InProgress, Priority = TaskPriority.High, DueDate = DateTime.Today.AddDays(3) },
            new TaskModel { Id = 2, Title = "Design homepage", Category = "Design", Status = TaskStatus.Pending, Priority = TaskPriority.Medium, DueDate = DateTime.Today.AddDays(5) },
            new TaskModel { Id = 3, Title = "Write API docs", Category = "Documentation", Status = TaskStatus.Completed, Priority = TaskPriority.Low, DueDate = DateTime.Today },
            new TaskModel { Id = 4, Title = "Database migration", Category = "Development", Status = TaskStatus.Pending, Priority = TaskPriority.High, DueDate = DateTime.Today.AddDays(2) }
        };

        // 1️⃣ Get All Pending Tasks
        var pendingTasks = tasks.Where(t => t.Status == TaskStatus.Pending);
        Console.WriteLine("Pending Tasks:");
        foreach (var task in pendingTasks)
            Console.WriteLine($"- {task.Title} (Due: {task.DueDate.ToShortDateString()})");
        
        // 2️⃣ Get High-Priority Tasks Ordered by Due Date
        var highPriorityTasks = tasks
            .Where(t => t.Priority == TaskPriority.High)
            .OrderBy(t => t.DueDate);
        Console.WriteLine("\nHigh-Priority Tasks Ordered by Due Date:");
        foreach (var task in highPriorityTasks)
            Console.WriteLine($"- {task.Title} (Due: {task.DueDate.ToShortDateString()})");

        // 3️⃣ Search Tasks by Title (Contains a Keyword)
        string keyword = "API";
        var searchResults = tasks.Where(t => t.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        Console.WriteLine($"\nSearch Results for '{keyword}':");
        foreach (var task in searchResults)
            Console.WriteLine($"- {task.Title}");

        // 4️⃣ Group Tasks by Category
        var groupedByCategory = tasks.GroupBy(t => t.Category);
        Console.WriteLine("\nTasks Grouped by Category:");
        foreach (var group in groupedByCategory)
        {
            Console.WriteLine($"\n📂 {group.Key}:");
            foreach (var task in group)
                Console.WriteLine($"- {task.Title}");
        }

        // 5️⃣ Count Tasks per Status
        var taskCounts = tasks.GroupBy(t => t.Status)
                              .Select(g => new { Status = g.Key, Count = g.Count()});
        Console.WriteLine("\nTask Counts per Status:");
        foreach (var entry in taskCounts)
            Console.WriteLine($"- {entry.Status}: {entry.Count}");

        // 6️⃣ Check If Any Task is Overdue
        bool hasOverdueTasks = tasks.Any(t => t.DueDate < DateTime.Today);
        Console.WriteLine($"\nAre there overdue tasks? {(hasOverdueTasks ? "Yes" : "No")}");

        // 7️⃣ Get the First Task That Needs Immediate Attention
        var urgentTask = tasks.OrderBy(t => t.DueDate).FirstOrDefault();
        Console.WriteLine("\nUrgent Task:");
        Console.WriteLine(urgentTask != null ? $"- {urgentTask.Title} (Due: {urgentTask.DueDate.ToShortDateString()})" : "No urgent tasks!");
    }
}
