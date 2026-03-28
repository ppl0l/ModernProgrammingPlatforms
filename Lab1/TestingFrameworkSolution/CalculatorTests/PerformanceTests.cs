using System.Threading.Tasks;
using TestingLibrary;
using CalculatorApp;

namespace CalculatorTests
{
    public class PerformanceTests
    {
        [Test]
        public async Task SlowTest1() { await Task.Delay(1000); Check.True(true); }

        [Test]
        public async Task SlowTest2() { await Task.Delay(1000); Check.True(true); }

        [Test]
        [Timeout(500)]
        public async Task TimeoutFailTest() { await Task.Delay(2000); Check.True(true); }
    }
}