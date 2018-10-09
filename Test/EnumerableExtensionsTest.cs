using System.Collections.Generic;
using FluentAssertions;
using PortForwardingManager;
using Xunit;

namespace Test
{
    public class EnumerableExtensionsTest
    {
        [Fact]
        public void SpliceRemoval()
        {
            var original = new[] { 1, 2, 3, 4, 5 };
            IEnumerable<int> actual = original.Splice(1, 3);
            actual.Should().Equal(1, 5);
        }

        [Fact]
        public void SpliceInsertion()
        {
            var original = new[] { 1, 2, 3, 4, 5 };
            IEnumerable<int> actual = original.Splice(1, 0, new[] { 100, 101 });
            actual.Should().Equal(1, 100, 101, 2, 3, 4, 5);
        }

        [Fact]
        public void SpliceReplacement()
        {
            var original = new[] { 1, 2, 3, 4, 5 };
            IEnumerable<int> actual = original.Splice(1, 3, new[] { 1, 2, 3 });
            actual.Should().Equal(1, 1, 2, 3, 5);
        }
    }
}