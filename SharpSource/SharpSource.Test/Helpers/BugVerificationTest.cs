using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SharpSource.Tests.Helpers
{
    public class BugVerificationTestAttribute : TestMethodAttribute
    {
        public string IssueUrl { get; set; }
    }
}