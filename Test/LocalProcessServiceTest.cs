using System.Diagnostics;
using FluentAssertions;
using PortForwardingManager;
using Xunit;

namespace Test
{
    public class LocalProcessServiceTest
    {
        private LocalProcessServiceImpl localProcessService = new LocalProcessServiceImpl();

        [Fact]
        public void GetProcessesByName()
        {
            localProcessService.GetProcessesByName("smss").Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Start()
        {
            Process actual = localProcessService.Start("winver.exe", "");
            actual.Should().NotBeNull();
            actual.Id.Should().BePositive();
            actual.Kill();
        }
    }
}