using BedrockCosmos.App;
using Xunit;

namespace BedrockCosmos.Tests
{
    public class TextSanitizerTests
    {
        [Fact]
        public void ReplaceInvalidUnicode_ReplacesUnpairedSurrogates()
        {
            string input = "before" + '\uDD18' + "middle" + '\uD83D' + "after";

            string sanitized = TextSanitizer.ReplaceInvalidUnicode(input);

            Assert.Equal("before\uFFFDmiddle\uFFFDafter", sanitized);
        }

        [Fact]
        public void ReplaceInvalidUnicode_PreservesValidSurrogatePairs()
        {
            string input = "ok " + char.ConvertFromUtf32(0x1F680);

            string sanitized = TextSanitizer.ReplaceInvalidUnicode(input);

            Assert.Equal(input, sanitized);
        }
    }
}
