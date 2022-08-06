using System;

namespace RoslynTester.Helpers.Testing
{
    internal static class Assert
    {
        public static void AreEqual(string expected, string actual, string message)
        {
            if (!expected.Equals(actual))
            {
                var expectedEscaped = expected.Replace("\n", "\\n").Replace("\r\n", "\\r\\n");
                var actualEscaped = actual.Replace("\n", "\\n").Replace("\r\n", "\\r\\n");
                throw new AssertionException($"{Environment.NewLine}{message}{Environment.NewLine}" +
                                             $"Expected: {expectedEscaped}{Environment.NewLine}" +
                                             $"  Actual: {actualEscaped}");
            }
        }

        public static void Fail(string message) => throw new AssertionException(message);
    }
}