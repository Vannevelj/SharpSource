using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.LockingOnDiscouragedObjectAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class LockingOnDiscouragedObjectTests
{
    [TestMethod]
    [DataRow("Type", "Type")]
    [DataRow("System.Type", "Type")]
    [DataRow("string", "string")]
    [DataRow("System.String", "string")]
    public async Task LockingOnDiscouragedObject(string lockObject, string lockMessage)
    {
        var original = $@"
using System;

class Test
{{
    private {lockObject} _lock = default;

    void Method()
    {{
        lock ({{|#0:_lock|}}) {{ }}
    }}
}}
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(LockingOnDiscouragedObjectAnalyzer.Rule).WithLocation(0).WithMessage($"A lock was used on an object of type {lockMessage} which can lead to deadlocks. It is recommended to create a dedicated lock instance of type System.Object instead."));
    }

    [TestMethod]
    public async Task LockingOnDiscouragedObject_this()
    {
        var original = @"
class Test
{
    void Method()
    {
        lock ({|#0:this|}) {}
    }
}
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(LockingOnDiscouragedObjectAnalyzer.RuleThis).WithLocation(0).WithMessage($"A lock was used referencing 'this' which can lead to deadlocks. It is recommended to create a dedicated lock instance of type System.Object instead."));
    }

    [TestMethod]
    public async Task LockingOnDiscouragedObject_OtherType()
    {
        var original = @"
class Test
{
    private object _lock = new object();

    void Method()
    {
        lock (_lock) {}
    }
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}