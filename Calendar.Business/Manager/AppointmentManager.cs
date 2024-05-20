using Calendar.Business.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Calendar.Business.Manager;

public class AppointmentManager(
	ILogger<AppointmentManager> logger,
	CalendarContext context
)
	: IAppointmentManager
{
	public async Task<Appointment> CreateAsync(DateTime startTime, int durationMinutes = Constants.DEFAULT_DURATION_MINUTES)
	{
		if (!IsValidDuration(durationMinutes))
		{
			logger.LogError($"Invalid {nameof(Appointment)}'s duration minutes: {{durationMinutes}}.", durationMinutes);

			throw new ApplicationException($"Invalid {nameof(Appointment)}'s duration minutes.");
		}
		TimeSpan duration = TimeSpan.FromMinutes(durationMinutes);
		if (!IsValid(startTime, duration))
		{
			logger.LogError($"Invalid {nameof(Appointment)}'s start time: {{startTime}} and/or duration: {{duration}}.", startTime, duration);

			throw new ApplicationException($"Invalid {nameof(Appointment)}'s start time and/or duration.");
		}

		var (date, start) = SplitDateTime(startTime);

		List<Appointment> dateAppointments = await context.Appointments.Where(app => app.Date == date).ToListAsync();
		Appointment? existing = dateAppointments.FirstOrDefault(app => app.Start == start && app.Duration == duration);
		List<KeptSlot> slots = await context.KeptSlots.ToListAsync();
		if (existing != null)
		{
			logger.LogWarning($"{nameof(Appointment)} with the same start time: {{startTime}} and {{duration}} already exists.", startTime, duration);

			return existing;
		}
		//	ensure there's no overlap with kept slots:
		else if (slots.Any(slot => HasOverlap(slot.Start, slot.Duration, start, duration)))
		{
			logger.LogError($"{nameof(Appointment)} with start time: {{startTime}} and duration: {{duration}} overlaps with kept slot.", startTime, duration);

			throw new ApplicationException($"{nameof(Appointment)} overlaps with another one.");
		}
		//	ensure there's no overlap with other appointments:
		else if (dateAppointments.Any(app => HasOverlap(app.Start, app.Duration, start, duration)))
		{
			logger.LogError($"{nameof(Appointment)} overlaps with the one that starts on: {{startTime}}.", startTime);

			throw new ApplicationException($"{nameof(Appointment)} overlaps with another one.");
		}
		//	all good - create one:
		else
		{
			var newAppointment = new Appointment
			{
				Date = date,
				Start = start,
				Duration = duration,
			};
			await context.Appointments.AddAsync(newAppointment);
			await context.SaveChangesAsync();

			return newAppointment;
		}
	}

	public async Task<bool> DeleteAsync(DateTime startTime)
	{
		var (date, start) = SplitDateTime(startTime);
		Appointment? appointment = await context.Appointments.FirstOrDefaultAsync(app => app.Date == date && app.Start == start);
		if (appointment != null)
		{
			context.Appointments.Remove(appointment);
			context.SaveChanges();
			return true;
		}
		else
		{
			logger.LogWarning($"Attempted to delete non-existant {nameof(Appointment)} with start time: {{startTime}}", startTime);

			return false;
		}
	}

	public async Task<List<Appointment>> GetAvailableAsync(DateOnly date, int maxSlots = -1)
	{
		List<Appointment> dateAppointments = await context.Appointments.Where(app => app.Date == date).ToListAsync();
		List<KeptSlot> slots = await context.KeptSlots.ToListAsync();

		IEnumerable<Appointment> available = GenerateNineToFive(date)
			.Where(app =>
				IsValid(app.Date.ToDateTime(app.Start), app.Duration) &&
				!slots.Any(slot => HasOverlap(app.Start, app.Duration, slot.Start, slot.Duration)) &&
				!dateAppointments.Any(exist => HasOverlap(app.Start, app.Duration, exist.Start, exist.Duration))
			)
		;

		if (maxSlots > 0)
		{
			available = available.Take(maxSlots);
		}

		return available.ToList();
	}

	public async Task<bool> KeepAsync(TimeOnly start, int durationMinutes = Constants.DEFAULT_DURATION_MINUTES)
	{
		if (!IsValidDuration(durationMinutes))
		{
			logger.LogError($"Invalid {nameof(KeptSlot)}'s duration minutes: {{durationMinutes}}.", durationMinutes);

			throw new ApplicationException($"Invalid {nameof(KeptSlot)}'s duration minutes.");
		}
		TimeSpan duration = TimeSpan.FromMinutes(durationMinutes);
		TimeOnly finishTime = start.Add(duration);
		//	The acceptable time is between 9AM and 5PM
		if (start.Hour < 9 || finishTime > new TimeOnly(17, 0))
		{
			logger.LogError($"Invalid {nameof(KeptSlot)}'s start time: {{start}} and/or duration: {{duration}}.", start, duration);

			throw new ApplicationException($"Invalid {nameof(KeptSlot)}'s start time and/or duration.");
		}
		//	check if we already have this slot:
		List<KeptSlot> slots = await context.KeptSlots.ToListAsync();
		List<Appointment> appointments = await context.Appointments.ToListAsync();
		if (slots.Any(slot => slot.Start == start && slot.Duration == duration))
		{
			logger.LogWarning($"{nameof(KeptSlot)} with the same start time: {{start}} already exists.", start);

			return false;
		}
		//	check overlaps with other existing slots:
		else if (slots.Any(slot => HasOverlap(start, duration, slot.Start, slot.Duration)))
		{
			logger.LogError($"{nameof(KeptSlot)} overlaps with the one that starts on: {{start}}.", start);

			throw new ApplicationException($"{nameof(KeptSlot)} overlaps with another one.");
		}
		//	check if we have any existing appointments already allocated for the slot:
		else if (appointments.Any(app => HasOverlap(start, duration, app.Start, app.Duration)))
		{
			logger.LogError($"{nameof(KeptSlot)} that starts on: {{start}} overlaps with the existing {nameof(Appointment)}.", start);

			throw new ApplicationException($"{nameof(KeptSlot)} overlaps with the existing {nameof(Appointment)}.");
		}
		//	all good - add one:
		else
		{
			var newKeptSlot = new KeptSlot
			{
				Start = start,
				Duration = duration,
			};
			await context.AddAsync(newKeptSlot);
			await context.SaveChangesAsync();

			return true;
		}
	}

	#region '	private methods
	private static (DateOnly date, TimeOnly time) SplitDateTime(DateTime startTime)
	{
		return (
			DateOnly.FromDateTime(startTime),
			TimeOnly.FromDateTime(startTime)
		);
	}

	private static List<Appointment> GenerateNineToFive(DateOnly date)
	{
		var result = new List<Appointment>();
		int id = 0;
		TimeSpan duration = TimeSpan.FromMinutes(30);
		DateTime startDate = date.ToDateTime(TimeOnly.MinValue);
		for (DateTime start = startDate.AddHours(9); start.Add(duration) <= startDate.AddHours(17); start = start.Add(duration))
		{
			result.Add(new Appointment
			{
				Id = --id,
				Date = DateOnly.FromDateTime(start),
				Start = TimeOnly.FromDateTime(start),
				Duration = duration,
			});
		}

		return result;
	}

	private static bool HasOverlap(TimeOnly start1, TimeSpan duration1, TimeOnly start2, TimeSpan duration2)
	{
		return start1.Add(duration1) > start2 && start1 < start2.Add(duration2);
	}

	private static bool IsValidDuration(int durationMinutes)
	{
		return durationMinutes > 0 && durationMinutes <= 8 * 60;    //	8 hours
	}

	private static bool IsValid(DateTime startTime, TimeSpan duration)
	{
		DateTime finishTime = startTime.Add(duration);

		//	The acceptable time is between 9AM and 5PM
		if (startTime.Hour < 9 || finishTime > startTime.Date.AddHours(17))
		{
			return false;
		}
		//	Except from 4 PM to 5 PM on each second day of the third week of any month - this must be reserved and unavailable
		DayOfWeek dayOfWeek = startTime.DayOfWeek;
		int weekOfMonth = (startTime.Day + ((int)dayOfWeek)) / 7 + 1;
		if (weekOfMonth == 3 && dayOfWeek == DayOfWeek.Tuesday &&
			finishTime > finishTime.Date.AddHours(16)
		)
		{
			return false;
		}

		return true;
	}
	#endregion
}
