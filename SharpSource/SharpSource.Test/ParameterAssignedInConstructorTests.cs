using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.ParameterAssignedInConstructorAnalyzer, SharpSource.Diagnostics.ParameterAssignedInConstructorCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class ParameterAssignedInConstructorTests
{
    [TestMethod]
    public async Task ParameterAssignedInConstructor_Property()
    {
        var original = @"
class Test
{
    int Count { get; set; }

    Test(int count)
    {
        {|#0:count|} = Count;
    }
}";

        var result = @"
class Test
{
    int Count { get; set; }

    Test(int count)
    {
        Count = count;
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Suspicious assignment of parameter count in constructor of Test"), result);
    }

    [TestMethod]
    public async Task ParameterAssignedInConstructor_Property_Correct()
    {
        var original = @"
class Test
{
    int Count { get; set; }

    Test(int count)
    {
        Count = count;
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ParameterAssignedInConstructor_Field()
    {
        var original = @"
class Test
{
    int _count;

    Test(int count)
    {
        {|#0:count|} = _count;
    }
}";

        var result = @"
class Test
{
    int _count;

    Test(int count)
    {
        _count = count;
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Suspicious assignment of parameter count in constructor of Test"), result);
    }

    [TestMethod]
    public async Task ParameterAssignedInConstructor_Field_Correct()
    {
        var original = @"
class Test
{
    int _count;

    Test(int count)
    {
        _count = count;
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ParameterAssignedInConstructor_NoStatementsInBody()
    {
        var original = @"
class Test
{
    int Count { get; set; }

    Test(int count)
    {
        
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ParameterAssignedInConstructor_NoParameters()
    {
        var original = @"
class Test
{
    int Count { get; set; }

    Test()
    {
        
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ParameterAssignedInConstructor_NoConstructor()
    {
        var original = @"
class Test
{
    int Count { get; set; }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ParameterAssignedInConstructor_PartialSameFile()
    {
        var original = @"
partial class Test
{
    int Count { get; set; }
}

partial class Test
{
    Test(int count)
    {
        {|#0:count|} = Count;
    }
}";

        var result = @"
partial class Test
{
    int Count { get; set; }
}

partial class Test
{
    Test(int count)
    {
        Count = count;
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Suspicious assignment of parameter count in constructor of Test"), result);
    }

    [TestMethod]
    public async Task ParameterAssignedInConstructor_ExpressionBody()
    {
        var original = @"
class Test
{
    int Count { get; set; }

    Test(int count) => {|#0:count|} = Count;
}";

        var result = @"
class Test
{
    int Count { get; set; }

    Test(int count) => Count = count;
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Suspicious assignment of parameter count in constructor of Test"), result);
    }

    [TestMethod]
    public async Task ParameterAssignedInConstructor_NotSimpleAssignment()
    {
        var original = @"
class Test
{
    int Count { get; set; }

    Test(int count) => count = count > 5 ? Count : 0;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ParameterAssignedInConstructor_Field_Multiple()
    {
        var original = @"
class Test
{
    int _count, _count2;

    Test(int count, int other)
    {
        {|#0:count|} = _count;
        {|#1:other|} = _count2;
    }
}";

        var result = @"
class Test
{
    int _count, _count2;

    Test(int count, int other)
    {
        _count = count;
        _count2 = other;
    }
}";

        await VerifyCS.VerifyCodeFix(original, new[] {
            VerifyCS.Diagnostic(location: 0).WithMessage("Suspicious assignment of parameter count in constructor of Test"),
            VerifyCS.Diagnostic(location: 1).WithMessage("Suspicious assignment of parameter other in constructor of Test")
        }, result);
    }

    [TestMethod]
    public async Task ParameterAssignedInConstructor_RefParameter()
    {
        var original = @"
class Test
{
    int Count { get; set; }

    Test(ref int count) => count = Count;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ParameterAssignedInConstructor_OutParameter()
    {
        var original = @"
class Test
{
    int Count { get; set; }

    Test(out int count) => count = Count;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ParameterAssignedInConstructor_OtherStatements()
    {
        var original = @"
using System;

class Test
{
    int Count { get; set; }

    Test(int count)
    {
        Console.Read();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ParameterAssignedInConstructor_WithoutFieldOrProperty()
    {
        var original = @"
class Test
{
    Test(int count)
    {
        count = 5;
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ParameterAssignedInConstructor_MoreThanSimpleAssignment()
    {
        var original = @"
class Test
{
    int Count { get; set; }

    Test(int count)
    {
        count = Count + Count;
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ParameterAssignedInConstructor_PartialDifferentFiles()
    {
        var file1 = @"
partial class Test
{
    int Count { get; set; }
}";

        var file2 = @"
partial class Test
{
    Test(int count)
    {
        {|#0:count|} = Count;
    }
}";

        var result = @"
partial class Test
{
    Test(int count)
    {
        Count = count;
    }
}";

        await VerifyCS.VerifyCodeFix(file2, new[] { VerifyCS.Diagnostic().WithMessage("Suspicious assignment of parameter count in constructor of Test") }, result, additionalFiles: new[] { file1 });
    }

    [TestMethod]
    public async Task ParameterAssignedInConstructor_ConstField()
    {
        var original = @"
class Test
{
    private const int Count = 32;

    Test(int count)
    {
        count = Count;
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}