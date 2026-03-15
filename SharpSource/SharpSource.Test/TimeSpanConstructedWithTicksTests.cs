using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.TimeSpanConstructedWithTicksAnalyzer, SharpSource.Diagnostics.TimeSpanConstructedWithTicksCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class TimeSpanConstructedWithTicksTests
{
    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_SingleIntLiteral()
    {
        var original = @"
using System;

var timeout = {|#0:new TimeSpan(30)|};";

        var result = @"
using System;

var timeout = TimeSpan.FromTicks(30);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("TimeSpan was constructed with a single-parameter constructor which accepts ticks (100 nanoseconds). Use TimeSpan.FromSeconds(), TimeSpan.FromMilliseconds() or similar instead"), result);
    }

    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_SingleLongLiteral()
    {
        var original = @"
using System;

var timeout = {|#0:new TimeSpan(30L)|};";

        var result = @"
using System;

var timeout = TimeSpan.FromTicks(30L);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("TimeSpan was constructed with a single-parameter constructor which accepts ticks (100 nanoseconds). Use TimeSpan.FromSeconds(), TimeSpan.FromMilliseconds() or similar instead"), result);
    }

    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_FullyQualified()
    {
        var original = @"
var timeout = {|#0:new System.TimeSpan(30)|};";

        var result = @"
var timeout = System.TimeSpan.FromTicks(30);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("TimeSpan was constructed with a single-parameter constructor which accepts ticks (100 nanoseconds). Use TimeSpan.FromSeconds(), TimeSpan.FromMilliseconds() or similar instead"), result);
    }

    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_ZeroValue()
    {
        var original = @"
using System;

var timeout = {|#0:new TimeSpan(0)|};";

        var result = @"
using System;

var timeout = TimeSpan.FromTicks(0);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("TimeSpan was constructed with a single-parameter constructor which accepts ticks (100 nanoseconds). Use TimeSpan.FromSeconds(), TimeSpan.FromMilliseconds() or similar instead"), result);
    }

    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_LargeValue()
    {
        var original = @"
using System;

var timeout = {|#0:new TimeSpan(86400)|};";

        var result = @"
using System;

var timeout = TimeSpan.FromTicks(86400);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("TimeSpan was constructed with a single-parameter constructor which accepts ticks (100 nanoseconds). Use TimeSpan.FromSeconds(), TimeSpan.FromMilliseconds() or similar instead"), result);
    }

    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_ThreeParameters_NoDiagnostic()
    {
        var original = @"
using System;

var timeout = new TimeSpan(1, 30, 0);";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_FiveParameters_NoDiagnostic()
    {
        var original = @"
using System;

var timeout = new TimeSpan(1, 2, 30, 0, 500);";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_Variable()
    {
        var original = @"
using System;

long ticks = 30;
var timeout = {|#0:new TimeSpan(ticks)|};";

        var result = @"
using System;

long ticks = 30;
var timeout = TimeSpan.FromTicks(ticks);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("TimeSpan was constructed with a single-parameter constructor which accepts ticks (100 nanoseconds). Use TimeSpan.FromSeconds(), TimeSpan.FromMilliseconds() or similar instead"), result);
    }

    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_MethodCall()
    {
        var original = @"
using System;

long GetTicks() => 30;
var timeout = {|#0:new TimeSpan(GetTicks())|};";

        var result = @"
using System;

long GetTicks() => 30;
var timeout = TimeSpan.FromTicks(GetTicks());";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("TimeSpan was constructed with a single-parameter constructor which accepts ticks (100 nanoseconds). Use TimeSpan.FromSeconds(), TimeSpan.FromMilliseconds() or similar instead"), result);
    }

    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_DateTimeTicks()
    {
        var original = @"
using System;

var timeout = {|#0:new TimeSpan(DateTime.UtcNow.Ticks)|};";

        var result = @"
using System;

var timeout = TimeSpan.FromTicks(DateTime.UtcNow.Ticks);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("TimeSpan was constructed with a single-parameter constructor which accepts ticks (100 nanoseconds). Use TimeSpan.FromSeconds(), TimeSpan.FromMilliseconds() or similar instead"), result);
    }

    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_FromTicksFactory_NoDiagnostic()
    {
        var original = @"
using System;

var timeout = TimeSpan.FromTicks(30);";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_FromSecondsFactory_NoDiagnostic()
    {
        var original = @"
using System;

var timeout = TimeSpan.FromSeconds(30);";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_InMethodArgument()
    {
        var original = @"
using System;

class MyClass
{
    void Method()
    {
        Console.WriteLine({|#0:new TimeSpan(30)|});
    }
}";

        var result = @"
using System;

class MyClass
{
    void Method()
    {
        Console.WriteLine(TimeSpan.FromTicks(30));
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("TimeSpan was constructed with a single-parameter constructor which accepts ticks (100 nanoseconds). Use TimeSpan.FromSeconds(), TimeSpan.FromMilliseconds() or similar instead"), result);
    }

    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_InFieldInitializer()
    {
        var original = @"
using System;

class MyClass
{
    private TimeSpan _timeout = {|#0:new TimeSpan(30)|};
}";

        var result = @"
using System;

class MyClass
{
    private TimeSpan _timeout = TimeSpan.FromTicks(30);
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("TimeSpan was constructed with a single-parameter constructor which accepts ticks (100 nanoseconds). Use TimeSpan.FromSeconds(), TimeSpan.FromMilliseconds() or similar instead"), result);
    }

    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_ConstantExpression()
    {
        var original = @"
using System;

class MyClass
{
    void Method()
    {
        const long ticks = 100;
        var timeout = {|#0:new TimeSpan(ticks)|};
    }
}";

        var result = @"
using System;

class MyClass
{
    void Method()
    {
        const long ticks = 100;
        var timeout = TimeSpan.FromTicks(ticks);
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("TimeSpan was constructed with a single-parameter constructor which accepts ticks (100 nanoseconds). Use TimeSpan.FromSeconds(), TimeSpan.FromMilliseconds() or similar instead"), result);
    }

    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_SelfDefinedTimeSpan_NoDiagnostic()
    {
        var original = @"
class TimeSpan
{
    public TimeSpan(long value) { }
}

class MyClass
{
    void Method()
    {
        var timeout = new TimeSpan(30);
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_NegativeLiteral()
    {
        var original = @"
using System;

var timeout = {|#0:new TimeSpan(-1)|};";

        var result = @"
using System;

var timeout = TimeSpan.FromTicks(-1);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("TimeSpan was constructed with a single-parameter constructor which accepts ticks (100 nanoseconds). Use TimeSpan.FromSeconds(), TimeSpan.FromMilliseconds() or similar instead"), result);
    }

    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_NoParameters_NoDiagnostic()
    {
        var original = @"
using System;

var timeout = new TimeSpan();";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_ImplicitCreation()
    {
        var original = @"
using System;

TimeSpan timeout = {|#0:new(30)|};";

        var result = @"
using System;

TimeSpan timeout = TimeSpan.FromTicks(30);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("TimeSpan was constructed with a single-parameter constructor which accepts ticks (100 nanoseconds). Use TimeSpan.FromSeconds(), TimeSpan.FromMilliseconds() or similar instead"), result);
    }

    [TestMethod]
    public async Task TimeSpanConstructedWithTicks_EnumCast()
    {
        var original = @"
using System;

class MyClass
{
    void Method()
    {
        var timeout = {|#0:new TimeSpan((long)DateTimeKind.Utc)|};
    }
}";

        var result = @"
using System;

class MyClass
{
    void Method()
    {
        var timeout = TimeSpan.FromTicks((long)DateTimeKind.Utc);
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("TimeSpan was constructed with a single-parameter constructor which accepts ticks (100 nanoseconds). Use TimeSpan.FromSeconds(), TimeSpan.FromMilliseconds() or similar instead"), result);
    }
}
