using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.CollectionManipulatedDuringTraversal, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class CollectionManipulatedDuringTraversalTests
{
    [TestMethod]
    [DataRow("Add(item)")]
    [DataRow("Remove(item)")]
    [DataRow("Insert(0, item)")]
    [DataRow("AddRange(new[] { item })")]
    [DataRow("Clear()")]
    [DataRow("InsertRange(0, new[] { item })")]
    [DataRow("RemoveAll(x => true)")]
    [DataRow("RemoveAt(0)")]
    [DataRow("RemoveRange(0, 1)")]
    [DataRow("Reverse()")]
    [DataRow("Sort()")]
    public async Task CollectionManipulatedDuringTraversal_List(string invocation)
    {
        var original = $@"
using System.Collections.Generic;

void Method(List<int> items)
{{
    foreach (var item in items)
    {{
        {{|#0:items.{invocation}|}};
    }}
}}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Attempted to manipulate a collection while traversing. Ensure modifications don't affect the original collection."));
    }

    [TestMethod]
    public async Task CollectionManipulatedDuringTraversal_ReAssign()
    {
        var original = @"
using System.Collections.Generic;
using System.Linq;

void Method(IEnumerable<int> items)
{
    foreach (var item in items)
    {
        items = items.Append(5);
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task CollectionManipulatedDuringTraversal_NonManipulation()
    {
        var original = @"
using System.Collections.Generic;

void Method(List<int> items)
{
    foreach (var item in items)
    {
        items.LastIndexOf(item);
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task CollectionManipulatedDuringTraversal_OnCopy()
    {
        var original = @"
using System.Collections.Generic;

void Method(List<int> items)
{
    foreach (var item in items.ToArray())
    {
        items.LastIndexOf(item);
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task CollectionManipulatedDuringTraversal_ModifiesOtherReference_ForEach()
    {
        var original = @"
using System.Collections.Generic;

void Method(List<int> items)
{
    var copy = items.ToArray();
    foreach (var item in copy)
    {
        items.Add(item);
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task CollectionManipulatedDuringTraversal_ModifiesProperty()
    {
        var original = @"
using System.Collections.Generic;

class Test
{
    public List<int> Items { get; set; }

    void Method()
    {
        foreach (var item in Items)
        {
            {|#0:Items.Add(item)|};
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Attempted to manipulate a collection while traversing. Ensure modifications don't affect the original collection."));
    }

    [TestMethod]
    public async Task CollectionManipulatedDuringTraversal_ModifiesOtherReference_For()
    {
        var original = @"
using System.Collections.Generic;

void Method(List<int> items)
{
    var copy = items.ToArray();
    for (var i = 0; i < copy.Length; i++)
    {
        items.Add(items[i]);
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task CollectionManipulatedDuringTraversal_ReferencesComplex()
    {
        var original = @"
using System.Collections.Generic;
using System.Linq;

void Method(List<int> items)
{
    var copy = items.ToArray();
    foreach (var item in copy.Concat(items))
    {
        items.Add(item);
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task CollectionManipulatedDuringTraversal_ReferencesComplex_ToNewCollection()
    {
        var original = @"
using System.Collections.Generic;
using System.Linq;

void Method(List<int> items)
{
    foreach (var item in items.ToArray())
    {
        items.Add(item);
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task CollectionManipulatedDuringTraversal_ReferencesComplex_SamePropertyOnDifferentType()
    {
        var original = @"
using System.Collections.Generic;
using System.Linq;

void Method(Test a, Test b)
{
    foreach (var item in a.Items)
    {
        b.Items.Add(item);
    }
}

class Test
{
    public List<int> Items { get; set; }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    [DataRow("Add(0, 5)")]
    [DataRow("Clear()")]
    [DataRow("Remove(32)")]
    [DataRow("TryAdd(1, 2)")]
    public async Task CollectionManipulatedDuringTraversal_Dictionary(string invocation)
    {
        var original = $@"
using System.Collections.Generic;

void Method(Dictionary<int, int> items)
{{
    foreach (var item in items)
    {{
        {{|#0:items.{invocation}|}};
    }}
}}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Attempted to manipulate a collection while traversing. Ensure modifications don't affect the original collection."));
    }

    [TestMethod]
    [DataRow("Add(0)")]
    [DataRow("Clear()")]
    [DataRow("ExceptWith(new int[] { })")]
    [DataRow("IntersectWith(new int[] { })")]
    [DataRow("Remove(0)")]
    [DataRow("RemoveWhere(x => true)")]
    [DataRow("SymmetricExceptWith(new int[] { })")]
    [DataRow("UnionWith(new int[] { })")]
    public async Task CollectionManipulatedDuringTraversal_HashSet(string invocation)
    {
        var original = $@"
using System.Collections.Generic;

void Method(HashSet<int> items)
{{
    foreach (var item in items)
    {{
        {{|#0:items.{invocation}|}};
    }}
}}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Attempted to manipulate a collection while traversing. Ensure modifications don't affect the original collection."));
    }

    [TestMethod]
    [DataRow("Clear()")]
    [DataRow("Pop()")]
    [DataRow("Push(1)")]
    [DataRow("TryPop(out var result)")]
    public async Task CollectionManipulatedDuringTraversal_Stack(string invocation)
    {
        var original = $@"
using System.Collections.Generic;

void Method(Stack<int> items)
{{
    foreach (var item in items)
    {{
        {{|#0:items.{invocation}|}};
    }}
}}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Attempted to manipulate a collection while traversing. Ensure modifications don't affect the original collection."));
    }

    [TestMethod]
    [DataRow("Clear()")]
    [DataRow("Dequeue()")]
    [DataRow("Enqueue(1)")]
    [DataRow("TryDequeue(out var result)")]
    public async Task CollectionManipulatedDuringTraversal_Queue(string invocation)
    {
        var original = $@"
using System.Collections.Generic;

void Method(Queue<int> items)
{{
    foreach (var item in items)
    {{
        {{|#0:items.{invocation}|}};
    }}
}}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Attempted to manipulate a collection while traversing. Ensure modifications don't affect the original collection."));
    }

    [TestMethod]
    [DataRow("Add(0, 5)")]
    [DataRow("Clear()")]
    [DataRow("Remove(32)")]
    public async Task CollectionManipulatedDuringTraversal_SortedDictionary(string invocation)
    {
        var original = $@"
using System.Collections.Generic;

void Method(SortedDictionary<int, int> items)
{{
    foreach (var item in items)
    {{
        {{|#0:items.{invocation}|}};
    }}
}}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Attempted to manipulate a collection while traversing. Ensure modifications don't affect the original collection."));
    }

    [TestMethod]
    [DataRow("Add(0, 2)")]
    [DataRow("Remove(0)")]
    [DataRow("RemoveAt(0)")]
    //[DataRow("SetValueAtIndex(0, 32)")] // .NET 7
    public async Task CollectionManipulatedDuringTraversal_SortedList(string invocation)
    {
        var original = $@"
using System.Collections.Generic;

void Method(SortedList<int, int> items)
{{
    foreach (var item in items)
    {{
        {{|#0:items.{invocation}|}};
    }}
}}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Attempted to manipulate a collection while traversing. Ensure modifications don't affect the original collection."));
    }

    [TestMethod]
    [DataRow("Add(0)")]
    [DataRow("Clear()")]
    [DataRow("ExceptWith(new int[] { })")]
    [DataRow("IntersectWith(new int[] { })")]
    [DataRow("Remove(0)")]
    [DataRow("RemoveWhere(x => true)")]
    [DataRow("Reverse()")]
    [DataRow("SymmetricExceptWith(new int[] { })")]
    [DataRow("UnionWith(new int[] { })")]
    public async Task CollectionManipulatedDuringTraversal_SortedSet(string invocation)
    {
        var original = $@"
using System.Collections.Generic;

void Method(SortedSet<int> items)
{{
    foreach (var item in items)
    {{
        {{|#0:items.{invocation}|}};
    }}
}}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Attempted to manipulate a collection while traversing. Ensure modifications don't affect the original collection."));
    }

    [TestMethod]
    [DataRow("Add(0)")]
    [DataRow("Clear()")]
    [DataRow("Remove(0)")]
    public async Task CollectionManipulatedDuringTraversal_ICollection(string invocation)
    {
        var original = $@"
using System.Collections.Generic;

void Method(ICollection<int> items)
{{
    foreach (var item in items)
    {{
        {{|#0:items.{invocation}|}};
    }}
}}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Attempted to manipulate a collection while traversing. Ensure modifications don't affect the original collection."));
    }

    [TestMethod]
    [DataRow("Insert(0, 2)")]
    [DataRow("RemoveAt(0)")]
    public async Task CollectionManipulatedDuringTraversal_IList(string invocation)
    {
        var original = $@"
using System.Collections.Generic;

void Method(IList<int> items)
{{
    foreach (var item in items)
    {{
        {{|#0:items.{invocation}|}};
    }}
}}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Attempted to manipulate a collection while traversing. Ensure modifications don't affect the original collection."));
    }

    [TestMethod]
    [DataRow("Add(0)")]
    [DataRow("ExceptWith(new int[] { })")]
    [DataRow("IntersectWith(new int[] { })")]
    [DataRow("SymmetricExceptWith(new int[] { })")]
    [DataRow("UnionWith(new int[] { })")]
    public async Task CollectionManipulatedDuringTraversal_ISet(string invocation)
    {
        var original = $@"
using System.Collections.Generic;

void Method(ISet<int> items)
{{
    foreach (var item in items)
    {{
        {{|#0:items.{invocation}|}};
    }}
}}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Attempted to manipulate a collection while traversing. Ensure modifications don't affect the original collection."));
    }

    [TestMethod]
    [DataRow("Add(0, 5)")]
    [DataRow("Remove(32)")]
    public async Task CollectionManipulatedDuringTraversal_IDictionary(string invocation)
    {
        var original = $@"
using System.Collections.Generic;

void Method(IDictionary<int, int> items)
{{
    foreach (var item in items)
    {{
        {{|#0:items.{invocation}|}};
    }}
}}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Attempted to manipulate a collection while traversing. Ensure modifications don't affect the original collection."));
    }

    [TestMethod]
    public async Task CollectionManipulatedDuringTraversal_Array()
    {
        var original = @"
using System.Collections.Generic;

void Method(int[] items)
{
    foreach (var item in items)
    {
        items.SetValue(null, 1);
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task CollectionManipulatedDuringTraversal_For()
    {
        var original = @"
using System.Collections.Generic;

void Method(List<int> items)
{
    for (var i = 0; i < items.Count; i++)
    {
        {|#0:items.Add(0)|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Attempted to manipulate a collection while traversing. Ensure modifications don't affect the original collection."));
    }

    [TestMethod]
    public async Task CollectionManipulatedDuringTraversal_Indexer()
    {
        var original = @"
using System.Collections.Generic;

void Method(List<int> items)
{
    for (var i = 0; i < items.Count; i++)
    {
        items[i] = 32;
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task CollectionManipulatedDuringTraversal_While()
    {
        var original = @"
using System.Collections.Generic;

void Method(List<int> items)
{
    while (true)
    {
        items.Add(0);
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task CollectionManipulatedDuringTraversal_Lambda()
    {
        var original = @"
using System;
using System.Collections.Generic;

void Method(List<int> items)
{
    Action thing;
    foreach (var item in items)
    {
        thing = () => items.Add(item);
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task CollectionManipulatedDuringTraversal_WithSubsequentReturn()
    {
        var original = @"
using System.Collections.Generic;

void Method(List<int> items)
{
    foreach (var item in items)
    {
        items.Add(1);
        return;
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task CollectionManipulatedDuringTraversal_WithSubsequentBreak()
    {
        var original = @"
using System.Collections.Generic;

void Method(List<int> items)
{
    foreach (var item in items)
    {
        items.Add(1);
        break;
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}