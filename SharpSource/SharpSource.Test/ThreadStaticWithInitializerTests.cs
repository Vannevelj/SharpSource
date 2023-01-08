using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.ThreadStaticWithInitializerAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class ThreadStaticWithInitializerTests
{
    [TestMethod]
    public async Task ThreadStaticWithInitializer()
    {
        var original = @"
using System;
using System.Threading;

class MyClass
{
    [ThreadStatic]
    static Random _random {|#0:= new Random()|};
}
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("_random is marked as [ThreadStatic] so it cannot contain an initializer"));
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

        await VerifyCS.VerifyNoDiagnostic(original);
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
    static Random _random, _random2 {|#0:= new Random()|};
}
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("_random2 is marked as [ThreadStatic] so it cannot contain an initializer"));
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
    static Random _random {|#0:= new()|}, _random2 {|#1:= new Random()|};
}
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithMessage("_random is marked as [ThreadStatic] so it cannot contain an initializer"),
            VerifyCS.Diagnostic(location: 1).WithMessage("_random2 is marked as [ThreadStatic] so it cannot contain an initializer"));
    }
}