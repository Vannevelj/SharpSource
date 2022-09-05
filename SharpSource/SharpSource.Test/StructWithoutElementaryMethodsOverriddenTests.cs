using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers.Helpers;

namespace SharpSource.Test;

[TestClass]
public class StructWithoutElementaryMethodsOverriddenTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new StructWithoutElementaryMethodsOverriddenAnalyzer();
    protected override CodeFixProvider CodeFixProvider => new StructWithoutElementaryMethodsOverriddenCodeFix();

    [TestMethod]
    public void StructWithoutElementaryMethodsOverridden_NoMethodsImplemented()
    {
        var original = @"
struct X
{
}";

        var result = @"
struct X
{
    public override bool Equals(object obj)
    {
        throw new System.NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }

    public override string ToString()
    {
        throw new System.NotImplementedException();
    }
}";

        VerifyDiagnostic(original, string.Format(StructWithoutElementaryMethodsOverriddenAnalyzer.Rule.MessageFormat.ToString(), "X"));
        VerifyFix(original, result);
    }

    [TestMethod]
    public void StructWithoutElementaryMethodsOverridden_EqualsImplemented()
    {
        var original = @"
struct X
{
    public override bool Equals(object obj)
    {
        return false;
    }
}";

        var result = @"
struct X
{
    public override bool Equals(object obj)
    {
        return false;
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }

    public override string ToString()
    {
        throw new System.NotImplementedException();
    }
}";

        VerifyDiagnostic(original, string.Format(StructWithoutElementaryMethodsOverriddenAnalyzer.Rule.MessageFormat.ToString(), "X"));
        VerifyFix(original, result);
    }

    [TestMethod]
    public void StructWithoutElementaryMethodsOverridden_GetHashCodeImplemented()
    {
        var original = @"
struct X
{
    public override int GetHashCode()
    {
        return 0;
    }
}";

        var result = @"
struct X
{
    public override int GetHashCode()
    {
        return 0;
    }

    public override bool Equals(object obj)
    {
        throw new System.NotImplementedException();
    }

    public override string ToString()
    {
        throw new System.NotImplementedException();
    }
}";

        VerifyDiagnostic(original, string.Format(StructWithoutElementaryMethodsOverriddenAnalyzer.Rule.MessageFormat.ToString(), "X"));
        VerifyFix(original, result);
    }

    [TestMethod]
    public void StructWithoutElementaryMethodsOverridden_ToStringImplemented()
    {
        var original = @"
struct X
{
    public override string ToString()
    {
        return string.Empty;
    }
}";

        var result = @"
struct X
{
    public override string ToString()
    {
        return string.Empty;
    }

    public override bool Equals(object obj)
    {
        throw new System.NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }
}";

        VerifyDiagnostic(original, string.Format(StructWithoutElementaryMethodsOverriddenAnalyzer.Rule.MessageFormat.ToString(), "X"));
        VerifyFix(original, result);
    }

    [TestMethod]
    public void StructWithoutElementaryMethodsOverridden_EqualsAndGetHashCodeImplemented()
    {
        var original = @"
struct X
{
    public override bool Equals(object obj)
    {
        return false;
    }

    public override int GetHashCode()
    {
        return 0;
    }
}";

        var result = @"
struct X
{
    public override bool Equals(object obj)
    {
        return false;
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public override string ToString()
    {
        throw new System.NotImplementedException();
    }
}";

        VerifyDiagnostic(original, string.Format(StructWithoutElementaryMethodsOverriddenAnalyzer.Rule.MessageFormat.ToString(), "X"));
        VerifyFix(original, result);
    }

    [TestMethod]
    public void StructWithoutElementaryMethodsOverridden_AllImplemented()
    {
        var original = @"
struct X
{
    public override bool Equals(object obj)
    {
        return false;
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public override string ToString()
    {
        return string.Empty;
    }
}";

        VerifyDiagnostic(original);
    }
}