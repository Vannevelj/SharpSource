using System;
using System.Diagnostics;

namespace RoslynTester.Helpers.Testing
{
    /// <summary>
    ///     An expected outcome is different from the actual outcome.
    /// </summary>
    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message)
        {
        }
    }
}