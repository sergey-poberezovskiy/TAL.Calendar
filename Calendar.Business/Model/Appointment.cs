using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calendar.Business.Model;

[Index(nameof(Date), nameof(Start), IsUnique = true)]
public class Appointment
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }
	public DateOnly Date { get; set; }
	public TimeOnly Start { get; set; }
	public TimeSpan Duration { get; set; }
}
