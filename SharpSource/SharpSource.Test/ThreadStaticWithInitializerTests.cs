using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class ThreadStaticWithInitializerTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new ThreadStaticWithInitializerAnalyzer();

    [TestMethod]
    public async Task ThreadStaticWithInitializer()
    {
        var original = @"
using System;
using System.Threading;

class MyClass
{
    [ThreadStatic]
    static Random _random = new Random();
}
";

        await VerifyDiagnostic(original, "_random is marked as [ThreadStatic] so it cannot contain an initializer");
    }

    [TestMethod]
    public async Task ThreadStaticWithInitializer_NoInitializer()
    {
        var original = @"
using System;
using System.Threading;

class MyClass
{
    [ThreadStatic]
    static Random _random;
}
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ThreadStaticWithInitializer_MultipleDeclarators_SingleInitializer()
    {
        var original = @"
using System;
using System.Threading;

class MyClass
{
    [ThreadStatic]
    static Random _random, _random2 = new Random();
}
";

        await VerifyDiagnostic(original, "_random2 is marked as [ThreadStatic] so it cannot contain an initializer");
    }

    [TestMethod]
    public async Task ThreadStaticWithInitializer_MultipleDeclarators_MultipleInitializers()
    {
        var original = @"
using System;
using System.Threading;

class MyClass
{
    [ThreadStatic]
    static Random _random = new(), _random2 = new Random();
}
";

        await VerifyDiagnostic(original, 
            "_random is marked as [ThreadStatic] so it cannot contain an initializer",
            "_random2 is marked as [ThreadStatic] so it cannot contain an initializer");
    }
}