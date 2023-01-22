using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.LinqTraversalBeforeFilterAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class LinqTraversalBeforeFilterTests
{
    [TestMethod]
    [DataRow("OrderBy(x => x)")]
    [DataRow("OrderByDescending(x => x)")]
    [DataRow("Chunk(5)")]
    [DataRow("Reverse()")]
    [DataRow("Take(5)")]
    [DataRow("TakeLast(5)")]
    [DataRow("TakeWhile(x => true)")]
    public async Task LinqTraversalBeforeFilter(string traversal)
    {
        var original = $@"
using System.Linq;
using System.Collections.Generic;

var values = new [] {{ 32 }};
{{|#0:values.{traversal}|}}.Where(x => true);";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Unexpected collection traversal before Where() clause. Could the traversal be more efficient if filtering is performed first?"));
    }

    [TestMethod]
    [DataRow("Where(x => true)")]
    [DataRow("Concat(new List<int>())")]
    public async Task LinqTraversalBeforeFilter_NoTraversal(string traversal)
    {
        var original = $@"
using System.Linq;
using System.Collections.Generic;

var values = new [] {{ 32 }};
values.{traversal}.Where(x => true);";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task LinqTraversalBeforeFilter_EnumerableStaticMethod()
    {
        var original = $@"
using System.Linq;
using System.Collections.Generic;

var values = Enumerable.Empty<int>();";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task LinqTraversalBeforeFilter_NestedEnumerableChain()
    {
        var original = $@"
using System;
using System.Linq;
using System.Collections.Generic;

var values = new Test[] {{ new Test() }};
values.OrderBy(x => x.Values.Reverse().Where(x => true)).Where(x => true);

class Test
{{
    public int[] Values => Array.Empty<int>();
}}
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, new[] {
            VerifyCS.Diagnostic().WithNoLocation().WithMessage("Unexpected collection traversal before Where() clause. Could the traversal be more efficient if filtering is performed first?").WithSpan(7, 1, 7, 57),
            VerifyCS.Diagnostic().WithNoLocation().WithMessage("Unexpected collection traversal before Where() clause. Could the traversal be more efficient if filtering is performed first?").WithSpan(7, 21, 7, 39)
        });
    }

    [TestMethod]
    public async Task LinqTraversalBeforeFilter_QuerySyntax()
    {
        var original = $@"
using System.Linq;
using System.Collections.Generic;

var values = new [] {{ 32 }};
var result = from v in values
             orderby {{|#0:v descending|}}
             where v > 5
             select v;
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Unexpected collection traversal before Where() clause. Could the traversal be more efficient if filtering is performed first?"));
    }

    [TestMethod]
    public async Task LinqTraversalBeforeFilter_QuerySyntax_CorrectOrder()
    {
        var original = $@"
using System.Linq;
using System.Collections.Generic;

var values = new [] {{ 32 }};
var result = from v in values
             where v > 5
             orderby v descending
             select v;
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}