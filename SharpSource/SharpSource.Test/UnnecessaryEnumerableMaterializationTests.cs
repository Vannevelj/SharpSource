using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Test.Helpers;
using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.UnnecessaryEnumerableMaterializationAnalyzer, SharpSource.Diagnostics.UnnecessaryEnumerableMaterializationCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class UnnecessaryEnumerableMaterializationTests
{
    private static IEnumerable<object[]> GetSingleValueData()
    {
        var materializingOperations = new string[]
        {
"ToList",
"ToArray",
"ToHashSet"
        };

        var deferredExecutionOperations = new string[]
        {
"Select(x => x)",
"SelectMany(x => x)",
"Take(1)",
"Skip(1)",
"TakeWhile(x => true)",
"SkipWhile(x => true)",
"SkipLast(1)",
"Where(x => true)",
"GroupBy(x => x)",
"GroupJoin(Enumerable.Empty<string>(), x=> x, x => x, (x, y) => string.Empty)",
"OrderBy(x => x)",
"OrderByDescending(x => x)",
"Union(Enumerable.Empty<string>())",
"UnionBy(Enumerable.Empty<string>(), x => x)",
"Zip(Enumerable.Empty<string>())",
"Reverse()",
"Join(Enumerable.Empty<string>(), x => x, x => x, (x, y) => \"\")",
"OfType<string>()",
"Intersect(Enumerable.Empty<string>())",
"IntersectBy(Enumerable.Empty<string>(), x => x)",
"Except(Enumerable.Empty<string>())",
"ExceptBy(Enumerable.Empty<string>(), x => x)",
"Distinct()",
"DistinctBy(x => x)",
"DefaultIfEmpty()",
"Concat(Enumerable.Empty<string>())",
"Cast<object>()",
        };

        return from materialization in materializingOperations
               from deferred in deferredExecutionOperations
               select new object[] { materialization, deferred };
    }

    [TestMethod]
    [DynamicData(nameof(GetSingleValueData), DynamicDataSourceType.Method)]
    public async Task UnnecessaryEnumerableMaterialization_Materialization_FollowByDeferredExecutionAsync(string materialization, string deferred)
    {
        var original = $@"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] {{ ""test"" }};
{{|#0:values.{materialization}().{deferred}|}};
";

        var expected = $@"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] {{ ""test"" }};
values.{deferred};
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage($"{materialization} is unnecessarily materializing the IEnumerable and can be omitted"), expected);
    }

    [TestMethod]
    [DataRow("ToArray")]
    [DataRow("ToHashSet")]
    [DataRow("ToList")]
    public async Task UnnecessaryEnumerableMaterialization_MultipleMaterialization_FollowByDeferredExecutionAsync(string materialization)
    {
        var original = $@"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] {{ ""test"" }};
{{|#0:values.{materialization}().ToList()|}};
";

        var expected = $@"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] {{ ""test"" }};
values.ToList();
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage($"{materialization} is unnecessarily materializing the IEnumerable and can be omitted"), expected);
    }

    [TestMethod]
    [DataRow("ToArray")]
    [DataRow("ToHashSet")]
    [DataRow("ToList")]
    public async Task UnnecessaryEnumerableMaterialization_DeferredBeforeAsync(string materialization)
    {
        var original = $@"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] {{ ""test"" }};
values.Where(x => true).{materialization}();
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    [DataRow("ToArray")]
    [DataRow("ToHashSet")]
    [DataRow("ToList")]
    public async Task UnnecessaryEnumerableMaterialization_ImmediateSingleMaterializationAsync(string materialization)
    {
        var original = $@"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] {{ ""test"" }};
values.{materialization}();
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    [DataRow("ToArray")]
    [DataRow("ToHashSet")]
    [DataRow("ToList")]
    public async Task UnnecessaryEnumerableMaterialization_OtherEnumerableMethodsAsync(string materialization)
    {
        var original = $@"
using System.Linq;
using System.Collections.Generic;

var test = Enumerable.Range(0, 100).{materialization}();
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task UnnecessaryEnumerableMaterialization_MultipleDeferred_NoMaterialization()
    {
        var original = $@"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] {{ ""test"" }};
values.Where(x => true).Skip(1).Reverse();
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    [DataRow("ToArray")]
    [DataRow("ToHashSet")]
    [DataRow("ToList")]
    public async Task UnnecessaryEnumerableMaterialization_MultipleDeferred_MaterializationAsync(string materialization)
    {
        var original = $@"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] {{ ""test"" }};
{{|#0:values.Skip(1).Reverse().{materialization}().Take(1)|}};
";

        var expected = $@"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] {{ ""test"" }};
values.Skip(1).Reverse().Take(1);
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage($"{materialization} is unnecessarily materializing the IEnumerable and can be omitted"), expected);
    }

    [TestMethod]
    public async Task UnnecessaryEnumerableMaterialization_ConditionalAccess()
    {
        var original = @"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] { ""test"" };
values?{|#0:.ToArray().ToList()|};";

        var expected = $@"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] {{ ""test"" }};
values?.ToList();";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("ToArray is unnecessarily materializing the IEnumerable and can be omitted"), expected);
    }

    [TestMethod]
    [Ignore("Need to find a way to handle the testing of a code fix when there are multiple issues, see https://github.com/Vannevelj/SharpSource/issues/288")]
    public async Task UnnecessaryEnumerableMaterialization_ConditionalAccess_Chained()
    {
        var original = @"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] { ""test"" };
values?.ToArray().ToList().AsEnumerable();";

        var expected = $@"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] {{ ""test"" }};
values?.ToList().AsEnumerable();";

        await VerifyCS.VerifyCodeFix(original, new[] {
            VerifyCS.Diagnostic().WithNoLocation().WithMessage("ToArray is unnecessarily materializing the IEnumerable and can be omitted").WithSpan(6, 8, 6, 27),
            VerifyCS.Diagnostic().WithNoLocation().WithMessage("ToList is unnecessarily materializing the IEnumerable and can be omitted").WithSpan(6, 8, 6, 42)
        }, expected);
    }

    [TestMethod]
    public async Task UnnecessaryEnumerableMaterialization_SuppressingAccess()
    {
        var original = @"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] { ""test"" };
{|#0:values!.ToArray().ToList()|};";

        var expected = @"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] { ""test"" };
values!.ToList();";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("ToArray is unnecessarily materializing the IEnumerable and can be omitted"), expected);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/310")]
    public async Task UnnecessaryEnumerableMaterialization_ExpressionAcceptsBaseType()
    {
        var original = @"
using System.Linq;
using System.Collections.Generic;

var files = new[] { """" };
Method(files.OfType<string>().ToList());

void Method(IList<string> list)
{

}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/310")]
    public async Task UnnecessaryEnumerableMaterialization_ToListForEach()
    {
        var original = @"
using System.Linq;
using System.Collections.Generic;

var files = new[] { """" };
files.OfType<string>().ToList().ForEach(x => System.Console.Write(x));";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}