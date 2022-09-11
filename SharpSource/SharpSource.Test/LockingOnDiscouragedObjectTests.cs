using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class LockingOnDiscouragedObjectTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new LockingOnDiscouragedObjectAnalyzer();

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
        lock (_lock) {{ }}
    }}
}}
";

        await VerifyDiagnostic(original, $"A lock was used on an object of type {lockMessage} which can lead to deadlocks. It is recommended to create a dedicated lock instance of type System.Object instead.");
    }

    [TestMethod]
    public async Task LockingOnDiscouragedObject_this()
    {
        var original = @"
class Test
{
    void Method()
    {
        lock (this) {}
    }
}
";

        await VerifyDiagnostic(original, $"A lock was used referencing 'this' which can lead to deadlocks. It is recommended to create a dedicated lock instance of type System.Object instead.");
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

        await VerifyDiagnostic(original);
    }
}