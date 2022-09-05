using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers.Helpers;
using SharpSource.Test.Helpers.Helpers.CSharp;

namespace SharpSource.Test;

[TestClass]
public class InstanceFieldWithThreadStaticTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new InstanceFieldWithThreadStaticAnalyzer();

    [TestMethod]
    [DataRow("[ThreadStatic]")]
    [DataRow("[ThreadStaticAttribute]")]
    [DataRow("[System.ThreadStaticAttribute]")]
    public void InstanceFieldWithThreadStatic_InstanceField(string attribute)
    {
        var original = $@"
using System;

class MyClass
{{
    {attribute}
    int _field;
}}";

        VerifyDiagnostic(original, "Field _field is marked as [ThreadStatic] but is not static");
    }

    [TestMethod]
    public void InstanceFieldWithThreadStatic_InstanceField_Static()
    {
        var original = @"
using System;

class MyClass
{
    [ThreadStatic]
    static int _field;
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void InstanceFieldWithThreadStatic_OtherModifiers()
    {
        var original = @"
using System;

class MyClass
{
    [ThreadStatic]
    public readonly int _field;
}";

        VerifyDiagnostic(original, "Field _field is marked as [ThreadStatic] but is not static");
    }

    [TestMethod]
    public void InstanceFieldWithThreadStatic_OtherAttribute()
    {
        var original = @"
using System;

class MyClass
{
    [Obsolete]
    int _field;
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void InstanceFieldWithThreadStatic_OtherAttribute_OnProperty()
    {
        var original = @"
using System;

class MyClass
{
    [Obsolete]
    int _field => 5;
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void InstanceFieldWithThreadStatic_IgnoresConst()
    {
        var original = @"
using System;

class MyClass
{
    [ThreadStatic]
    const int _field = 5;
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void InstanceFieldWithThreadStatic_MultipleDeclarators()
    {
        var original = @"
using System;

class MyClass
{
    [ThreadStatic]
    int _field, _field2;
}";

        VerifyDiagnostic(original, "Field _field is marked as [ThreadStatic] but is not static", "Field _field2 is marked as [ThreadStatic] but is not static");
    }

    [TestMethod]
    public void InstanceFieldWithThreadStatic_MultipleAttributes()
    {
        var original = @"
using System;

class MyClass
{
    [ThreadStatic]
    [Obsolete]
    int _field;
}";

        VerifyDiagnostic(original, "Field _field is marked as [ThreadStatic] but is not static");
    }
}