using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class LockingOnMutableReferenceTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new LockingOnMutableReferenceAnalyzer();

    protected override CodeFixProvider CodeFixProvider => new LockingOnMutableReferenceCodeFix();

    [TestMethod]
    public async Task LockingOnMutableReference_MissingReadonly()
    {
        var original = @"
class Test
{
    private object _lock = new object();

    void M()
    {
        lock(_lock) { }
    }
}";

        var expected = @"
class Test
{
    private readonly object _lock = new object();

    void M()
    {
        lock(_lock) { }
    }
}";

        await VerifyDiagnostic(original, "A lock was obtained on _lock but the field is mutable. This can lead to deadlocks when a new value is assigned.");
        await VerifyFix(original, expected);
    }

    [TestMethod]
    public async Task LockingOnMutableReference_Readonly()
    {
        var original = @"
class Test
{
    private readonly object _lock = new object();

    void M()
    {
        lock(_lock) { }
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task LockingOnMutableReference_Const()
    {
        var original = @"
class Test
{
    private const string _lock = ""bad lock!"";

    void M()
    {
        lock(_lock) { }
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task LockingOnMutableReference_NoLocking()
    {
        var original = @"
class Test
{
    private object _lock = new object();
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task LockingOnMutableReference_Property()
    {
        var original = @"
class Test
{
    public object Prop { get; set; } = new();

    void M()
    {
        lock(Prop) { }
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task LockingOnMutableReference_Partial_SameFile()
    {
        var original = @"
partial class Test
{
    private object _lock = new object();
}

partial class Test
{
    void M()
    {
        lock(_lock) { }
    }
}";

        var expected = @"
partial class Test
{
    private readonly object _lock = new object();
}

partial class Test
{
    void M()
    {
        lock(_lock) { }
    }
}";

        await VerifyDiagnostic(original, "A lock was obtained on _lock but the field is mutable. This can lead to deadlocks when a new value is assigned.");
        await VerifyFix(original, expected);
    }

    [TestMethod]
    public async Task LockingOnMutableReference_Partial_DifferentFiles()
    {
        var file1 = @"
partial class Test
{
    private object _lock = new object();
}";

        var file2 = @"
partial class Test
{
    void M()
    {
        lock(_lock) { }
    }
}";

        var result = @"
partial class Test
{
    private readonly object _lock = new object();
}";

        await VerifyDiagnostic(new[] { file1, file2 }, "A lock was obtained on _lock but the field is mutable. This can lead to deadlocks when a new value is assigned.");
        await VerifyFix(file1, result, additionalSources: new[] { file2 });
    }
}