using EasyConsole;

namespace GoRogue_PerformanceTests
{
    class PerfTests : EasyConsole.Program
    {
        public PerfTests()
            : base("GoRogue Performance Tests", true)
        {
            AddPage(new MainPage(this));
            SetPage<MainPage>();
        }
    }
}
