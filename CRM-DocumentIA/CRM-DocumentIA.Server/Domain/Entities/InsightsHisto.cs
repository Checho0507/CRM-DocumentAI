using System;
using System.ComponentModel.DataAnnotations;

namespace CRM_DocumentIA.Server.Domain.Entities;
public class InsightsHisto
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    public DateTime Date { get; set; } = DateTime.Now;

    [Required]
    public required string Question { get; set; }

    [Required]
    public required string Answer { get; set; }

    public Usuario Usuario { get; set; }
}
