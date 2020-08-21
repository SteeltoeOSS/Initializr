using Xunit;

namespace Steeltoe.Initializr.TemplateEngine.Test
{
    public class ConstantsTests
    {
        [Fact]
        public void Steeltoe24()
        {
            Assert.Equal("2.4.4", Constants.Steeltoe24);
        }

        [Fact]
        public void Steeltoe30()
        {
            Assert.Equal("3.0.0", Constants.Steeltoe30);
        }

        [Fact]
        public void NetCoreApp21()
        {
            Assert.Equal("netcoreapp2.1", Constants.NetCoreApp21);
        }

        [Fact]
        public void NetCoreApp31()
        {
            Assert.Equal("netcoreapp3.1", Constants.NetCoreApp31);
        }
    }
}
