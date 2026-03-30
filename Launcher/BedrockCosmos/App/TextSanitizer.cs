using System.Text;

namespace BedrockCosmos.App
{
    internal static class TextSanitizer
    {
        internal static string ReplaceInvalidUnicode(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var builder = new StringBuilder(value.Length);

            for (int index = 0; index < value.Length; index++)
            {
                char current = value[index];

                if (char.IsHighSurrogate(current))
                {
                    if (index + 1 < value.Length && char.IsLowSurrogate(value[index + 1]))
                    {
                        builder.Append(current);
                        builder.Append(value[index + 1]);
                        index++;
                    }
                    else
                    {
                        builder.Append('\uFFFD');
                    }

                    continue;
                }

                if (char.IsLowSurrogate(current))
                {
                    builder.Append('\uFFFD');
                    continue;
                }

                builder.Append(current);
            }

            return builder.ToString();
        }
    }
}
