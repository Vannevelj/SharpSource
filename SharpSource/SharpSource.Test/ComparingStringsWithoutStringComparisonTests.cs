using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class ComparingStringsWithoutStringComparisonTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new ComparingStringsWithoutStringComparisonAnalyzer();

    protected override CodeFixProvider CodeFixProvider => new ComparingStringsWithoutStringComparisonCodeFix();

    [TestMethod]
    [DataRow("ToLower", "OrdinalIgnoreCase")]
    [DataRow("ToUpper", "OrdinalIgnoreCase")]
    [DataRow("ToLowerInvariant", "InvariantCultureIgnoreCase")]
    [DataRow("ToUpperInvariant", "InvariantCultureIgnoreCase")]
    public async Task ComparingStringsWithoutStringComparison_EqualsEquals(string call, string expectedStringComparison)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = s1.{call}() == s2.{call}();";

        var result = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = string.Equals(s1, s2, StringComparison.{expectedStringComparison});";

        await VerifyDiagnostic(original, "A string is being compared through allocating a new string. Use a case-insensitive comparison instead.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    [DataRow("ToLower", "OrdinalIgnoreCase")]
    [DataRow("ToUpper", "OrdinalIgnoreCase")]
    [DataRow("ToLowerInvariant", "InvariantCultureIgnoreCase")]
    [DataRow("ToUpperInvariant", "InvariantCultureIgnoreCase")]
    public async Task ComparingStringsWithoutStringComparison_NotEquals(string call, string expectedStringComparison)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = s1.{call}() != s2.{call}();";

        var result = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = !string.Equals(s1, s2, StringComparison.{expectedStringComparison});";

        await VerifyDiagnostic(original, "A string is being compared through allocating a new string. Use a case-insensitive comparison instead.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    [DataRow("ToLower", "OrdinalIgnoreCase")]
    [DataRow("ToUpper", "OrdinalIgnoreCase")]
    [DataRow("ToLowerInvariant", "InvariantCultureIgnoreCase")]
    [DataRow("ToUpperInvariant", "InvariantCultureIgnoreCase")]
    public async Task ComparingStringsWithoutStringComparison_Nullable(string call, string expectedStringComparison)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = s1?.{call}() == s2?.{call}();";

        var result = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = string.Equals(s1, s2, StringComparison.{expectedStringComparison});";

        await VerifyDiagnostic(original, "A string is being compared through allocating a new string. Use a case-insensitive comparison instead.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    [DataRow("ToLower")]
    [DataRow("ToUpper")]
    [DataRow("ToLowerInvariant")]
    [DataRow("ToUpperInvariant")]
    public async Task ComparingStringsWithoutStringComparison_NullableChained(string call)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = s1?.{call}().Trim() == s2?.{call}().Trim();";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    [DataRow("ToLower")]
    [DataRow("ToUpper")]
    [DataRow("ToLowerInvariant")]
    [DataRow("ToUpperInvariant")]
    public async Task ComparingStringsWithoutStringComparison_NullableChainedMultiple_CompareInStartOfChain(string call)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = s1?.{call}().Trim().ToString() == s2?.{call}().Trim().ToString();";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    [DataRow("ToLower")]
    [DataRow("ToUpper")]
    [DataRow("ToLowerInvariant")]
    [DataRow("ToUpperInvariant")]
    public async Task ComparingStringsWithoutStringComparison_NullableChainedMultiple_CompareInMiddleOfChain(string call)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = s1?.Trim().{call}().ToString() == s2?.Trim().{call}().ToString();";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    [DataRow("ToLower")]
    [DataRow("ToUpper")]
    [DataRow("ToLowerInvariant")]
    [DataRow("ToUpperInvariant")]
    public async Task ComparingStringsWithoutStringComparison_NullableChainedMultiple_CompareInEndOfChain(string call)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = s1?.Trim().ToString().{call}() == s2?.Trim().ToString().{call}();";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    [DataRow("ToLower", "OrdinalIgnoreCase")]
    [DataRow("ToUpper", "OrdinalIgnoreCase")]
    [DataRow("ToLowerInvariant", "InvariantCultureIgnoreCase")]
    [DataRow("ToUpperInvariant", "InvariantCultureIgnoreCase")]
    public async Task ComparingStringsWithoutStringComparison_ForceNotNullable(string call, string expectedStringComparison)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = s1!.{call}() == s2!.{call}();";

        var result = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = string.Equals(s1, s2, StringComparison.{expectedStringComparison});";

        await VerifyDiagnostic(original, "A string is being compared through allocating a new string. Use a case-insensitive comparison instead.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    [DataRow("ToLower", "OrdinalIgnoreCase")]
    [DataRow("ToUpper", "OrdinalIgnoreCase")]
    [DataRow("ToLowerInvariant", "InvariantCultureIgnoreCase")]
    [DataRow("ToUpperInvariant", "InvariantCultureIgnoreCase")]
    public async Task ComparingStringsWithoutStringComparison_ForceNotNullableChained(string call, string expectedStringComparison)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = s1!.{call}().Trim() == s2!.{call}().Trim();";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    [DataRow( "OrdinalIgnoreCase")]
    [DataRow( "OrdinalIgnoreCase")]
    [DataRow( "InvariantCultureIgnoreCase")]
    [DataRow( "InvariantCultureIgnoreCase")]
    public async Task ComparingStringsWithoutStringComparison_AlreadyUsingStringComparison(string expectedStringComparison)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = string.Equals(s1, s2, StringComparison.{expectedStringComparison});";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    [DataRow("ToLower", "OrdinalIgnoreCase")]
    [DataRow("ToUpper", "OrdinalIgnoreCase")]
    [DataRow("ToLowerInvariant", "InvariantCultureIgnoreCase")]
    [DataRow("ToUpperInvariant", "InvariantCultureIgnoreCase")]
    public async Task ComparingStringsWithoutStringComparison_WithoutReference(string call, string expectedStringComparison)
    {
        var original = @$"
using System;

string s1 = string.Empty;
bool result = s1.{call}() == ""TeSt"";";

        var result = @$"
using System;

string s1 = string.Empty;
bool result = string.Equals(s1, ""TeSt"", StringComparison.{expectedStringComparison});";

        await VerifyDiagnostic(original, "A string is being compared through allocating a new string. Use a case-insensitive comparison instead.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    [DataRow("ToLower")]
    [DataRow("ToUpper")]
    [DataRow("ToLowerInvariant")]
    [DataRow("ToUpperInvariant")]
    public async Task ComparingStringsWithoutStringComparison_WithPostfixedCalls(string call)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = s1.{call}().Trim() == s2.{call}().Trim();";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    [DataRow("ToLower")]
    [DataRow("ToUpper")]
    [DataRow("ToLowerInvariant")]
    [DataRow("ToUpperInvariant")]
    public async Task ComparingStringsWithoutStringComparison_WithPrefixedCalls(string call)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = s1.Trim().{call}() == s2.Trim().{call}();";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    [DataRow("ToLower", "OrdinalIgnoreCase")]
    [DataRow("ToUpper", "OrdinalIgnoreCase")]
    [DataRow("ToLowerInvariant", "InvariantCultureIgnoreCase")]
    [DataRow("ToUpperInvariant", "InvariantCultureIgnoreCase")]
    public async Task ComparingStringsWithoutStringComparison_Is(string call, string expectedStringComparison)
    {
        var original = @$"
using System;

string s1 = string.Empty;
bool result = s1.{call}() is ""test"";";

        var result = @$"
using System;

string s1 = string.Empty;
bool result = string.Equals(s1, ""test"", StringComparison.{expectedStringComparison});";

        await VerifyDiagnostic(original, "A string is being compared through allocating a new string. Use a case-insensitive comparison instead.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    [DataRow("ToLower", "OrdinalIgnoreCase")]
    [DataRow("ToUpper", "OrdinalIgnoreCase")]
    [DataRow("ToLowerInvariant", "InvariantCultureIgnoreCase")]
    [DataRow("ToUpperInvariant", "InvariantCultureIgnoreCase")]
    public async Task ComparingStringsWithoutStringComparison_IsNot(string call, string expectedStringComparison)
    {
        var original = @$"
using System;

string s1 = string.Empty;
bool result = s1.{call}() is not ""test"";";

        var result = @$"
using System;

string s1 = string.Empty;
bool result = !string.Equals(s1, ""test"", StringComparison.{expectedStringComparison});";

        await VerifyDiagnostic(original, "A string is being compared through allocating a new string. Use a case-insensitive comparison instead.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    [DataRow("ToLower")]
    [DataRow("ToUpper")]
    [DataRow("ToLowerInvariant")]
    [DataRow("ToUpperInvariant")]
    public async Task ComparingStringsWithoutStringComparison_WrappedInAnother(string call)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = GetValue(s1.{call}()) == GetValue(s2.{call}());
string GetValue(string s) => s;";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    [DataRow("ToLower", "OrdinalIgnoreCase")]
    [DataRow("ToUpper", "OrdinalIgnoreCase")]
    [DataRow("ToLowerInvariant", "InvariantCultureIgnoreCase")]
    [DataRow("ToUpperInvariant", "InvariantCultureIgnoreCase")]
    public async Task ComparingStringsWithoutStringComparison_ReferencingProperty(string call, string expectedStringComparison)
    {
        var original = @$"
using System;

bool result = T.Name.{call}() == T.Name.{call}();

class T
{{
    public static string Name {{ get; set; }}
}}";

        var result = @$"
using System;

bool result = string.Equals(T.Name, T.Name, StringComparison.{expectedStringComparison});

class T
{{
    public static string Name {{ get; set; }}
}}";

        await VerifyDiagnostic(original, "A string is being compared through allocating a new string. Use a case-insensitive comparison instead.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    [DataRow("ToLower", "OrdinalIgnoreCase")]
    [DataRow("ToUpper", "OrdinalIgnoreCase")]
    [DataRow("ToLowerInvariant", "InvariantCultureIgnoreCase")]
    [DataRow("ToUpperInvariant", "InvariantCultureIgnoreCase")]
    public async Task ComparingStringsWithoutStringComparison_ReferencingThis(string call, string expectedStringComparison)
    {
        var original = @$"
using System;

class Test
{{
    string _name;
    bool IsValid() => this._name.{call}() == this._name.{call}();
}}";

        var result = @$"
using System;

class Test
{{
    string _name;
    bool IsValid() => string.Equals(this._name, this._name, StringComparison.{expectedStringComparison});
}}";

        await VerifyDiagnostic(original, "A string is being compared through allocating a new string. Use a case-insensitive comparison instead.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task ComparingStringsWithoutStringComparison_AddsUsingStatement()
    {
        var original = @$"
string s1 = ""first"";
bool result = s1.ToLower() is ""test"";";

        var result = @$"using System;

string s1 = ""first"";
bool result = string.Equals(s1, ""test"", StringComparison.OrdinalIgnoreCase);";

        await VerifyDiagnostic(original, "A string is being compared through allocating a new string. Use a case-insensitive comparison instead.");
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task ComparingStringsWithoutStringComparison_PassedAsArgument()
    {
        var original = @$"
using System;

string s1 = ""first"";
Method(s1.ToLower() is ""test"");

void Method(bool b) {{ }}";

        var result = @$"
using System;

string s1 = ""first"";
Method(string.Equals(s1, ""test"", StringComparison.OrdinalIgnoreCase));

void Method(bool b) {{ }}";

        await VerifyDiagnostic(original, "A string is being compared through allocating a new string. Use a case-insensitive comparison instead.");
        await VerifyFix(original, result);
    }
}