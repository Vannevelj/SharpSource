using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SharpSource.Test.Helpers;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
[SuppressMessage("Correctness", "SS044:An attribute was defined without specifying the [AttributeUsage]", Justification = "Bug, it shouldn't be firing")]
public class BugVerificationTestAttribute : TestMethodAttribute
{
    public string? IssueUrl { get; set; }
}