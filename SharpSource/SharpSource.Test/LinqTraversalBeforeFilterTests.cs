using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Test.Helpers;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.LinqTraversalBeforeFilterAnalyzer, SharpSource.Diagnostics.LinqTraversalBeforeFilterCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class LinqTraversalBeforeFilterTests
{
    [TestMethod]
    [DataRow("OrderBy(x => x)")]
    [DataRow("OrderByDescending(x => x)")]
    [DataRow("Chunk(5)")]
    public async Task LinqTraversalBeforeFilter(string traversal)
    {
        var original = $@"
using System.Linq;
using System.Collections.Generic;

var values = new [] {{ 32 }};
{{|#0:values.{traversal}|}}.Where(x => true);";

        var result = $@"
using System.Linq;
using System.Collections.Generic;

var values = new [] {{ 32 }};
values.Where(x => true).{traversal};";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Unexpected collection traversal before Where() clause. Could the traversal be more efficient if filtering is performed first?"), result);
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
{{|#0:values.OrderBy(x => x.Values.Reverse().Where(x => true))|}}.Where(x => true);

class Test
{{
    public int[] Values => Array.Empty<int>();
}}
";

        var result = $@"
using System;
using System.Linq;
using System.Collections.Generic;

var values = new Test[] {{ new Test() }};
values.Where(x => true).OrderBy(x => x.Values.Reverse().Where(x => true));

class Test
{{
    public int[] Values => Array.Empty<int>();
}}
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Unexpected collection traversal before Where() clause. Could the traversal be more efficient if filtering is performed first?"), result);
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

    [TestMethod]
    public async Task LinqTraversalBeforeFilter_CorrectOrder()
    {
        var original = @"
using System.Linq;

var values = new [] { 32 };
values.Where(x => true).OrderBy(x => x);";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task LinqTraversalBeforeFilter_WithIntermediateCall()
    {
        var original = @"
using System.Linq;

var values = new [] { 32 };
values.OrderBy(x => x).Select(x => x * 2).Where(x => true);";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task LinqTraversalBeforeFilter_WithSubsequentCalls()
    {
        var original = @"
using System.Linq;

var values = new [] { 32 };
{|#0:values.OrderBy(x => x)|}.Where(x => true).ToList();";

        var result = @"
using System.Linq;

var values = new [] { 32 };
values.Where(x => true).OrderBy(x => x).ToList();";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Unexpected collection traversal before Where() clause. Could the traversal be more efficient if filtering is performed first?"), result);
    }

    [TestMethod]
    public async Task LinqTraversalBeforeFilter_AssignedToVariable()
    {
        var original = @"
using System.Linq;

var values = new [] { 32 };
var result = {|#0:values.OrderBy(x => x)|}.Where(x => true);";

        var result = @"
using System.Linq;

var values = new [] { 32 };
var result = values.Where(x => true).OrderBy(x => x);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Unexpected collection traversal before Where() clause. Could the traversal be more efficient if filtering is performed first?"), result);
    }

    [TestMethod]
    public async Task LinqTraversalBeforeFilter_AsMethodArgument()
    {
        var original = @"
using System.Linq;
using System.Collections.Generic;

var values = new [] { 32 };
Process({|#0:values.OrderBy(x => x)|}.Where(x => true));

void Process(IEnumerable<int> items) { }";

        var result = @"
using System.Linq;
using System.Collections.Generic;

var values = new [] { 32 };
Process(values.Where(x => true).OrderBy(x => x));

void Process(IEnumerable<int> items) { }";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Unexpected collection traversal before Where() clause. Could the traversal be more efficient if filtering is performed first?"), result);
    }

    [TestMethod]
    public async Task LinqTraversalBeforeFilter_OnListType()
    {
        var original = @"
using System.Linq;
using System.Collections.Generic;

var values = new List<int> { 32 };
{|#0:values.OrderBy(x => x)|}.Where(x => true);";

        var result = @"
using System.Linq;
using System.Collections.Generic;

var values = new List<int> { 32 };
values.Where(x => true).OrderBy(x => x);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Unexpected collection traversal before Where() clause. Could the traversal be more efficient if filtering is performed first?"), result);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/392")]
    [DataRow("Reverse()")]
    [DataRow("Take(5)")]
    [DataRow("TakeLast(5)")]
    [DataRow("TakeWhile(x => true)")]
    public async Task LinqTraversalBeforeFilter_NonCommutativeTraversal_NotFlagged(string traversal)
    {
        var original = $@"
using System.Linq;
using System.Collections.Generic;

var values = new [] {{ 32 }};
values.{traversal}.Where(x => true);";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}