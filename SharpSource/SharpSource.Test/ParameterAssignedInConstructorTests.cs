using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class ParameterAssignedInConstructorTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new ParameterAssignedInConstructorAnalyzer();

    //protected override CodeFixProvider CodeFixProvider => new ParameterAssignedInConstructorCodeFix();

    [TestMethod]
    public async Task ParameterAssignedInConstructor_Property()
    {
        var original = @"
class Test
{
    int Count { get; set; }

    Test(int count)
    {
        count = Count;
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

        await VerifyDiagnostic(original, "Suspicious assignment of parameter count in constructor of Test");
        await VerifyFix(original, result);
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

        await VerifyDiagnostic(original);
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
        count = _count;
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

        await VerifyDiagnostic(original, "Suspicious assignment of parameter count in constructor of Test");
        await VerifyFix(original, result);
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

        await VerifyDiagnostic(original);
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

        await VerifyDiagnostic(original);
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ParameterAssignedInConstructor_NoConstructor()
    {
        var original = @"
class Test
{
    int Count { get; set; }
}";

        await VerifyDiagnostic(original);
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
        count = Count;
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

        await VerifyDiagnostic(original, "Suspicious assignment of parameter count in constructor of Test");
        await VerifyFix(original, result);
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
        count = Count;
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

        await VerifyDiagnostic(new[] { file1, file2 }, "Suspicious assignment of parameter count in constructor of Test");
        await VerifyFix(file2, result);
    }

    [TestMethod]
    public async Task ParameterAssignedInConstructor_ExpressionBody()
    {
        var original = @"
class Test
{
    int Count { get; set; }

    Test(int count) => count = Count;
}";

        var result = @"
class Test
{
    int Count { get; set; }

    Test(int count) => Count = count;
}";

        await VerifyDiagnostic(original, "Suspicious assignment of parameter count in constructor of Test");
        await VerifyFix(original, result);
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
        count = _count;
        other = _count2;
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

        await VerifyDiagnostic(
            original,
            "Suspicious assignment of parameter count in constructor of Test",
            "Suspicious assignment of parameter other in constructor of Test");
        await VerifyFix(original, result);
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

        await VerifyDiagnostic(original);
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

        await VerifyDiagnostic(original);
    }
}