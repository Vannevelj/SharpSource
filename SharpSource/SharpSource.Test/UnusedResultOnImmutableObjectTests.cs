using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class UnusedResultOnImmutableObjectTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new UnusedResultOnImmutableObjectAnalyzer();

    [TestMethod]
    [DataRow("Trim()")]
    [DataRow("Replace(\"e\", \"oa\")")]
    [DataRow("Contains(\"t\")")]
    [DataRow("StartsWith(\"t\")")]
    [DataRow("ToLower()")]
    [DataRow("ToUpper()")]
    [DataRow("Split('e')")]
    [DataRow("PadRight(5)")]
    public async Task UnusedResultOnImmutableObjectTests_UnusedResultAsync(string invocation)
    {
        var original = $@"
class Test
{{
    void Method()
    {{
        ""test"".{invocation};
    }}
}}
";

        await VerifyDiagnostic(original, "The result of an operation on an immutable object is unused");
    }

    [TestMethod]
    [DataRow("Trim()")]
    [DataRow("Replace(\"e\", \"oa\")")]
    [DataRow("Contains(\"t\")")]
    [DataRow("StartsWith(\"t\")")]
    [DataRow("ToLower()")]
    [DataRow("ToUpper()")]
    [DataRow("Split('e')")]
    [DataRow("PadRight(5)")]
    public async Task UnusedResultOnImmutableObjectTests_UnusedResult_GlobalAsync(string invocation)
    {
        var original = $@"
""test"".{invocation};
";

        await VerifyDiagnostic(original, "The result of an operation on an immutable object is unused");
    }

    [TestMethod]
    [DataRow("Trim()")]
    [DataRow("Replace(\"e\", \"oa\")")]
    [DataRow("Contains(\"t\")")]
    [DataRow("StartsWith(\"t\")")]
    [DataRow("ToLower()")]
    [DataRow("ToUpper()")]
    [DataRow("Split('e')")]
    [DataRow("PadRight(5)")]
    public async Task UnusedResultOnImmutableObjectTests_UnusedResult_WithVariableAsync(string invocation)
    {
        var original = $@"
var str = ""test"";
str.{invocation};
";

        await VerifyDiagnostic(original, "The result of an operation on an immutable object is unused");
    }

    [TestMethod]
    [DataRow("Trim()")]
    [DataRow("Replace(\"e\", \"oa\")")]
    [DataRow("Contains(\"t\")")]
    [DataRow("StartsWith(\"t\")")]
    [DataRow("ToLower()")]
    [DataRow("ToUpper()")]
    [DataRow("Split('e')")]
    [DataRow("PadRight(5)")]
    public async Task UnusedResultOnImmutableObjectTests_UsedResultAsync(string invocation)
    {
        var original = $@"
var temp = ""test"".{invocation};
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    [DataRow("if")]
    [DataRow("while")]
    public async Task UnusedResultOnImmutableObjectTests_UsedResult_InConditionAsync(string condition)
    {
        var original = $@"
class Test
{{
    void Method()
    {{
        {condition}(""test"".Contains(""e"")) {{ }}
    }}
}}
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task UnusedResultOnImmutableObjectTests_UsedResult_InCondition_DoWhile()
    {
        var original = @"
class Test
{
    void Method()
    {
        do {

        } while(""test"".Contains(""e""));
    }
}
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task UnusedResultOnImmutableObjectTests_UsedResult_InCondition_Ternary()
    {
        var original = @"
class Test
{
    void Method()
    {
        var x = ""test"".Contains(""e"") ? 1 : 2;
    }
}
";

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/82")]
    public async Task UnusedResultOnImmutableObjectTests_UsedResult_SeparateVariableDefinition()
    {
        var original = @"
class Test
{
    void Method()
    {
        bool x = false;
        x = ""test"".Contains(""e"");
    }
}
";

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/83")]
    public async Task UnusedResultOnImmutableObjectTests_UsedResult_AsArgument()
    {
        var original = @"
class Test
{
    void Method()
    {
        Other(""test"".Contains(""e""));
    }

    void Other(bool b) { }
}
";

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/85")]
    public async Task UnusedResultOnImmutableObjectTests_UsedResult_AsReturnValue()
    {
        var original = @"
class Test
{
    public bool Validate(string id)
    {
	    return !string.IsNullOrWhiteSpace(id);
    }
}
";

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/81")]
    public async Task UnusedResultOnImmutableObjectTests_UsedResult_InLambda()
    {
        var original = @"
using System.Linq;
using System.Collections.Generic;

class Test
{
	private string _id;

	void Method(List<string> ids)
	{
		_id = ids.First(x => !string.IsNullOrEmpty(x));
	}
}
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task UnusedResultOnImmutableObjectTests_UsedResult_NullCoalescing()
    {
        var original = @"
string Method() => string.Empty ?? """".Trim();
";

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/119")]
    [DataRow("CopyTo(Span<char>.Empty)")]
    [DataRow("TryCopyTo(Span<char>.Empty)")]
    public async Task UnusedResultOnImmutableObjectTests_ExcludedFunctionsAsync(string invocation)
    {
        var original = @$"
using System;

"""".{invocation};
";

        await VerifyDiagnostic(original);
    }
}