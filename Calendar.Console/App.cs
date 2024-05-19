using Calendar.Business.Manager;
using Calendar.Business.Model;
using System.Text.RegularExpressions;

namespace Calendar.Console;

internal partial class App(
	IAppointmentManager appointmentManager
)
{
	public async Task RunAsync()
	{
		WriteLineToConsole("Welcome to the Calendar console!");
		WriteLineToConsole("");

		while (true)
		{
			string? input = System.Console.ReadLine();
			if (string.IsNullOrEmpty(input))
			{
				continue;
			}
			if (QuitRegex().IsMatch(input))
			{
				break;
			}

			Match match;
			if (
				(match = AddOrDeleteRegex().Match(input)).Success &&
				TryParseDateTime(match, out DateTime startTime)
			)
			{
				if (input.StartsWith("add", StringComparison.OrdinalIgnoreCase))
				{
					await TryExecute(
						appointmentManager.CreateAsync(startTime),
						"Successfully created"
					);
				}
				else
				{
					await TryExecute(
						appointmentManager.DeleteAsync(startTime),
						"Successfully deleted"
					);

				}
			}
			else if (
				(match = FindRegex().Match(input)).Success &&
				TryParseDate(match, out DateOnly date)
			)
			{
				await TryExecute(
					Task.Run(async () =>
					{
						Appointment? appointment = (await appointmentManager.GetAvailableAsync(date, 1)).FirstOrDefault();

						if (appointment != null)
						{
							WriteLineToConsole($"The first available appointment starts on {appointment.Start:HH:mm}.");
						}
						else
						{
							WriteLineToConsole("No appointments available on the day.");
						}
					})
				);
			}
			else if (
				(match = KeepRegex().Match(input)).Success &&
				TryParseTime(match, out TimeOnly start)
			)
			{
				await TryExecute(appointmentManager.KeepAsync(start),
					"Successfully set keep slot"
				);
			}
		}
	}

	private async static Task TryExecute<T>(Task<T> task, string? successText = null)
	{
		try
		{
			await task;
			if (!string.IsNullOrEmpty(successText))
			{
				WriteLineToConsole(successText);
			}
		}
		catch (Exception ex)
		{
			WriteLineToConsoleError(ex.Message);
		}
	}
	private async static Task TryExecute(Task task, string? successText = null)
	{
		try
		{
			await task;
			if (!string.IsNullOrEmpty(successText))
			{
				WriteLineToConsole(successText);
			}
		}
		catch (Exception ex)
		{
			WriteLineToConsoleError(ex.Message);
		}
	}

	private static void WriteLineToConsole(string value)
		=> System.Console.WriteLine(value);
	private static void WriteLineToConsoleError(string value)
		=> System.Console.Error.WriteLine(value);

	private static bool TryParseDateTime(Match match, out DateTime result)
	{
		var groups = match.Groups;
		string toParse = $"{groups["day"]}/{groups["month"]}/{DateTime.Today.Year} {groups["hours"]}:{groups["minutes"]}";
		return DateTime.TryParse(toParse, out result);
	}

	private static bool TryParseDate(Match match, out DateOnly result)
	{
		var groups = match.Groups;
		string toParse = $"{groups["day"]}/{groups["month"]}/{DateTime.Today.Year}";
		return DateOnly.TryParse(toParse, out result);
	}

	private static bool TryParseTime(Match match, out TimeOnly result)
	{
		var groups = match.Groups;
		string toParse = $"{groups["hours"]}:{groups["minutes"]}";
		return TimeOnly.TryParse(toParse, out result);
	}
}

partial class App
{
	[GeneratedRegex("quit", RegexOptions.IgnoreCase, "en-AU")]
	private static partial Regex QuitRegex();

	[GeneratedRegex(@"(add|delete) (?<day>\d{2})/(?<month>\d{2}) (?<hours>\d{2}):(?<minutes>\d{2})", RegexOptions.IgnoreCase)]
	private static partial Regex AddOrDeleteRegex();

	[GeneratedRegex(@"find (?<day>\d{2})/(?<month>\d{2})", RegexOptions.IgnoreCase)]
	private static partial Regex FindRegex();

	[GeneratedRegex(@"keep (?<hours>\d{2}):(?<minutes>\d{2})", RegexOptions.IgnoreCase)]
	private static partial Regex KeepRegex();
}
