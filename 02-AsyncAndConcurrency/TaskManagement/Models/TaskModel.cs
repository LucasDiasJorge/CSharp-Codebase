using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TaskManagement.Enums;
using TaskStatus = TaskManagement.Enums.TaskStatus;

public class TaskModel
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
    public string Category { get; set; }
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime DueDate { get; set; }
}