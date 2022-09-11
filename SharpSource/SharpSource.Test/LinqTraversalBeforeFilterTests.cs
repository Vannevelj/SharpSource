using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class LinqTraversalBeforeFilterTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new LinqTraversalBeforeFilterAnalyzer();

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
values.{traversal}.Where(x => true);";

        await VerifyDiagnostic(original, "Unexpected collection traversal before Where() clause. Could the traversal be more efficient if filtering if performed first?");
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task LinqTraversalBeforeFilter_EnumerableStaticMethod()
    {
        var original = $@"
using System.Linq;
using System.Collections.Generic;

var values = Enumerable.Empty<int>();";

        await VerifyDiagnostic(original);
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

        await VerifyDiagnostic(original,
            "Unexpected collection traversal before Where() clause. Could the traversal be more efficient if filtering if performed first?",
            "Unexpected collection traversal before Where() clause. Could the traversal be more efficient if filtering if performed first?");
    }
}