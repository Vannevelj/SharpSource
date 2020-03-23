using System;

namespace RoslynTester.Helpers.Testing
{
    /// <summary>
    ///     Indicates the code is in an invalid state
    /// </summary>
    public class InvalidCodeException : Exception
    {
        public InvalidCodeException(string code) : base(code)
        {
        }
    }
}