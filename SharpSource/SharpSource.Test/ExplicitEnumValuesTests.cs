using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.ExplicitEnumValuesAnalyzer, SharpSource.Diagnostics.ExplicitEnumValuesCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class ExplicitEnumValuesTests
{
    [TestMethod]
    public async Task ExplicitEnumValues_NotSpecified()
    {
        var original = @"
enum Test {
    {|#0:A|}
}";

        var result = @"
enum Test {
    A = 0
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Option A on enum Test should explicitly specify its value"), result);
    }

    [TestMethod]
    public async Task ExplicitEnumValues_Specified()
    {
        var original = @"
enum Test {
    A = 0
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ExplicitEnumValues_SomeSpecified()
    {
        var original = @"
enum Test {
    A = 0,
    {|#0:B|}
}";

        var result = @"
enum Test {
    A = 0,
    B = 1
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Option B on enum Test should explicitly specify its value"), result);
    }

    [TestMethod]
    public async Task ExplicitEnumValues_SpecifiedWithReference()
    {
        var original = @"
enum Test {
    A = 0,
    B = A
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ExplicitEnumValues_SpecifiedWithCalculation()
    {
        var original = @"
enum Test {
    A = 1 << 1
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ExplicitEnumValues_NotSpecifiedWithMultiple()
    {
        var original = @"
enum Test {
    {|#0:A|},
    B = 1,
    {|#1:C|}
}";

        var result = @"
enum Test {
    A = 0,
    B = 1,
    C = 2
}";

        await VerifyCS.VerifyCodeFix(original, new[] {
            VerifyCS.Diagnostic(location: 0).WithMessage("Option A on enum Test should explicitly specify its value"),
            VerifyCS.Diagnostic(location: 1).WithMessage("Option C on enum Test should explicitly specify its value")
        }, result);
    }
}