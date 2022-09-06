using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SharpSource.Test.Helpers;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public class BugVerificationTestAttribute : TestMethodAttribute
{
    public string? IssueUrl { get; set; }
}