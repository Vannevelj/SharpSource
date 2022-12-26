using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class PointlessCollectionToStringTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new PointlessCollectionToStringAnalyzer();

    [TestMethod]
    [DataRow("List<int>")]
    [DataRow("LinkedList<int>")]
    [DataRow("HashSet<int>")]
    [DataRow("Dictionary<int, int>")]
    [DataRow("Queue<int>")]
    [DataRow("Stack<int>")]
    [DataRow("PriorityQueue<int, int>")]
    [DataRow("SortedDictionary<int, int>")]
    [DataRow("SortedList<int, int>")]
    [DataRow("SortedSet<int>")]
    public async Task PointlessCollectionToString(string collection)
    {
        var original = @$"
using System;
using System.Collections.Generic;

var collection = new {collection}();
Console.Write(collection.ToString());
";

        await VerifyDiagnostic(original, ".ToString() was called on a collection which results in impractical output");
    }

    [TestMethod]
    public async Task PointlessCollectionToString_OtherType()
    {
        var original = @$"
using System;

var collection = 5;
Console.Write(collection.ToString());
";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task PointlessCollectionToString_IEnumerable()
    {
        var original = @$"
using System;
using System.Collections.Generic;

IEnumerable<int> collection = new List<int>();
Console.Write(collection.ToString());
";

        await VerifyDiagnostic(original, ".ToString() was called on a collection which results in impractical output");
    }

    [TestMethod]
    public async Task PointlessCollectionToString_IEnumerableAsParam()
    {
        var original = @$"
using System;
using System.Collections.Generic;

void DoThing(IEnumerable<int> collection)
{{
    Console.Write(collection.ToString());
}}
";

        await VerifyDiagnostic(original, ".ToString() was called on a collection which results in impractical output");
    }

    [TestMethod]
    public async Task PointlessCollectionToString_NullConditional()
    {
        var original = @$"
using System;
using System.Collections.Generic;

var collection = new List<int>();
Console.Write(collection?.ToString());
";

        await VerifyDiagnostic(original, ".ToString() was called on a collection which results in impractical output");
    }

    [TestMethod]
    public async Task PointlessCollectionToString_NullSuppress()
    {
        var original = @$"
using System;
using System.Collections.Generic;

var collection = new List<int>();
Console.Write(collection!.ToString());
";

        await VerifyDiagnostic(original, ".ToString() was called on a collection which results in impractical output");
    }

    [TestMethod]
    public async Task PointlessCollectionToString_OtherMethod()
    {
        var original = @$"
using System;
using System.Collections.Generic;

var collection = new List<int>();
Console.Write(collection.GetHashCode());
";

        await VerifyDiagnostic(original);
    }
}