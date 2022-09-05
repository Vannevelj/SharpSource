using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;
using SharpSource.Test.Helpers.Helpers;

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
    public void UnusedResultOnImmutableObjectTests_UnusedResult(string invocation)
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

        VerifyDiagnostic(original, "The result of an operation on an immutable object is unused");
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
    public void UnusedResultOnImmutableObjectTests_UnusedResult_Global(string invocation)
    {
        var original = $@"
""test"".{invocation};
";

        VerifyDiagnostic(original, "The result of an operation on an immutable object is unused");
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
    public void UnusedResultOnImmutableObjectTests_UnusedResult_WithVariable(string invocation)
    {
        var original = $@"
var str = ""test"";
str.{invocation};
";

        VerifyDiagnostic(original, "The result of an operation on an immutable object is unused");
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
    public void UnusedResultOnImmutableObjectTests_UsedResult(string invocation)
    {
        var original = $@"
var temp = ""test"".{invocation};
";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    [DataRow("if")]
    [DataRow("while")]
    public void UnusedResultOnImmutableObjectTests_UsedResult_InCondition(string condition)
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void UnusedResultOnImmutableObjectTests_UsedResult_InCondition_DoWhile()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void UnusedResultOnImmutableObjectTests_UsedResult_InCondition_Ternary()
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

        VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/82")]
    public void UnusedResultOnImmutableObjectTests_UsedResult_SeparateVariableDefinition()
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

        VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/83")]
    public void UnusedResultOnImmutableObjectTests_UsedResult_AsArgument()
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

        VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/85")]
    public void UnusedResultOnImmutableObjectTests_UsedResult_AsReturnValue()
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

        VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/81")]
    public void UnusedResultOnImmutableObjectTests_UsedResult_InLambda()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void UnusedResultOnImmutableObjectTests_UsedResult_NullCoalescing()
    {
        var original = @"
string Method() => string.Empty ?? """".Trim();
";

        VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/119")]
    [DataRow("CopyTo(Span<char>.Empty)")]
    [DataRow("TryCopyTo(Span<char>.Empty)")]
    public void UnusedResultOnImmutableObjectTests_ExcludedFunctions(string invocation)
    {
        var original = @$"
using System;

"""".{invocation};
";

        VerifyDiagnostic(original);
    }
}