using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers.Helpers;

namespace SharpSource.Test;

[TestClass]
public class ExplicitEnumValuesTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new ExplicitEnumValuesAnalyzer();

    protected override CodeFixProvider CodeFixProvider => new ExplicitEnumValuesCodeFix();

    [TestMethod]
    public void ExplicitEnumValues_NotSpecified()
    {
        var original = @"
enum Test {
    A
}";

        var result = @"
enum Test {
    A = 0
}";

        VerifyDiagnostic(original, "Option A on enum Test should explicitly specify its value");
        VerifyFix(original, result);
    }

    [TestMethod]
    public void ExplicitEnumValues_Specified()
    {
        var original = @"
enum Test {
    A = 0
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ExplicitEnumValues_SomeSpecified()
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

        VerifyDiagnostic(original, "Option B on enum Test should explicitly specify its value");
        VerifyFix(original, result);
    }

    [TestMethod]
    public void ExplicitEnumValues_SpecifiedWithReference()
    {
        var original = @"
enum Test {
    A = 0,
    B = A
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ExplicitEnumValues_SpecifiedWithCalculation()
    {
        var original = @"
enum Test {
    A = 1 << 1
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ExplicitEnumValues_NotSpecifiedWithMultiple()
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

        VerifyDiagnostic(original, "Option A on enum Test should explicitly specify its value", "Option C on enum Test should explicitly specify its value");
        VerifyFix(original, result);
    }
}