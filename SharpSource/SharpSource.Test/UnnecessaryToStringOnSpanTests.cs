using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.UnnecessaryToStringOnSpanAnalyzer, SharpSource.Diagnostics.UnnecessaryToStringOnSpanCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class UnnecessaryToStringOnSpanTests
{
    [TestMethod]
    public async Task UnnecessaryToStringOnSpan_IntParse_ReadOnlySpan()
    {
        var original = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""12345"".AsSpan();
        int.Parse({|#0:input.ToString()|});
    }
}";

        var result = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""12345"".AsSpan();
        int.Parse(input);
    }
}";
        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Unnecessary ToString() call, an overload that accepts ReadOnlySpan<char> is available"), result);
    }

    [TestMethod]
    public async Task UnnecessaryToStringOnSpan_IntTryParse_ReadOnlySpan()
    {
        var original = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""12345"".AsSpan();
        int.TryParse({|#0:input.ToString()|}, out int id);
    }
}";

        var result = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""12345"".AsSpan();
        int.TryParse(input, out int id);
    }
}";
        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Unnecessary ToString() call, an overload that accepts ReadOnlySpan<char> is available"), result);
    }

    [TestMethod]
    public async Task UnnecessaryToStringOnSpan_DateTimeParse_ReadOnlySpan()
    {
        var original = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""2023-01-01"".AsSpan();
        DateTime.Parse({|#0:input.ToString()|});
    }
}";

        var result = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""2023-01-01"".AsSpan();
        DateTime.Parse(input);
    }
}";
        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Unnecessary ToString() call, an overload that accepts ReadOnlySpan<char> is available"), result);
    }

    [TestMethod]
    public async Task UnnecessaryToStringOnSpan_StringBuilderAppend_ReadOnlySpan()
    {
        var original = @"
using System;
using System.Text;

class Test
{
    void Method()
    {
        var sb = new StringBuilder();
        ReadOnlySpan<char> value = ""test"".AsSpan();
        sb.Append({|#0:value.ToString()|});
    }
}";

        var result = @"
using System;
using System.Text;

class Test
{
    void Method()
    {
        var sb = new StringBuilder();
        ReadOnlySpan<char> value = ""test"".AsSpan();
        sb.Append(value);
    }
}";
        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Unnecessary ToString() call, an overload that accepts ReadOnlySpan<char> is available"), result);
    }

    [TestMethod]
    public async Task UnnecessaryToStringOnSpan_SpanChar()
    {
        var original = @"
using System;

class Test
{
    void Method()
    {
        Span<char> input = stackalloc char[] { '1', '2', '3' };
        int.Parse({|#0:input.ToString()|});
    }
}";

        var result = @"
using System;

class Test
{
    void Method()
    {
        Span<char> input = stackalloc char[] { '1', '2', '3' };
        int.Parse(input);
    }
}";
        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Unnecessary ToString() call, an overload that accepts Span<char> is available"), result);
    }

    [TestMethod]
    public async Task UnnecessaryToStringOnSpan_NoOverload_NoDiagnostic()
    {
        var original = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""test"".AsSpan();
        Console.WriteLine(input.ToString());
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task UnnecessaryToStringOnSpan_NotUsedAsArgument_NoDiagnostic()
    {
        var original = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""test"".AsSpan();
        var str = input.ToString();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task UnnecessaryToStringOnSpan_OtherSpanType_NoDiagnostic()
    {
        var original = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<int> input = stackalloc int[] { 1, 2, 3 };
        var str = input.ToString();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task UnnecessaryToStringOnSpan_CustomMethodWithSpanOverload()
    {
        var original = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""test"".AsSpan();
        Process({|#0:input.ToString()|});
    }

    void Process(string value) { }
    void Process(ReadOnlySpan<char> value) { }
}";

        var result = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""test"".AsSpan();
        Process(input);
    }

    void Process(string value) { }
    void Process(ReadOnlySpan<char> value) { }
}";
        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Unnecessary ToString() call, an overload that accepts ReadOnlySpan<char> is available"), result);
    }

    [TestMethod]
    public async Task UnnecessaryToStringOnSpan_DoubleParse()
    {
        var original = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""3.14"".AsSpan();
        double.Parse({|#0:input.ToString()|});
    }
}";

        var result = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""3.14"".AsSpan();
        double.Parse(input);
    }
}";
        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Unnecessary ToString() call, an overload that accepts ReadOnlySpan<char> is available"), result);
    }

    [TestMethod]
    public async Task UnnecessaryToStringOnSpan_GuidParse()
    {
        var original = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""12345678-1234-1234-1234-123456789012"".AsSpan();
        Guid.Parse({|#0:input.ToString()|});
    }
}";

        var result = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""12345678-1234-1234-1234-123456789012"".AsSpan();
        Guid.Parse(input);
    }
}";
        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Unnecessary ToString() call, an overload that accepts ReadOnlySpan<char> is available"), result);
    }

    [TestMethod]
    public async Task UnnecessaryToStringOnSpan_InlineExpression()
    {
        var original = @"
using System;

class Test
{
    void Method()
    {
        int.Parse({|#0:""12345"".AsSpan().ToString()|});
    }
}";

        var result = @"
using System;

class Test
{
    void Method()
    {
        int.Parse(""12345"".AsSpan());
    }
}";
        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Unnecessary ToString() call, an overload that accepts ReadOnlySpan<char> is available"), result);
    }

    [TestMethod]
    public async Task UnnecessaryToStringOnSpan_MultipleArguments_FirstArgument()
    {
        var original = @"
using System;
using System.Globalization;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""12345"".AsSpan();
        int.Parse({|#0:input.ToString()|}, NumberStyles.Integer);
    }
}";

        var result = @"
using System;
using System.Globalization;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""12345"".AsSpan();
        int.Parse(input, NumberStyles.Integer);
    }
}";
        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Unnecessary ToString() call, an overload that accepts ReadOnlySpan<char> is available"), result);
    }

    [TestMethod]
    public async Task UnnecessaryToStringOnSpan_StringContains_NoDiagnostic()
    {
        // string.Contains doesn't have a ReadOnlySpan<char> overload that returns bool
        var original = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""test"".AsSpan();
        ""hello world"".Contains(input.ToString());
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task UnnecessaryToStringOnSpan_EnumTryParse_NoDiagnostic()
    {
        // Enum.TryParse<T> doesn't have a span overload in .NET 8
        var original = @"
using System;

enum MyEnum { Value1, Value2 }

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""Value1"".AsSpan();
        Enum.TryParse<MyEnum>(input.ToString(), out _);
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task UnnecessaryToStringOnSpan_SlicedSpan()
    {
        var original = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""ID:12345"".AsSpan();
        int.Parse({|#0:input.Slice(3).ToString()|});
    }
}";

        var result = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""ID:12345"".AsSpan();
        int.Parse(input.Slice(3));
    }
}";
        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Unnecessary ToString() call, an overload that accepts ReadOnlySpan<char> is available"), result);
    }

    [TestMethod]
    public async Task UnnecessaryToStringOnSpan_NoMatchingOverloadParameters_NoDiagnostic()
    {
        // Ensure we don't flag when the overload has incompatible additional parameters
        var original = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""test"".AsSpan();
        Process(input.ToString(), 42);
    }

    void Process(string value, int number) { }
    void Process(ReadOnlySpan<char> value, string other) { }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task UnnecessaryToStringOnSpan_VersionParse()
    {
        var original = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""1.2.3.4"".AsSpan();
        Version.Parse({|#0:input.ToString()|});
    }
}";

        var result = @"
using System;

class Test
{
    void Method()
    {
        ReadOnlySpan<char> input = ""1.2.3.4"".AsSpan();
        Version.Parse(input);
    }
}";
        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Unnecessary ToString() call, an overload that accepts ReadOnlySpan<char> is available"), result);
    }
}
