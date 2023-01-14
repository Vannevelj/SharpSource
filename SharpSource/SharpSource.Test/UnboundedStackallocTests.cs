using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.UnboundedStackallocAnalyzer, SharpSource.Diagnostics.UnboundedStackallocCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class UnboundedStackallocTests
{
    [TestMethod]
    public async Task UnboundedStackalloc()
    {
        var original = @"
using System;

var len = new Random().Next();
Span<int> values = stackalloc int{|#0:[len]|};";

        var result = @"
using System;

var len = new Random().Next();
Span<int> values = len < 1024 ? stackalloc int[len] : new int[len];";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("An array is stack allocated without checking the length. Explicitly check the length against a constant value"), result);
    }

    [TestMethod]
    public async Task UnboundedStackalloc_TernaryCheck()
    {
        var original = @"
using System;

var len = new Random().Next();
Span<int> values = len < 1024 ? stackalloc int[len] : new int[len];";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task UnboundedStackalloc_InsideIf()
    {
        var original = @"
using System;

var len = new Random().Next();
if (len < 1024) {
    Span<int> values = len < 1024 ? stackalloc int[len] : new int[len];
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task UnboundedStackalloc_OutsideIf()
    {
        var original = @"
using System;

var len = new Random().Next();
if (len < 1024) {
    return;
}
Span<int> values = len < 1024 ? stackalloc int[len] : new int[len];
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task UnboundedStackalloc_InvertedCheck()
    {
        var original = @"
using System;

var len = new Random().Next();
if (1024 > len) {
    Span<int> values = len < 1024 ? stackalloc int[len] : new int[len];
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task UnboundedStackalloc_ConstantLen()
    {
        var original = @"System.Span<int> values = stackalloc int[32];";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task UnboundedStackalloc_Unsafe()
    {
        var original = @"
using System;

var len = new Random().Next();
unsafe
{
    var v2 = stackalloc int{|#0:[len]|};
}";

        var result = @"
using System;

var len = new Random().Next();
unsafe
{
    var v2 = len < 1024 ? stackalloc int[len] : new int[len];
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("An array is stack allocated without checking the length. Explicitly check the length against a constant value"), result);
    }

    [TestMethod]
    public async Task UnboundedStackalloc_ConstantReferencedLen()
    {
        var original = @"
const int len = 32;
System.Span<int> values = stackalloc int[len];";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task UnboundedStackalloc_NonConstantReferencedLen()
    {
        var original = @"
var len = 32;
System.Span<int> values = stackalloc int{|#0:[len]|};";

        var result = @"
var len = 32;
System.Span<int> values = len < 1024 ? stackalloc int[len] : new int[len];";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("An array is stack allocated without checking the length. Explicitly check the length against a constant value"), result);
    }

    [TestMethod]
    public async Task UnboundedStackalloc_PassedAsArgument()
    {
        var original = @"
using System;

var len = 32;
M(stackalloc int{|#0:[len]|});
void M(Span<int> arr) { }";

        var result = @"
using System;

var len = 32;
M(len < 1024 ? stackalloc int[len] : new int[len]);
void M(Span<int> arr) { }";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("An array is stack allocated without checking the length. Explicitly check the length against a constant value"), result);
    }

    [TestMethod]
    public async Task UnboundedStackalloc_LocalFunctionExpressionBody()
    {
        var original = @"
using System;

void Outer()
{
    var len = new Random().Next();
    void Inner() => (stackalloc int{|#0:[len]|}).ToString();
}";

        var result = @"
using System;

void Outer()
{
    var len = new Random().Next();
    void Inner() => (len < 1024 ? stackalloc int[len] : new int[len]).ToString();
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("An array is stack allocated without checking the length. Explicitly check the length against a constant value"), result);
    }

    [TestMethod]
    public async Task UnboundedStackalloc_MethodExpressionBody()
    {
        var original = @"
using System;

class Test
{
    private int len = new Random().Next();
    void Method() => (stackalloc int{|#0:[len]|}).ToString();
}";

        var result = @"
using System;

class Test
{
    private int len = new Random().Next();
    void Method() => (len < 1024 ? stackalloc int[len] : new int[len]).ToString();
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("An array is stack allocated without checking the length. Explicitly check the length against a constant value"), result);
    }

    [TestMethod]
    public async Task UnboundedStackalloc_MethodBlock()
    {
        var original = @"
using System;

class Test
{
    private int len = new Random().Next();
    void Method()
    {
        var len = new Random().Next();
        Span<int> values = stackalloc int{|#0:[len]|};
    }
}";

        var result = @"
using System;

class Test
{
    private int len = new Random().Next();
    void Method()
    {
        var len = new Random().Next();
        Span<int> values = len < 1024 ? stackalloc int[len] : new int[len];
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("An array is stack allocated without checking the length. Explicitly check the length against a constant value"), result);
    }
}