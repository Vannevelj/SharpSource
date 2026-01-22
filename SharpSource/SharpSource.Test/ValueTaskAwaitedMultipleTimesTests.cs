using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.ValueTaskAwaitedMultipleTimesAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class ValueTaskAwaitedMultipleTimesTests
{
    [TestMethod]
    public async Task ValueTaskAwaitedMultipleTimes_LocalVariable()
    {
        var original = @"
using System.Threading.Tasks;

class MyClass
{
    async Task MyMethod()
    {
        ValueTask Method() => ValueTask.CompletedTask;

        var task = Method();
        await task;
        {|#0:await task|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("A ValueTask can only be awaited once"));
    }

    [TestMethod]
    public async Task ValueTaskAwaitedMultipleTimes_LocalVariable_Generic()
    {
        var original = @"
using System.Threading.Tasks;

class MyClass
{
    async Task MyMethod()
    {
        ValueTask<int> Method() => ValueTask.FromResult(42);

        var task = Method();
        await task;
        {|#0:await task|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("A ValueTask can only be awaited once"));
    }

    [TestMethod]
    public async Task ValueTaskAwaitedMultipleTimes_Parameter()
    {
        var original = @"
using System.Threading.Tasks;

class MyClass
{
    async Task MyMethod(ValueTask task)
    {
        await task;
        {|#0:await task|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("A ValueTask can only be awaited once"));
    }

    [TestMethod]
    public async Task ValueTaskAwaitedMultipleTimes_Parameter_Generic()
    {
        var original = @"
using System.Threading.Tasks;

class MyClass
{
    async Task MyMethod(ValueTask<int> task)
    {
        await task;
        {|#0:await task|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("A ValueTask can only be awaited once"));
    }

    [TestMethod]
    public async Task ValueTaskAwaitedMultipleTimes_AwaitedThreeTimes()
    {
        var original = @"
using System.Threading.Tasks;

class MyClass
{
    async Task MyMethod()
    {
        ValueTask Method() => ValueTask.CompletedTask;

        var task = Method();
        await task;
        {|#0:await task|};
        {|#1:await task|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic(0).WithMessage("A ValueTask can only be awaited once"),
            VerifyCS.Diagnostic(1).WithMessage("A ValueTask can only be awaited once"));
    }

    [TestMethod]
    public async Task ValueTaskAwaitedMultipleTimes_SingleAwait_NoDiagnostic()
    {
        var original = @"
using System.Threading.Tasks;

class MyClass
{
    async Task MyMethod()
    {
        ValueTask Method() => ValueTask.CompletedTask;

        var task = Method();
        await task;
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ValueTaskAwaitedMultipleTimes_DirectAwait_NoDiagnostic()
    {
        var original = @"
using System.Threading.Tasks;

class MyClass
{
    async Task MyMethod()
    {
        ValueTask Method() => ValueTask.CompletedTask;

        await Method();
        await Method();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ValueTaskAwaitedMultipleTimes_DifferentVariables_NoDiagnostic()
    {
        var original = @"
using System.Threading.Tasks;

class MyClass
{
    async Task MyMethod()
    {
        ValueTask Method() => ValueTask.CompletedTask;

        var task1 = Method();
        var task2 = Method();
        await task1;
        await task2;
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ValueTaskAwaitedMultipleTimes_TaskNotValueTask_NoDiagnostic()
    {
        var original = @"
using System.Threading.Tasks;

class MyClass
{
    async Task MyMethod()
    {
        Task Method() => Task.CompletedTask;

        var task = Method();
        await task;
        await task;
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ValueTaskAwaitedMultipleTimes_TaskGenericNotValueTask_NoDiagnostic()
    {
        var original = @"
using System.Threading.Tasks;

class MyClass
{
    async Task MyMethod()
    {
        Task<int> Method() => Task.FromResult(42);

        var task = Method();
        await task;
        await task;
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ValueTaskAwaitedMultipleTimes_InLambda()
    {
        var original = @"
using System;
using System.Threading.Tasks;

class MyClass
{
    async Task MyMethod()
    {
        Func<Task> action = async () =>
        {
            ValueTask Method() => ValueTask.CompletedTask;
            var task = Method();
            await task;
            {|#0:await task|};
        };
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("A ValueTask can only be awaited once"));
    }

    [TestMethod]
    public async Task ValueTaskAwaitedMultipleTimes_InLocalFunction()
    {
        var original = @"
using System.Threading.Tasks;

class MyClass
{
    async Task MyMethod()
    {
        async Task LocalMethod()
        {
            ValueTask Method() => ValueTask.CompletedTask;
            var task = Method();
            await task;
            {|#0:await task|};
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("A ValueTask can only be awaited once"));
    }

    [TestMethod]
    public async Task ValueTaskAwaitedMultipleTimes_ConfigureAwait()
    {
        var original = @"
using System.Threading.Tasks;

class MyClass
{
    async Task MyMethod()
    {
        ValueTask Method() => ValueTask.CompletedTask;

        var task = Method();
        await task.ConfigureAwait(false);
        {|#0:await task.ConfigureAwait(false)|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("A ValueTask can only be awaited once"));
    }

    [TestMethod]
    public async Task ValueTaskAwaitedMultipleTimes_Field_NoDiagnostic()
    {
        var original = @"
using System.Threading.Tasks;

class MyClass
{
    private ValueTask _task;

    async Task MyMethod()
    {
        await _task;
        await _task;
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ValueTaskAwaitedMultipleTimes_Property_NoDiagnostic()
    {
        var original = @"
using System.Threading.Tasks;

class MyClass
{
    private ValueTask Task => ValueTask.CompletedTask;

    async Task MyMethod()
    {
        await Task;
        await Task;
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}
