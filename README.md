# Calendar console application

## Application flow:
Console application accepts the following commands from the command line (all commands are case-insensitive):
- `ADD DD/MM hh:mm` to add an appointment
- `DELETE DD/MM` to remove an appointment
- `FIND DD/MM` to find a free timeslot for the day
- `KEEP hh:mm keep a timeslot for any day`

## Prerequisites
- [.Net 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

NB. all external dependencies are automatically restored during the build

## Source:
- `Source.Console` main project that should be set as a Startup
- `Source.Business` project containing business logic
- `Source.Testing` project containing unit tests
- `Blank.Database`solution folder includes a blank copy of the SQL Server database with the corresponding structure
	- the `TAL.Calendar.bak` needs to be restored in your instance of SQL Server or SQL Server express
	- the `ConnectionString` setting needs to be changed accordingly to point to the right database

## Areas of improvement
- Better exception handling, including typed exceptions
- Introduce database migrations
- Move overlaping checks to the database
- Add funtionality:
	- Ability to record the person requesting appointment
	- Ability to request appointment with a specific provider
	- Ability to see all appointments for a person
	- Ability to see all appointments for a specific provider
