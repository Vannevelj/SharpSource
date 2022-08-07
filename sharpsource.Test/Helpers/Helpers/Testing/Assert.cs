using System;
using SharpSource.Tests.Helpers.Helpers.Testing;

namespace RoslynTester.Helpers.Testing
{
    internal static class Assert
    {
        public static void AreEqual(string expected, string actual, string message = "Strings are not equal") => actual.ShouldEqualWithDiff(expected);

        public static void Fail(string message) => throw new AssertionException(message);
    }
}