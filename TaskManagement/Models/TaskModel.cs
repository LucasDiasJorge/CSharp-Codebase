using System;
using System.Collections.Generic;
using TaskManagement.Enums;
using TaskStatus = TaskManagement.Enums.TaskStatus;

public class TaskModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Category { get; set; }
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime DueDate { get; set; }
}