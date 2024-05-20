using System.ComponentModel.DataAnnotations;

namespace Calendar.Business.Model;

/// <summary>
/// Slot to always keep free from appointments
/// </summary>
public class KeptSlot
{
	[Key]
	public TimeOnly Start { get; set; }
	public TimeSpan Duration { get; set; }
}
