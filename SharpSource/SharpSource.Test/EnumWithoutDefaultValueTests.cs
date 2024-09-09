using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.EnumWithoutDefaultValueAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class EnumWithoutDefaultValueTests
{
    [TestMethod]
    public async Task EnumWithoutDefaultValue_WrongName()
    {
        var original = @"
enum {|#0:Test|} {
    A
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Enum Test should specify a default value of 0 as \"Unknown\" or \"None\""));
    }

    [TestMethod]
    public async Task EnumWithoutDefaultValue_NoMembers()
    {
        var original = @"
enum {|#0:Test|} {
    
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Enum Test should specify a default value of 0 as \"Unknown\" or \"None\""));
    }

    [TestMethod]
    [DataRow("None")]
    [DataRow("Unknown")]
    public async Task EnumWithoutDefaultValue_RightName_WrongValue(string memberName)
    {
        var original = $@"
enum {{|#0:Test|}} {{
    A,
    {memberName} = 1
}}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Enum Test should specify a default value of 0 as \"Unknown\" or \"None\""));
    }

    [TestMethod]
    [DataRow("None")]
    [DataRow("Unknown")]
    public async Task EnumWithoutDefaultValue_RightName_RightValueExplicit(string memberName)
    {
        var original = $@"
enum Test {{
    {memberName} = 0,
    A = 1
}}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    [DataRow("None")]
    [DataRow("Unknown")]
    public async Task EnumWithoutDefaultValue_RightName_RightValueImplicit(string memberName)
    {
        var original = $@"
enum Test {{
    {memberName},
    A = 1
}}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    [DataRow("None")]
    [DataRow("Unknown")]
    public async Task EnumWithoutDefaultValue_RightName_RightValueDuplicated(string memberName)
    {
        var original = $@"
enum Test {{
    A = 0,
    {memberName} = 0,
}}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    [DataRow("None")]
    [DataRow("Unknown")]
    public async Task EnumWithoutDefaultValue_RightName_WithDifferentType(string memberName)
    {
        var original = $@"
enum Test : ushort {{
   {memberName} = 0,

}}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}