using System.Diagnostics;
using FluentAssertions;
using PortForwardingManager;
using Xunit;

namespace Test {

    public class LocalProcessServiceTest {

        private readonly LocalProcessServiceImpl localProcessService = new LocalProcessServiceImpl();

        [Fact]
        public void getProcessesByName() {
            localProcessService.getProcessesByName("smss").Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void start() {
            Process actual = localProcessService.start("winver.exe", "");
            actual.Should().NotBeNull();
            actual.Id.Should().BePositive();
            actual.Kill();
        }

    }

}