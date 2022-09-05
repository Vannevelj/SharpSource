using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers.Helpers;

namespace SharpSource.Test;

[TestClass]
public class AttributeMustSpecifyAttributeUsageTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new AttributeMustSpecifyAttributeUsageAnalyzer();

    protected override CodeFixProvider CodeFixProvider => new AttributeMustSpecifyAttributeUsageCodeFix();

    [TestMethod]
    public void AttributeMustSpecifyAttributeUsage_NoAttribute()
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

        VerifyDiagnostic(original, "MyAttribute should specify how the attribute can be used");
        VerifyFix(original, result);
    }

    [TestMethod]
    public void AttributeMustSpecifyAttributeUsage_NoAttribute_OtherExisting()
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

        VerifyDiagnostic(original, "MyAttribute should specify how the attribute can be used");
        VerifyFix(original, result);
    }

    [TestMethod]
    [DataRow("AttributeUsage")]
    [DataRow("AttributeUsageAttribute")]
    [DataRow("System.AttributeUsageAttribute")]
    public void AttributeMustSpecifyAttributeUsage_Existing(string name)
    {
        var original = $@"
using System;

[{name}(AttributeTargets.All, AllowMultiple = false)]
class MyAttribute : Attribute
{{
}}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void AttributeMustSpecifyAttributeUsage_NotAnAttribute()
    {
        var original = $@"
using System;

class SomeAttribute {{ }}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void AttributeMustSpecifyAttributeUsage_NoUsingStatement()
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

        VerifyDiagnostic(original, "MyAttribute should specify how the attribute can be used");
        VerifyFix(original, result);
    }
}