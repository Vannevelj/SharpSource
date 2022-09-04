using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharpSource.Test.Helpers;

public class BugVerificationTestAttribute : TestMethodAttribute
{
    public string? IssueUrl { get; set; }
}