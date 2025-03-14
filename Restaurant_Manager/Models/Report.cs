using System;
using System.ComponentModel.DataAnnotations;

public class Report
{
    public int Id { get; set; }

    [Required]
    public string ReportType { get; set; } 

    [Required]
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string Data { get; set; } 
}
