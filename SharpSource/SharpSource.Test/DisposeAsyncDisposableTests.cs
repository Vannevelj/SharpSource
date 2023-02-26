using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.DisposeAsyncDisposableAnalyzer, SharpSource.Diagnostics.DisposeAsyncDisposableCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class DisposeAsyncDisposableTests
{
    [TestMethod]
    public async Task DisposeAsyncDisposable_GlobalStatement()
    {
        var original = @"
using System.IO;

{|#0:using var stream = new FileStream("""", FileMode.Create);|}
";

        var result = @"
using System.IO;

await using var stream = new FileStream("""", FileMode.Create);
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("FileStream can be disposed of asynchronously"), result);
    }

    [TestMethod]
    public async Task DisposeAsyncDisposable_InMethod()
    {
        var original = @"
using System.IO;
using System.Threading.Tasks;

async Task Method()
{
    {|#0:using var stream = new FileStream("""", FileMode.Create);|}
}
";

        var result = @"
using System.IO;
using System.Threading.Tasks;

async Task Method()
{
    await using var stream = new FileStream("""", FileMode.Create);
}
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("FileStream can be disposed of asynchronously"), result);
    }

    [TestMethod]
    public async Task DisposeAsyncDisposable_Parentheses()
    {
        var original = @"
using System.IO;

{|#0:using (var stream = new FileStream("""", FileMode.Create));|}
";

        var result = @"
using System.IO;

await using (var stream = new FileStream("""", FileMode.Create));
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("FileStream can be disposed of asynchronously"), result);
    }

    [TestMethod]
    public async Task DisposeAsyncDisposable_ParenthesesWithBody()
    {
        var original = @"
using System.IO;

{|#0:using (var stream = new FileStream("""", FileMode.Create))
{

}|}
";

        var result = @"
using System.IO;

await using (var stream = new FileStream("""", FileMode.Create))
{

}
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("FileStream can be disposed of asynchronously"), result);
    }

    [TestMethod]
    public async Task DisposeAsyncDisposable_InMethod_Sync()
    {
        var original = @"
using System.IO;
using System.Threading.Tasks;

Task Method()
{
    using var stream = new FileStream("""", FileMode.Create);
    return Task.CompletedTask;
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DisposeAsyncDisposable_InLocalFunction()
    {
        var original = @"
using System.IO;
using System.Threading.Tasks;

async Task Method() 
{
    async Task Inner() 
    {
        {|#0:using var stream = new FileStream("""", FileMode.Create);|}
    }

    await Inner();
}
";

        var result = @"
using System.IO;
using System.Threading.Tasks;

async Task Method() 
{
    async Task Inner() 
    {
        await using var stream = new FileStream("""", FileMode.Create);
    }

    await Inner();
}
"
        ;

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("FileStream can be disposed of asynchronously"), result);
    }

    [TestMethod]
    public async Task DisposeAsyncDisposable_InLocalFunction_Sync()
    {
        var original = @"
using System.IO;
using System.Threading.Tasks;

async Task Method() 
{
    Task Inner() 
    {
        using var stream = new FileStream("""", FileMode.Create);
        return Task.CompletedTask;
    }

    await Inner();
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DisposeAsyncDisposable_UsingImportStatement()
    {
        var original = @"
using testing = System.IO.FileStream;
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DisposeAsyncDisposable_AlreadyAsync()
    {
        var original = @"
using System.IO;

await using var stream = new FileStream("""", FileMode.Create);
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DisposeAsyncDisposable_FetchesValueFromInvocation()
    {
        var original = @"
using System.IO;

{|#0:using var stream = Create();|}

FileStream Create() => new FileStream("""", FileMode.Create);
";

        var result = @"
using System.IO;

await using var stream = Create();

FileStream Create() => new FileStream("""", FileMode.Create);
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("FileStream can be disposed of asynchronously"), result);
    }

    [TestMethod]
    public async Task DisposeAsyncDisposable_MultipleDeclarators()
    {
        var original = @"
using System.IO;

{|#0:using FileStream stream = Create(), stream2 = Create();|}

FileStream Create() => new FileStream("""", FileMode.Create);
";

        var result = @"
using System.IO;

{|#0:await using FileStream stream = Create(), stream2 = Create();|}

FileStream Create() => new FileStream("""", FileMode.Create);
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("FileStream can be disposed of asynchronously"), result);
    }

    [TestMethod]
    public async Task DisposeAsyncDisposable_MultipleUsings()
    {
        var original = @"
using System.IO;

{|#0:using (var stream = new FileStream("""", FileMode.Create))
{|#1:using (var stream2 = new FileStream("""", FileMode.Create))
{

}|}|}
";

        var result = @"
using System.IO;

await using (var stream = new FileStream("""", FileMode.Create))
await using (var stream2 = new FileStream("""", FileMode.Create))
{

}
";

        await VerifyCS.VerifyCodeFix(original, new[] {
            VerifyCS.Diagnostic(location: 0).WithMessage("FileStream can be disposed of asynchronously"),
            VerifyCS.Diagnostic(location: 1).WithMessage("FileStream can be disposed of asynchronously")
        }, result);
    }

    [TestMethod]
    public async Task DisposeAsyncDisposable_Lock()
    {
        var original = @"
using System.IO;

object _lock = new object();

lock(_lock)
{
    using var stream = new FileStream("""", FileMode.Create);
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}