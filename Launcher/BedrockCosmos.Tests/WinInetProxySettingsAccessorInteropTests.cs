using System.Reflection;
using System.Runtime.InteropServices;
using BedrockCosmos.Proxy;
using Xunit;

namespace BedrockCosmos.Tests
{
    public class WinInetProxySettingsAccessorInteropTests
    {
        [Theory]
        [InlineData("InternetQueryOption", "InternetQueryOptionW")]
        [InlineData("InternetSetOption", "InternetSetOptionW")]
        public void WinInetImports_ExplicitlyBindToUnicodeEntryPoints(string methodName, string expectedEntryPoint)
        {
            MethodInfo method = typeof(WinInetProxySettingsAccessor).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);

            Assert.NotNull(method);

            var attribute = method.GetCustomAttribute<DllImportAttribute>();

            Assert.NotNull(attribute);
            Assert.Equal(expectedEntryPoint, attribute.EntryPoint);
            Assert.Equal(CharSet.Unicode, attribute.CharSet);
            Assert.True(attribute.ExactSpelling);
        }
    }
}
