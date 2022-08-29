using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers.Helpers.CSharp;

namespace SharpSource.Test
{
    [TestClass]
    public class UnnecessaryEnumerableMaterializationTests : CSharpCodeFixVerifier
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
        public void UnnecessaryEnumerableMaterialization_Materialization_FollowByDeferredExecution(string materialization, string deferred)
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

            VerifyDiagnostic(original, $"{materialization} is unnecessarily materializing the IEnumerable and can be omitted");
            VerifyFix(original, expected);
        }

        [TestMethod]
        [DataRow("ToArray")]
        [DataRow("ToHashSet")]
        [DataRow("ToList")]
        public void UnnecessaryEnumerableMaterialization_MultipleMaterialization_FollowByDeferredExecution(string materialization)
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

            VerifyDiagnostic(original, $"{materialization} is unnecessarily materializing the IEnumerable and can be omitted");
            VerifyFix(original, expected);
        }

        [TestMethod]
        [DataRow("ToArray")]
        [DataRow("ToHashSet")]
        [DataRow("ToList")]
        public void UnnecessaryEnumerableMaterialization_DeferredBefore(string materialization)
        {
            var original = $@"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] {{ ""test"" }};
values.Where(x => true).{materialization}();
";

            VerifyDiagnostic(original);
        }

        [TestMethod]
        [DataRow("ToArray")]
        [DataRow("ToHashSet")]
        [DataRow("ToList")]
        public void UnnecessaryEnumerableMaterialization_ImmediateSingleMaterialization(string materialization)
        {
            var original = $@"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] {{ ""test"" }};
values.{materialization}();
";

            VerifyDiagnostic(original);
        }

        [TestMethod]
        [DataRow("ToArray")]
        [DataRow("ToHashSet")]
        [DataRow("ToList")]
        public void UnnecessaryEnumerableMaterialization_OtherEnumerableMethods(string materialization)
        {
            var original = $@"
using System.Linq;
using System.Collections.Generic;

var test = Enumerable.Range(0, 100).{materialization}();
";

            VerifyDiagnostic(original);
        }

        [TestMethod]
        public void UnnecessaryEnumerableMaterialization_MultipleDeferred_NoMaterialization()
        {
            var original = $@"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] {{ ""test"" }};
values.Where(x => true).Skip(1).Reverse();
";

            VerifyDiagnostic(original);
        }

        [TestMethod]
        [DataRow("ToArray")]
        [DataRow("ToHashSet")]
        [DataRow("ToList")]
        public void UnnecessaryEnumerableMaterialization_MultipleDeferred_Materialization(string materialization)
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

            VerifyDiagnostic(original, $"{materialization} is unnecessarily materializing the IEnumerable and can be omitted");
            VerifyFix(original, expected);
        }
    }
}