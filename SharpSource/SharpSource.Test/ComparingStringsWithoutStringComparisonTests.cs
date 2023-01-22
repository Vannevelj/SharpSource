using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Test.Helpers;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.ComparingStringsWithoutStringComparisonAnalyzer, SharpSource.Diagnostics.ComparingStringsWithoutStringComparisonCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class ComparingStringsWithoutStringComparisonTests
{

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
bool result = {{|#0:s1.{call}()|}} == s2.{call}();";

        var result = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = string.Equals(s1, s2, StringComparison.{expectedStringComparison});";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A string is being compared through allocating a new string. Use a case-insensitive comparison instead."), result);
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
bool result = {{|#0:s1.{call}()|}} != s2.{call}();";

        var result = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = !string.Equals(s1, s2, StringComparison.{expectedStringComparison});";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A string is being compared through allocating a new string. Use a case-insensitive comparison instead."), result);
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
bool result = {{|#0:s1?.{call}()|}} == s2?.{call}();";

        var result = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = string.Equals(s1, s2, StringComparison.{expectedStringComparison});";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A string is being compared through allocating a new string. Use a case-insensitive comparison instead."), result);
    }

    [TestMethod]
    [Ignore("See https://github.com/Vannevelj/SharpSource/issues/190")]
    [DataRow("ToLower", "OrdinalIgnoreCase")]
    [DataRow("ToUpper", "OrdinalIgnoreCase")]
    [DataRow("ToLowerInvariant", "InvariantCultureIgnoreCase")]
    [DataRow("ToUpperInvariant", "InvariantCultureIgnoreCase")]
    public async Task ComparingStringsWithoutStringComparison_NullableChained(string call, string expectedStringComparison)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = {{|#0:s1?.{call}().Trim()|}} == s2?.{call}().Trim();";

        var result = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = string.Equals(s1?.Trim(), s2?.Trim(), StringComparison.{expectedStringComparison});";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A string is being compared through allocating a new string. Use a case-insensitive comparison instead."), result);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    [Ignore("See https://github.com/Vannevelj/SharpSource/issues/190")]
    [DataRow("ToLower", "OrdinalIgnoreCase")]
    [DataRow("ToUpper", "OrdinalIgnoreCase")]
    [DataRow("ToLowerInvariant", "InvariantCultureIgnoreCase")]
    [DataRow("ToUpperInvariant", "InvariantCultureIgnoreCase")]
    public async Task ComparingStringsWithoutStringComparison_NullableChainedMultiple_CompareInEndOfChain(string call, string expectedStringComparison)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = {{|#0:s1?.Trim().ToString().{call}()|}} == s2?.Trim().ToString().{call}();";

        var result = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = string.Equals(s1?.Trim().ToString(), s2?.Trim().ToString(), StringComparison.{expectedStringComparison});";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A string is being compared through allocating a new string. Use a case-insensitive comparison instead."), result);
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
bool result = {{|#0:s1!.{call}()|}} == s2!.{call}();";

        var result = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = string.Equals(s1, s2, StringComparison.{expectedStringComparison});";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A string is being compared through allocating a new string. Use a case-insensitive comparison instead."), result);
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    [DataRow("OrdinalIgnoreCase")]
    [DataRow("OrdinalIgnoreCase")]
    [DataRow("InvariantCultureIgnoreCase")]
    [DataRow("InvariantCultureIgnoreCase")]
    public async Task ComparingStringsWithoutStringComparison_AlreadyUsingStringComparison(string expectedStringComparison)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = string.Equals(s1, s2, StringComparison.{expectedStringComparison});";

        await VerifyCS.VerifyNoDiagnostic(original);
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
bool result = {{|#0:s1.{call}()|}} == ""TeSt"";";

        var result = @$"
using System;

string s1 = string.Empty;
bool result = string.Equals(s1, ""TeSt"", StringComparison.{expectedStringComparison});";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A string is being compared through allocating a new string. Use a case-insensitive comparison instead."), result);
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    [DataRow("ToLower", "OrdinalIgnoreCase")]
    [DataRow("ToUpper", "OrdinalIgnoreCase")]
    [DataRow("ToLowerInvariant", "InvariantCultureIgnoreCase")]
    [DataRow("ToUpperInvariant", "InvariantCultureIgnoreCase")]
    public async Task ComparingStringsWithoutStringComparison_Chained(string call, string expectedStringComparison)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = {{|#0:s1.Trim().{call}()|}} == s2.Trim().{call}();";

        var result = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = string.Equals(s1.Trim(), s2.Trim(), StringComparison.{expectedStringComparison});";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A string is being compared through allocating a new string. Use a case-insensitive comparison instead."), result);
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
bool result = {{|#0:s1.{call}()|}} is ""test"";";

        var result = @$"
using System;

string s1 = string.Empty;
bool result = string.Equals(s1, ""test"", StringComparison.{expectedStringComparison});";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A string is being compared through allocating a new string. Use a case-insensitive comparison instead."), result);
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
bool result = {{|#0:s1.{call}()|}} is not ""test"";";

        var result = @$"
using System;

string s1 = string.Empty;
bool result = !string.Equals(s1, ""test"", StringComparison.{expectedStringComparison});";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A string is being compared through allocating a new string. Use a case-insensitive comparison instead."), result);
    }

    [TestMethod]
    [DataRow("ToLower")]
    [DataRow("ToUpper")]
    [DataRow("ToLowerInvariant")]
    [DataRow("ToUpperInvariant")]
    public async Task ComparingStringsWithoutStringComparison_IsOr(string call)
    {
        var original = @$"
using System;

string s1 = string.Empty;
bool result = s1.{call}() is (""test"" or ""other"");";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    [DataRow("ToLower")]
    [DataRow("ToUpper")]
    [DataRow("ToLowerInvariant")]
    [DataRow("ToUpperInvariant")]
    public async Task ComparingStringsWithoutStringComparison_IsNotOr(string call)
    {
        var original = @$"
using System;

string s1 = string.Empty;
bool result = s1.{call}() is not (""test"" or ""other"");";

        await VerifyCS.VerifyNoDiagnostic(original);
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

        await VerifyCS.VerifyNoDiagnostic(original);
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

bool result = {{|#0:T.Name.{call}()|}} == T.Name.{call}();

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

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A string is being compared through allocating a new string. Use a case-insensitive comparison instead."), result);
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
    bool IsValid() => {{|#0:this._name.{call}()|}} == this._name.{call}();
}}";

        var result = @$"
using System;

class Test
{{
    string _name;
    bool IsValid() => string.Equals(this._name, this._name, StringComparison.{expectedStringComparison});
}}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A string is being compared through allocating a new string. Use a case-insensitive comparison instead."), result);
    }

    [TestMethod]
    public async Task ComparingStringsWithoutStringComparison_AddsUsingStatement()
    {
        var original = @$"
string s1 = ""first"";
bool result = {{|#0:s1.ToLower()|}} is ""test"";";

        var result = @$"
using System;

string s1 = ""first"";
bool result = string.Equals(s1, ""test"", StringComparison.OrdinalIgnoreCase);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A string is being compared through allocating a new string. Use a case-insensitive comparison instead."), result);
    }

    [TestMethod]
    public async Task ComparingStringsWithoutStringComparison_PassedAsArgument()
    {
        var original = @$"
using System;

string s1 = ""first"";
Method({{|#0:s1.ToLower()|}} is ""test"");

void Method(bool b) {{ }}";

        var result = @$"
using System;

string s1 = ""first"";
Method(string.Equals(s1, ""test"", StringComparison.OrdinalIgnoreCase));

void Method(bool b) {{ }}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A string is being compared through allocating a new string. Use a case-insensitive comparison instead."), result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/56")]
    [Ignore("Code fix introduces a \r\n newline which fails the equality check because the test expects \n. Presumably fixed when https://github.com/Vannevelj/SharpSource/issues/274 is done")]
    public async Task ComparingStringsWithoutStringComparison_WithOtherUsingStatements()
    {
        var original = @"
using System.Text;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = {|#0:s1.ToLower()|} == s2.ToLower();";

        var result = @$"
using System.Text;
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A string is being compared through allocating a new string. Use a case-insensitive comparison instead."), result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/256")]
    public async Task ComparingStringsWithoutStringComparison_PreservesTrivia()
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;

bool result = // Compare the two
    {{|#0:s1.ToLower()|}} == s2.ToLower();";

        var result = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;

bool result = // Compare the two
    string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A string is being compared through allocating a new string. Use a case-insensitive comparison instead."), result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/301")]
    public async Task ComparingStringsWithoutStringComparison_PreservesTrailingTrivia()
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;

bool result = {{|#0:s1.ToLower()|}} == s2.ToLower() && true == true;";

        var result = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;

bool result = string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase) && true == true;";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A string is being compared through allocating a new string. Use a case-insensitive comparison instead."), result);
    }

    [TestMethod]
    [DataRow("ToLower")]
    [DataRow("ToUpper")]
    public async Task ComparingStringsWithoutStringComparison_WithArgument_Left(string call)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;

bool result = s1.{call}(System.Globalization.CultureInfo.CurrentCulture) == {{|#0:s2.{call}()|}};";

        var result = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;

bool result = string.Equals(s1.{call}(System.Globalization.CultureInfo.CurrentCulture), s2, StringComparison.OrdinalIgnoreCase);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A string is being compared through allocating a new string. Use a case-insensitive comparison instead."), result);
    }

    [TestMethod]
    [DataRow("ToLower")]
    [DataRow("ToUpper")]
    public async Task ComparingStringsWithoutStringComparison_WithArgument_Right(string call)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;

bool result = {{|#0:s1.{call}()|}} == s2.{call}(System.Globalization.CultureInfo.CurrentCulture);";

        var result = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;

bool result = string.Equals(s1, s2.{call}(System.Globalization.CultureInfo.CurrentCulture), StringComparison.OrdinalIgnoreCase);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("A string is being compared through allocating a new string. Use a case-insensitive comparison instead."), result);
    }

    [TestMethod]
    [DataRow("ToLower")]
    [DataRow("ToUpper")]
    public async Task ComparingStringsWithoutStringComparison_WithArgument_Both(string call)
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = s1.{call}(System.Globalization.CultureInfo.CurrentCulture) == s2.{call}(System.Globalization.CultureInfo.CurrentCulture);";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ComparingStringsWithoutStringComparison_DifferentMethod()
    {
        var original = @$"
using System;

string s1 = string.Empty;
string s2 = string.Empty;
bool result = s1.Trim() == s2;";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}