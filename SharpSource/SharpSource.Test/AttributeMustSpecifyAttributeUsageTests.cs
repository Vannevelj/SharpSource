using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class AttributeMustSpecifyAttributeUsageTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AttributeMustSpecifyAttributeUsageAnalyzer();

    protected override CodeFixProvider CodeFixProvider => new AttributeMustSpecifyAttributeUsageCodeFix();

    [TestMethod]
    public async Task AttributeMustSpecifyAttributeUsage_NoAttribute()
    {
        var original = @"
using System;

class MyAttribute : Attribute
{
}";

        var result = @"
using System;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
class MyAttribute : Attribute
{
}";

        await VerifyDiagnostic(original, "MyAttribute should specify how the attribute can be used");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task AttributeMustSpecifyAttributeUsage_NoAttribute_OtherExisting()
    {
        var original = @"
using System;

[Obsolete]
class MyAttribute : Attribute
{
}";

        var result = @"
using System;

[Obsolete]
[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
class MyAttribute : Attribute
{
}";

        await VerifyDiagnostic(original, "MyAttribute should specify how the attribute can be used");
        await VerifyFix(original, result);
    }

    [TestMethod]
    [DataRow("AttributeUsage")]
    [DataRow("AttributeUsageAttribute")]
    [DataRow("System.AttributeUsageAttribute")]
    public async Task AttributeMustSpecifyAttributeUsage_Existing(string name)
    {
        var original = $@"
using System;

[{name}(AttributeTargets.All, AllowMultiple = false)]
class MyAttribute : Attribute
{{
}}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AttributeMustSpecifyAttributeUsage_NotAnAttribute()
    {
        var original = $@"
using System;

class SomeAttribute {{ }}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task AttributeMustSpecifyAttributeUsage_NoUsingStatement()
    {
        var original = @"
class MyAttribute : System.Attribute
{
}";

        var result = @"using System;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
class MyAttribute : System.Attribute
{
}";

        await VerifyDiagnostic(original, "MyAttribute should specify how the attribute can be used");
        await VerifyFix(original, result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/169")]
    public async Task AttributeMustSpecifyAttributeUsage_WithAttribute_InPreProcessorDirective()
    {
        var original = @"
#if DOESNOTEXIST
[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
class MyAttribute : Attribute
{
}
#endif";

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/197")]
    public async Task AttributeMustSpecifyAttributeUsage_WithoutAttribute_InDerivedClass()
    {
        var original = @"
using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
class MyAttribute : Attribute { }

class DerivedAttribute : MyAttribute { }";

        await VerifyDiagnostic(original);
    }
}