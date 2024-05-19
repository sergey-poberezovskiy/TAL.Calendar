using Calendar.Business;
using Calendar.Business.Manager;
using Calendar.Business.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Calendar.Testing;

public class AppointmentTests
	: IClassFixture<DatabaseFixture>
{
	private readonly CalendarContext _context;
	private readonly IAppointmentManager _manager;

	public AppointmentTests(
		DatabaseFixture fixture
	)
	{
		_context = fixture.Context;
		_manager = new AppointmentManager(Mock.Of<ILogger<AppointmentManager>>(), _context);
	}

	[Fact]
	public async Task SuccessfullyCreateAppointment()
	{
		DateTime startTime = new(2024, 6, 1, 10, 0, 0);
		Appointment? appointment = await _manager.CreateAsync(startTime);

		Assert.NotNull(appointment);
		Assert.Equal(appointment.Date, DateOnly.FromDateTime(startTime));
	}

	[Fact]
	public async Task FailsToCreateOverlappingAppointment()
	{
		DateTime startTime = new(2024, 6, 1, 15, 0, 0);
		Appointment? appointment = await _manager.CreateAsync(startTime);

		Assert.NotNull(appointment);

		await Assert.ThrowsAnyAsync<ApplicationException>(
			async () => _ = await _manager.CreateAsync(startTime, 60)
		);
	}

	[Fact]
	public async Task SuccessfullyDeleteAppointment()
	{
		DateTime startTime = new(2024, 6, 1, 11, 30, 0);
		_ = await _manager.CreateAsync(startTime);

		bool deleted = await _manager.DeleteAsync(startTime);
		Assert.True(deleted);
	}

	[Fact]
	public async Task FailsToDeleteAppointment()
	{
		DateTime startTime = new(2024, 6, 1, 11, 30, 0);
		bool deleted = await _manager.DeleteAsync(startTime);
		Assert.False(deleted);
	}

	[Fact]
	public async Task ReturnsAvailableAppointments()
	{
		DateOnly date = new(2024, 05, 13);
		List<Appointment> appointments = await _manager.GetAvailableAsync(date);
		int fullDayCount = appointments.Count;
		Assert.Equal(16, fullDayCount);
		//	exclude two for the 2nd day of the 3rd week:
		appointments = await _manager.GetAvailableAsync(date.AddDays(1));
		Assert.Equal(appointments.Count, fullDayCount - 2);
	}

	[Fact]
	public async Task SuccessfullyAddsKeptSlot()
	{
		TimeOnly start = new(12, 30, 0);

		bool added = await _manager.KeepAsync(start);
		Assert.True(added);
		//	try to add it again:
		added = await _manager.KeepAsync(start);
		Assert.False(added);
	}

	[Fact]
	public async Task FailsToAddKeptSlotThatOverlapsWithAppointment()
	{
		DateTime startTime = new(2024, 6, 1, 13, 30, 0);
		_ = await _manager.CreateAsync(startTime);

		await Assert.ThrowsAnyAsync<ApplicationException>(
			async () => _ = await _manager.KeepAsync(TimeOnly.FromDateTime(startTime))
		);
	}
}
