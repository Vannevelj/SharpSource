using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Test.Helpers;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.AttributeMustSpecifyAttributeUsageAnalyzer, SharpSource.Diagnostics.AttributeMustSpecifyAttributeUsageCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class AttributeMustSpecifyAttributeUsageTests
{
    [TestMethod]
    public async Task AttributeMustSpecifyAttributeUsage_NoAttribute()
    {
        var original = @"
using System;

class {|#0:MyAttribute|} : Attribute
{
}";

        var result = @"
using System;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
class MyAttribute : Attribute
{
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("MyAttribute should specify how the attribute can be used"), result);
    }

    [TestMethod]
    public async Task AttributeMustSpecifyAttributeUsage_NoAttribute_OtherExisting()
    {
        var original = @"
using System;

[Obsolete]
class {|#0:MyAttribute|} : Attribute
{
}";

        var result = @"
using System;

[Obsolete]
[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
class MyAttribute : Attribute
{
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("MyAttribute should specify how the attribute can be used"), result);
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task AttributeMustSpecifyAttributeUsage_NotAnAttribute()
    {
        var original = $@"
using System;

class SomeAttribute {{ }}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task AttributeMustSpecifyAttributeUsage_NoUsingStatement()
    {
        var original = @"
class {|#0:MyAttribute|} : System.Attribute
{
}";

        var result = @"using System;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
class MyAttribute : System.Attribute
{
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("MyAttribute should specify how the attribute can be used"), result);
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/197")]
    public async Task AttributeMustSpecifyAttributeUsage_WithoutAttribute_InDerivedClass()
    {
        var original = @"
using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
class MyAttribute : Attribute { }

class DerivedAttribute : MyAttribute { }";

        await VerifyCS.VerifyNoDiagnostic(original);

    }
}