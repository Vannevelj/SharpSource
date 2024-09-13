using System.Collections.Generic;
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
    public async Task EnumWithoutDefaultValue_RightName_NegativeValue(string memberName)
    {
        var original = $@"
enum [|Test|] : short {{
    {memberName} = -1
}}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    [DynamicData(nameof(GetEnumTypes), DynamicDataSourceType.Method)]
    public async Task EnumWithoutDefaultValue_RightName_WithDifferentType(string memberName, string dataType)
    {
        var original = $@"
enum Test : {dataType} {{
   {memberName} = 0
}}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    public static IEnumerable<object[]> GetEnumTypes()
    {
        var memberNames = new[] { "None", "Unknown" };
        var dataTypes = new[] { "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong" };

        foreach (var memberName in memberNames)
        {
            foreach (var dataType in dataTypes)
            {
                yield return new object[] { memberName, dataType };
            }
        }
    }
}