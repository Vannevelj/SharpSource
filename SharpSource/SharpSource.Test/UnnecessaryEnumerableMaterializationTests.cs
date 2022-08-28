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
    public class UnnecessaryEnumerableMaterializationTests : CSharpDiagnosticVerifier
    {
        protected override DiagnosticAnalyzer DiagnosticAnalyzer => new UnnecessaryEnumerableMaterializationAnalyzer();

        //protected override CodeFixProvider CodeFixProvider => new UnnecessaryEnumerableMaterializationCodeFix();

        private static IEnumerable<object[]> GetTestData()
        {
            var materializingOperations = new string[]
            {
"ToList()",
"ToArray()",
"ToLookup()",
"ToDictionary()",
"ToHashSet()"
            };

            var deferredExecutionOperations = new string[]
            {
"Select(x => x)",
"SelectMany(x => x)",
"Take(1)",
"Skip(1)",
"TakeWhile(x => x != null)",
"SkipWhile(x => x != null)",
"SkipLast()",
"Where(x => x != null)",
"GroupBy(x => x)",
"GroupJoin(Enumerable.Empty<string>(), x=> x, x => x, (x, y) => string.Empty)",
"OrderBy(x => x)",
"OrderByDescending(x => x)",
"Union(Enumerable.Empty<string>())",
"UnionBy(Enumerable.Empty<string>(), x => x)",
"Zip(Enumerable.Empty<string>())",
"Reverse()",
"Repeat(1)",
"Range()",
"Join(Enumerable.Empty<string>(), x => x, x => x, (x, y) => \"\")",
"OfType<string>()",
"Intersect(Enumerable.Empty<string>())",
"IntersectBy(Enumerable.Empty<string>(), x => x)",
"Except(Enumerable.Empty<string>())",
"ExceptBy(x => x)",
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
        [DynamicData(nameof(GetTestData), DynamicDataSourceType.Method)]
        public void UnnecessaryEnumerableMaterialization_Materialization_FollowByDeferredExecution(string first, string second)
        {
            var original = $@"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] {{ ""test"" }};
values.{first}.{second};
";

            var expected = $@"
using System.Linq;
using System.Collections.Generic;

IEnumerable<string> values = new [] {{ ""test"" }};
values.{second};
";

            VerifyDiagnostic(original, $"{first} is unnecessarily materializing the IEnumerable and can be omitted");
            //VerifyFix(original, expected);
        }
    }
}