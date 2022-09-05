using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class InstanceFieldWithThreadStaticTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new InstanceFieldWithThreadStaticAnalyzer();

    [TestMethod]
    [DataRow("[ThreadStatic]")]
    [DataRow("[ThreadStaticAttribute]")]
    [DataRow("[System.ThreadStaticAttribute]")]
    public async Task InstanceFieldWithThreadStatic_InstanceFieldAsync(string attribute)
    {
        var original = $@"
using System;

class MyClass
{{
    {attribute}
    int _field;
}}";

        await VerifyDiagnostic(original, "Field _field is marked as [ThreadStatic] but is not static");
    }

    [TestMethod]
    public async Task InstanceFieldWithThreadStatic_InstanceField_StaticAsync()
    {
        var original = @"
using System;

class MyClass
{
    [ThreadStatic]
    static int _field;
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task InstanceFieldWithThreadStatic_OtherModifiersAsync()
    {
        var original = @"
using System;

class MyClass
{
    [ThreadStatic]
    public readonly int _field;
}";

        await VerifyDiagnostic(original, "Field _field is marked as [ThreadStatic] but is not static");
    }

    [TestMethod]
    public async Task InstanceFieldWithThreadStatic_OtherAttributeAsync()
    {
        var original = @"
using System;

class MyClass
{
    [Obsolete]
    int _field;
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task InstanceFieldWithThreadStatic_OtherAttribute_OnPropertyAsync()
    {
        var original = @"
using System;

class MyClass
{
    [Obsolete]
    int _field => 5;
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task InstanceFieldWithThreadStatic_IgnoresConstAsync()
    {
        var original = @"
using System;

class MyClass
{
    [ThreadStatic]
    const int _field = 5;
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task InstanceFieldWithThreadStatic_MultipleDeclaratorsAsync()
    {
        var original = @"
using System;

class MyClass
{
    [ThreadStatic]
    int _field, _field2;
}";

        await VerifyDiagnostic(original, "Field _field is marked as [ThreadStatic] but is not static", "Field _field2 is marked as [ThreadStatic] but is not static");
    }

    [TestMethod]
    public async Task InstanceFieldWithThreadStatic_MultipleAttributesAsync()
    {
        var original = @"
using System;

class MyClass
{
    [ThreadStatic]
    [Obsolete]
    int _field;
}";

        await VerifyDiagnostic(original, "Field _field is marked as [ThreadStatic] but is not static");
    }
}