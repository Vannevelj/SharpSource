using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SharpSource.Test.Helpers;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class BugVerificationTestAttribute : TestMethodAttribute
{
    public string? IssueUrl { get; set; }
}