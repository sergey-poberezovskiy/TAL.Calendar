using Calendar.Business;
using Calendar.Console;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();

IHostEnvironment env = builder.Environment;
env.ContentRootPath = Directory.GetCurrentDirectory();

builder.Configuration
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
;

builder.Services
	.AddDbContext<CalendarContext>(options =>
		options.UseSqlServer(builder.Configuration.GetConnectionString("CalendarContext"))
	)
	.AddCalendarBusiness()
	.AddSingleton<App>()
;

using IHost host = builder.Build();

var app = host.Services.GetService<App>();

if (app != null)
{
	await app.RunAsync();
}
