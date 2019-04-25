using EasyConsole;

namespace GoRogue_PerformanceTests
{
	internal class PerfTests : Program
	{
		public PerfTests()
			: base("GoRogue Performance Tests", true)
		{
			AddPage(new MainPage(this));
			SetPage<MainPage>();
		}
	}
}