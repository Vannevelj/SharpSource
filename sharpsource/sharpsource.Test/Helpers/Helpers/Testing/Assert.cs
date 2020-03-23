using System;

namespace RoslynTester.Helpers.Testing
{
    internal static class Assert
    {
        public static void AreEqual(object expected, object actual, string message)
        {
            if (!expected.Equals(actual))
            {
                throw new AssertionException($"{Environment.NewLine}{message}{Environment.NewLine}" +
                                             $"Expected: {expected}{Environment.NewLine}" +
                                             $"Actual: {actual}");
            }
        }

        public static void Fail(string message)
        {
            throw new AssertionException(message);
        }
    }
}