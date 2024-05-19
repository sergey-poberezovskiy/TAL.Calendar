using Calendar.Business.Model;

namespace Calendar.Business.Manager;

public interface IAppointmentManager
{
	Task<Appointment> CreateAsync(DateTime startTime, int durationMinutes = Constants.DEFAULT_DURATION_MINUTES);

	Task<bool> DeleteAsync(DateTime startTime);

	Task<bool> KeepAsync(TimeOnly start, int durationMinutes = Constants.DEFAULT_DURATION_MINUTES);

	Task<List<Appointment>> GetAvailableAsync(DateOnly date, int maxSlots = -1);
}
