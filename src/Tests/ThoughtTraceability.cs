using FluentAssertions;
using Tests.Mocks;
using ThoughtSharp.Runtime;

namespace Tests
{
    [TestClass]
    public sealed class ThoughtTraceability
    {
        [TestMethod]
        public void CarriesPayload()
        {
            var Expected = new MockProduct();
            
            var T = Thought.ForProduct(Expected);

            var Actual = T.Unwrap();

            Actual.Should().BeSameAs(Expected);
        }
    }
}
