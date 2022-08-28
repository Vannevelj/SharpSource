using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers.Helpers.CSharp;

namespace SharpSource.Test
{
    [TestClass]
    public class UnusedResultOnImmutableObjectTests : CSharpDiagnosticVerifier
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
    }
}