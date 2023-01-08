using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.InstanceFieldWithThreadStaticAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class InstanceFieldWithThreadStaticTests
{
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
    int {{|#0:_field|}};
}}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Field _field is marked as [ThreadStatic] but is not static"));
    }

    [TestMethod]
    public async Task InstanceFieldWithThreadStatic_InstanceField_Static()
    {
        var original = @"
using System;

class MyClass
{
    [ThreadStatic]
    static int _field;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task InstanceFieldWithThreadStatic_OtherModifiers()
    {
        var original = @"
using System;

class MyClass
{
    [ThreadStatic]
    public readonly int {|#0:_field|};
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Field _field is marked as [ThreadStatic] but is not static"));
    }

    [TestMethod]
    public async Task InstanceFieldWithThreadStatic_OtherAttribute()
    {
        var original = @"
using System;

class MyClass
{
    [Obsolete]
    int _field;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task InstanceFieldWithThreadStatic_OtherAttribute_OnProperty()
    {
        var original = @"
using System;

class MyClass
{
    [Obsolete]
    int _field => 5;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task InstanceFieldWithThreadStatic_IgnoresConst()
    {
        var original = @"
using System;

class MyClass
{
    [ThreadStatic]
    const int _field = 5;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task InstanceFieldWithThreadStatic_MultipleDeclarators()
    {
        var original = @"
using System;

class MyClass
{
    [ThreadStatic]
    int {|#0:_field|}, {|#1:_field2|};
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic(location: 0).WithMessage("Field _field is marked as [ThreadStatic] but is not static"),
            VerifyCS.Diagnostic(location: 1).WithMessage("Field _field2 is marked as [ThreadStatic] but is not static"));
    }

    [TestMethod]
    public async Task InstanceFieldWithThreadStatic_MultipleAttributes()
    {
        var original = @"
using System;

class MyClass
{
    [ThreadStatic]
    [Obsolete]
    int {|#0:_field|};
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Field _field is marked as [ThreadStatic] but is not static"));
    }
}