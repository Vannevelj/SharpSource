using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.LockingOnMutableReferenceAnalyzer, SharpSource.Diagnostics.LockingOnMutableReferenceCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class LockingOnMutableReferenceTests
{

    [TestMethod]
    public async Task LockingOnMutableReference_MissingReadonly()
    {
        var original = @"
class Test
{
    private object _someLock = new object();

    void M()
    {
        lock({|#0:_someLock|}) { }
    }
}";

        var expected = @"
class Test
{
    private readonly object _someLock = new object();

    void M()
    {
        lock(_someLock) { }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A lock was obtained on _someLock but the field is mutable. This can lead to deadlocks when a new value is assigned."), expected);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task LockingOnMutableReference_NoLocking()
    {
        var original = @"
class Test
{
    private object _lock = new object();
}";

        await VerifyCS.VerifyNoDiagnostic(original);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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
        lock({|#0:_lock|}) { }
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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A lock was obtained on _lock but the field is mutable. This can lead to deadlocks when a new value is assigned."), expected);
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
        lock({|#0:_lock|}) { }
    }
}";

        var result = @"
partial class Test
{
    private object _lock = new object();
}";

        await VerifyCS.VerifyCodeFix(file1, new[] { VerifyCS.Diagnostic().WithMessage("A lock was obtained on _lock but the field is mutable. This can lead to deadlocks when a new value is assigned.") }, result, additionalFiles: new[] { file2 });
    }
}