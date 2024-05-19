using Calendar.Business.Manager;
using Microsoft.Extensions.DependencyInjection;

namespace Calendar.Business;

public static class ServiceExtension
{
	public static IServiceCollection AddCalendarBusiness(this IServiceCollection services)
	{
		services.AddScoped<IAppointmentManager, AppointmentManager>();

		return services;
	}
}
