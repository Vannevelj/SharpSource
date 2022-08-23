using System;
using System.Globalization;
using System.IO;

// https://gist.github.com/haacked/1610603
namespace SharpSource.Test.Helpers.Helpers.Testing
{
    public static class TestHelpers
    {
        public static void ShouldEqualWithDiff(this string actualValue, string expectedValue, string message) => actualValue.ShouldEqualWithDiff(expectedValue, message, DiffStyle.Full, Console.Out);

        public static void ShouldEqualWithDiff(this string actualValue, string expectedValue, string message, DiffStyle diffStyle) => actualValue.ShouldEqualWithDiff(expectedValue, message, diffStyle, Console.Out);

        public static void ShouldEqualWithDiff(this string actualValue, string expectedValue, string message, DiffStyle diffStyle, TextWriter output)
        {
            if (actualValue == null || expectedValue == null)
            {
                Assert.AreEqual(expectedValue, actualValue);
                return;
            }

            if (actualValue.Equals(expectedValue, StringComparison.Ordinal))
            {
                return;
            }

            output.WriteLine("  Idx Expected    Actual");
            output.WriteLine("-------------------------");
            var maxLen = Math.Max(actualValue.Length, expectedValue.Length);
            var minLen = Math.Min(actualValue.Length, expectedValue.Length);
            for (var i = 0; i < maxLen; i++)
            {
                if (diffStyle != DiffStyle.Minimal || i >= minLen || actualValue[i] != expectedValue[i])
                {
                    output.WriteLine("{0} {1,-3} {2,-4} {3,-5}  {4,-4} {5,-3}",
                        i < minLen && actualValue[i] == expectedValue[i] ? " " : "*", // put a mark beside a differing row
                        i, // the index
                        i < expectedValue.Length ? ( (int)expectedValue[i] ).ToString() : "", // character decimal value
                        i < expectedValue.Length ? expectedValue[i].ToSafeString() : "", // character safe string
                        i < actualValue.Length ? ( (int)actualValue[i] ).ToString() : "", // character decimal value
                        i < actualValue.Length ? actualValue[i].ToSafeString() : "" // character safe string
                    );
                }
            }
            output.WriteLine();

            if (expectedValue != actualValue)
            {
                throw new AssertionException($"{Environment.NewLine}{message}{Environment.NewLine}" +
                                             $"Expected: {expectedValue}{Environment.NewLine}" +
                                             $"  Actual: {actualValue}{Environment.NewLine}");
            }
        }

        private static string ToSafeString(this char c)
        {
            if (char.IsControl(c) || char.IsWhiteSpace(c))
            {
                switch (c)
                {
                    case '\r':
                        return @"\r";
                    case '\n':
                        return @"\n";
                    case '\t':
                        return @"\t";
                    case '\a':
                        return @"\a";
                    case '\v':
                        return @"\v";
                    case '\f':
                        return @"\f";
                    default:
                        return string.Format("\\u{0:X};", (int)c);
                }
            }
            return c.ToString(CultureInfo.InvariantCulture);
        }
    }

    public enum DiffStyle
    {
        Full,
        Minimal
    }
}