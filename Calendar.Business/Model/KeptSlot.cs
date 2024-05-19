using System.ComponentModel.DataAnnotations;

namespace Calendar.Business.Model;

public class KeptSlot
{
	[Key]
	public TimeOnly Start { get; set; }
	public TimeSpan Duration { get; set; }
}
