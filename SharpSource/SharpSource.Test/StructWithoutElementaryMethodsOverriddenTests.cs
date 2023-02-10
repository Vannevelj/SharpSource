using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Test.Helpers;
using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.StructWithoutElementaryMethodsOverriddenAnalyzer, SharpSource.Diagnostics.StructWithoutElementaryMethodsOverriddenCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class StructWithoutElementaryMethodsOverriddenTests
{
    [TestMethod]
    public async Task StructWithoutElementaryMethodsOverridden_NoMethodsImplemented()
    {
        var original = @"
struct {|#0:X|}
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Implement Equals(), GetHashCode() and ToString() methods on struct X."), result);
    }

    [TestMethod]
    public async Task StructWithoutElementaryMethodsOverridden_EqualsImplemented()
    {
        var original = @"
struct {|#0:X|}
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Implement Equals(), GetHashCode() and ToString() methods on struct X."), result);
    }

    [TestMethod]
    public async Task StructWithoutElementaryMethodsOverridden_GetHashCodeImplemented()
    {
        var original = @"
struct {|#0:X|}
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Implement Equals(), GetHashCode() and ToString() methods on struct X."), result);
    }

    [TestMethod]
    public async Task StructWithoutElementaryMethodsOverridden_ToStringImplemented()
    {
        var original = @"
struct {|#0:X|}
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Implement Equals(), GetHashCode() and ToString() methods on struct X."), result);
    }

    [TestMethod]
    public async Task StructWithoutElementaryMethodsOverridden_EqualsAndGetHashCodeImplemented()
    {
        var original = @"
struct {|#0:X|}
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Implement Equals(), GetHashCode() and ToString() methods on struct X."), result);
    }

    [TestMethod]
    public async Task StructWithoutElementaryMethodsOverridden_AllImplemented()
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StructWithoutElementaryMethodsOverridden_Partial_PartiallyImplemented()
    {
        var original = @"
partial struct {|#0:X|}
{
    public override bool Equals(object obj) => false;
}

partial struct X
{

}";

        var result = @"
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
}

partial struct X
{

}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Implement Equals(), GetHashCode() and ToString() methods on struct X."), result);
    }

    [TestMethod]
    public async Task StructWithoutElementaryMethodsOverridden_Partial_FullyImplemented()
    {
        var original = @"
partial struct {|#0:X|}
{
    public override bool Equals(object obj) => false;
}

partial struct X
{
    public override int GetHashCode() => 5;
    public override string ToString() => string.Empty;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StructWithoutElementaryMethodsOverridden_IEquatableWithoutEquals()
    {
        var original = @"
using System;

struct {|#0:X|} : IEquatable<X>
{
    public bool Equals(X obj) => false;
    public override int GetHashCode() => 5;
    public override string ToString() => string.Empty;
}";

        var result = @"
using System;

struct {|#0:X|} : IEquatable<X>
{
    public bool Equals(X obj) => false;
    public override int GetHashCode() => 5;
    public override string ToString() => string.Empty;

    public override bool Equals(object obj)
    {
        throw new NotImplementedException();
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Implement Equals(), GetHashCode() and ToString() methods on struct X."), result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/296")]
    public async Task StructWithoutElementaryMethodsOverridden_NullableEnabled()
    {
        var original = @"
struct {|#0:X|}
{
}";

        var result = @"
struct X
{
    public override bool Equals(object? obj)
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

        var test = new VerifyCS.Test
        {
            TestCode = original,
            FixedCode = result,
            NullableContextOptions = NullableContextOptions.Enable
        };

        test.ExpectedDiagnostics.Add(VerifyCS.Diagnostic().WithMessage("Implement Equals(), GetHashCode() and ToString() methods on struct X."));

        await test.RunAsync();
    }
}