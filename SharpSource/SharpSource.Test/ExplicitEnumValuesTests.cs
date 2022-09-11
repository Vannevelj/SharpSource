using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class ExplicitEnumValuesTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new ExplicitEnumValuesAnalyzer();

    protected override CodeFixProvider CodeFixProvider => new ExplicitEnumValuesCodeFix();

    [TestMethod]
    public async Task ExplicitEnumValues_NotSpecifiedAsync()
    {
        var original = @"
enum Test {
    A
}";

        var result = @"
enum Test {
    A = 0
}";

        await VerifyDiagnostic(original, "Option A on enum Test should explicitly specify its value");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task ExplicitEnumValues_Specified()
    {
        var original = @"
enum Test {
    A = 0
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ExplicitEnumValues_SomeSpecifiedAsync()
    {
        var original = @"
enum Test {
    A = 0,
    B
}";

        var result = @"
enum Test {
    A = 0,
    B = 1
}";

        await VerifyDiagnostic(original, "Option B on enum Test should explicitly specify its value");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task ExplicitEnumValues_SpecifiedWithReferenceAsync()
    {
        var original = @"
enum Test {
    A = 0,
    B = A
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ExplicitEnumValues_SpecifiedWithCalculationAsync()
    {
        var original = @"
enum Test {
    A = 1 << 1
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task ExplicitEnumValues_NotSpecifiedWithMultipleAsync()
    {
        var original = @"
enum Test {
    A,
    B = 1,
    C
}";

        var result = @"
enum Test {
    A = 0,
    B = 1,
    C = 2
}";

        await VerifyDiagnostic(original, "Option A on enum Test should explicitly specify its value", "Option C on enum Test should explicitly specify its value");
        await VerifyFix(original, result);
    }
}