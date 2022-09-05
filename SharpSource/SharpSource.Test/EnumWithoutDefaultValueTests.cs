using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class EnumWithoutDefaultValueTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new EnumWithoutDefaultValueAnalyzer();

    [TestMethod]
    public async Task EnumWithoutDefaultValue_WrongName()
    {
        var original = @"
enum Test {
    A
}";

        await VerifyDiagnostic(original, "Enum Test should specify a default value of 0 as \"Unknown\" or \"None\"");
    }

    [TestMethod]
    public async Task EnumWithoutDefaultValue_NoMembers()
    {
        var original = @"
enum Test {
    
}";

        await VerifyDiagnostic(original, "Enum Test should specify a default value of 0 as \"Unknown\" or \"None\"");
    }

    [TestMethod]
    [DataRow("None")]
    [DataRow("Unknown")]
    public async Task EnumWithoutDefaultValue_RightName_WrongValue(string memberName)
    {
        var original = $@"
enum Test {{
    A,
    {memberName} = 1
}}";

        await VerifyDiagnostic(original, "Enum Test should specify a default value of 0 as \"Unknown\" or \"None\"");
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

        await VerifyDiagnostic(original);
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

        await VerifyDiagnostic(original);
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

        await VerifyDiagnostic(original);
    }
}