using Calendar.Business.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Business;

public class CalendarContext(DbContextOptions options)
	: DbContext(options)
{
	public DbSet<Appointment> Appointments { get; set; } = null!;

	public DbSet<KeptSlot> KeptSlots { get; set; } = null!;
}
