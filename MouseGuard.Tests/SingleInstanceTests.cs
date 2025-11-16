using System;
using Xunit;

namespace MouseGuard.Tests
{
    public class SingleInstanceTests
    {
        [Fact]
        public void SecondInstance_DetectsNonPrimary()
        {
            var name = "MouseGuard_Test_" + Guid.NewGuid().ToString();
            using var first = new SingleInstance(name);
            using var second = new SingleInstance(name);
            Assert.True(first.IsFirstInstance);
            Assert.False(second.IsFirstInstance);
        }
    }
}
