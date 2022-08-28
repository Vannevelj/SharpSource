using System;
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

        private static IEnumerable<object[]> GetSingleValueData()
        {
            var materializingOperations = new string[]
            {
"ToList()",
"ToArray()",
"ToHashSet()"
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
        public void UnnecessaryEnumerableMaterialization_Materialization_FollowByDeferredExecution_SingleValueCollections(string first, string second)
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

            VerifyDiagnostic(original, $"{string.Join("",first.TakeWhile(c => c != '('))} is unnecessarily materializing the IEnumerable and can be omitted");
            //VerifyFix(original, expected);
        }

        private static IEnumerable<object[]> GetDictionaryData()
        {
            var materializingOperations = new string[]
            {
"ToDictionary(x => x)"
            };

            var deferredExecutionOperations = new string[]
            {
"Select(x => x)",
"SelectMany(x => x.Value)",
"Take(1)",
"Skip(1)",
"TakeWhile(x => true)",
"SkipWhile(x => true)",
"SkipLast(1)",
"Where(x => true)",
"GroupBy(x => x)",
"GroupJoin(Enumerable.Empty<KeyValuePair<string, string>>(), x => x, x => x, (x, y) => new KeyValuePair<string, string>())",
"OrderBy(x => x)",
"OrderByDescending(x => x)",
"Union(Enumerable.Empty<KeyValuePair<string, string>>())",
"UnionBy(Enumerable.Empty<KeyValuePair<string, string>>(), x => x)",
"Zip(Enumerable.Empty<KeyValuePair<string, string>>())",
"Reverse()",
"Join(Enumerable.Empty<KeyValuePair<string, string>>(), x => x, x => x, (x, y) => \"\")",
"OfType<string>()",
"Intersect(Enumerable.Empty<KeyValuePair<string, string>>())",
"IntersectBy(Enumerable.Empty<KeyValuePair<string, string>>(), x => x)",
"Except(Enumerable.Empty<KeyValuePair<string, string>>())",
"ExceptBy(Enumerable.Empty<KeyValuePair<string, string>>(), x => x)",
"Distinct()",
"DistinctBy(x => x)",
"DefaultIfEmpty()",
"Concat(Enumerable.Empty<KeyValuePair<string, string>>())",
"Cast<object>()",
            };

            return from materialization in materializingOperations
                   from deferred in deferredExecutionOperations
                   select new object[] { materialization, deferred };
        }

        [TestMethod]
        [DynamicData(nameof(GetDictionaryData), DynamicDataSourceType.Method)]
        public void UnnecessaryEnumerableMaterialization_Materialization_FollowByDeferredExecution_Dictionary(string first, string second)
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

            VerifyDiagnostic(original, "ToDictionary is unnecessarily materializing the IEnumerable and can be omitted");
            //VerifyFix(original, expected);
        }

        private static IEnumerable<object[]> GetLookupData()
        {
            var materializingOperations = new string[]
            {
"ToLookup(x => x)"
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
"GroupJoin(Enumerable.Empty<IGrouping<string, string>>(), x => x, x => x, (x, y) => new KeyValuePair<string, string>())",
"OrderBy(x => x)",
"OrderByDescending(x => x)",
"Union(Enumerable.Empty<IGrouping<string, string>>())",
"UnionBy(Enumerable.Empty<IGrouping<string, string>>(), x => x)",
"Zip(Enumerable.Empty<IGrouping<string, string>>())",
"Reverse()",
"Join(Enumerable.Empty<IGrouping<string, string>>(), x => x, x => x, (x, y) => \"\")",
"OfType<string>()",
"Intersect(Enumerable.Empty<IGrouping<string, string>>())",
"IntersectBy(Enumerable.Empty<IGrouping<string, string>>(), x => x)",
"Except(Enumerable.Empty<IGrouping<string, string>>())",
"ExceptBy(Enumerable.Empty<IGrouping<string, string>>(), x => x)",
"Distinct()",
"DistinctBy(x => x)",
"DefaultIfEmpty()",
"Concat(Enumerable.Empty<IGrouping<string, string>>())",
"Cast<object>()",
            };

            return from materialization in materializingOperations
                   from deferred in deferredExecutionOperations
                   select new object[] { materialization, deferred };
        }

        [TestMethod]
        [DynamicData(nameof(GetLookupData), DynamicDataSourceType.Method)]
        public void UnnecessaryEnumerableMaterialization_Materialization_FollowByDeferredExecution_Lookup(string first, string second)
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

            VerifyDiagnostic(original, "ToLookup is unnecessarily materializing the IEnumerable and can be omitted");
            //VerifyFix(original, expected);
        }
    }
}