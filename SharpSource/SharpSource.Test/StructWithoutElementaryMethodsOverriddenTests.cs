using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class StructWithoutElementaryMethodsOverriddenTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new StructWithoutElementaryMethodsOverriddenAnalyzer();
    protected override CodeFixProvider CodeFixProvider => new StructWithoutElementaryMethodsOverriddenCodeFix();

    [TestMethod]
    public async Task StructWithoutElementaryMethodsOverridden_NoMethodsImplementedAsync()
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

        await VerifyDiagnostic(original, "Implement Equals(), GetHashCode() and ToString() methods on struct X.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task StructWithoutElementaryMethodsOverridden_EqualsImplementedAsync()
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

        await VerifyDiagnostic(original, "Implement Equals(), GetHashCode() and ToString() methods on struct X.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task StructWithoutElementaryMethodsOverridden_GetHashCodeImplementedAsync()
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

        await VerifyDiagnostic(original, "Implement Equals(), GetHashCode() and ToString() methods on struct X.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task StructWithoutElementaryMethodsOverridden_ToStringImplementedAsync()
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

        await VerifyDiagnostic(original, "Implement Equals(), GetHashCode() and ToString() methods on struct X.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task StructWithoutElementaryMethodsOverridden_EqualsAndGetHashCodeImplementedAsync()
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

        await VerifyDiagnostic(original, "Implement Equals(), GetHashCode() and ToString() methods on struct X.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task StructWithoutElementaryMethodsOverridden_AllImplementedAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StructWithoutElementaryMethodsOverridden_Partial_PartiallyImplemented()
    {
        var file1 = @"
partial struct X
{
    public override bool Equals(object obj) => false;
}";

        var file2 = @"
partial struct X
{
    
}";

        var file1Result = @"
partial struct X
{
    public override bool Equals(object obj) => false;

    public override int GetHashCode()
    {
        throw new System.NotImplementedException();
    }

    public override string ToString()
    {
        throw new System.NotImplementedException();
    }
}";

        await VerifyDiagnostic(new[] { file1, file2 },
            "Implement Equals(), GetHashCode() and ToString() methods on struct X.",
            "Implement Equals(), GetHashCode() and ToString() methods on struct X.");
        await VerifyFix(file1, file1Result);
    }

    [TestMethod]
    public async Task StructWithoutElementaryMethodsOverridden_Partial_FullyImplemented()
    {
        var file1 = @"
partial struct X
{
    public override bool Equals(object obj) => false;
}";

        var file2 = @"
partial struct X
{
    public override int GetHashCode() => 5;
    public override string ToString() => string.Empty;
}";

        await VerifyDiagnostic(new[] { file1, file2 });
    }
}