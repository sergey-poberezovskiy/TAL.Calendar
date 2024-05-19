using Calendar.Business;
using Microsoft.EntityFrameworkCore;

namespace Calendar.Testing;

public class DatabaseFixture
	: IDisposable
{

	public DatabaseFixture()
	{
		var options = new DbContextOptionsBuilder<CalendarContext>()
					.UseInMemoryDatabase("in-memory")
					.Options;
		Context = new CalendarContext(options);
	}

	public CalendarContext Context { get; private set; }

	private bool _disposedValue;

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Context.Dispose();
			}

			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
