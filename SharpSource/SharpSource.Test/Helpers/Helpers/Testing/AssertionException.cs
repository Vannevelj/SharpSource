using System;

namespace SharpSource.Test.Helpers.Helpers.Testing;

/// <summary>
///     An expected outcome is different from the actual outcome.
/// </summary>
public class AssertionException : Exception
{
    public AssertionException(string message) : base(message)
    {
    }
}