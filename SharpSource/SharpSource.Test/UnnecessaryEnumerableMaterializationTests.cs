using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class UnnecessaryEnumerableMaterializationTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new UnnecessaryEnumerableMaterializationAnalyzer();

    protected override CodeFixProvider CodeFixProvider => new UnnecessaryEnumerableMaterializationCodeFix();

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
values.{materialization}().{deferred};
";

        var expected = $@"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] {{ ""test"" }};
values.{deferred};
";

        await VerifyDiagnostic(original, $"{materialization} is unnecessarily materializing the IEnumerable and can be omitted");
        await VerifyFix(original, expected);
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
values.{materialization}().ToList();
";

        var expected = $@"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] {{ ""test"" }};
values.ToList();
";

        await VerifyDiagnostic(original, $"{materialization} is unnecessarily materializing the IEnumerable and can be omitted");
        await VerifyFix(original, expected);
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

        await VerifyDiagnostic(original);
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

        await VerifyDiagnostic(original);
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

        await VerifyDiagnostic(original);
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

        await VerifyDiagnostic(original);
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
values.Skip(1).Reverse().{materialization}().Take(1);
";

        var expected = $@"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] {{ ""test"" }};
values.Skip(1).Reverse().Take(1);
";

        await VerifyDiagnostic(original, $"{materialization} is unnecessarily materializing the IEnumerable and can be omitted");
        await VerifyFix(original, expected);
    }

    [TestMethod]
    public async Task UnnecessaryEnumerableMaterialization_ConditionalAccess()
    {
        var original = @"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] { ""test"" };
values?.ToArray().ToList();";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task UnnecessaryEnumerableMaterialization_ConditionalAccess_Chained()
    {
        var original = @"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] { ""test"" };
values?.ToArray().ToList().AsEnumerable();";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task UnnecessaryEnumerableMaterialization_SuppressingAccess()
    {
        var original = @"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] { ""test"" };
values!.ToArray().ToList();";

        var expected = @"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] { ""test"" };
values!.ToList();";

        await VerifyDiagnostic(original, $"ToArray is unnecessarily materializing the IEnumerable and can be omitted");
        await VerifyFix(original, expected);
    }
}