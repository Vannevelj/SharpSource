namespace SharpSource.Test.Helpers.Helpers.Testing
{
    internal static class Assert
    {
        public static void AreEqual(string expected, string actual, string message = "Strings are not equal") => actual.ShouldEqualWithDiff(expected, message);

        public static void Fail(string message) => throw new AssertionException(message);
    }
}