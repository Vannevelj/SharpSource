using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Test.Helpers;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.ElementaryMethodsOfTypeInCollectionNotOverriddenAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class ElementaryMethodsOfTypeInCollectionNotOverriddenTests
{
    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_WithReferenceType()
    {
        var original = @"
using System.Collections.Generic;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var list = new List<MyCollectionItem>();
            var s = list.Contains({|#0:default|});
        }
    }

    class MyCollectionItem {}
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()"));
    }

    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_WithInterfaceType()
    {
        var original = @"
using System.Collections.Generic;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var list = new List<MyCollectionItem>();
            var s = list.Contains(default);
        }
    }

    interface MyCollectionItem {}
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_WithValueType()
    {
        var original = @"
using System.Collections.Generic;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var list = new List<MyCollectionItem>();
            var s = list.Contains({|#0:default|});
        }
    }

    struct MyCollectionItem {}
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_WithReferenceType_ImplementsEquals()
    {
        var original = @"
using System.Collections.Generic;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var list = new List<MyCollectionItem>();
            var s = list.Contains({|#0:default|});
        }
    }

    class MyCollectionItem
    {
        public override bool Equals(object obj)
        {
            throw new System.NotImplementedException();
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()"));
    }

    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_WithValueType_ImplementsEquals()
    {
        var original = @"
using System.Collections.Generic;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var list = new List<MyCollectionItem>();
            var s = list.Contains({|#0:default|});
        }
    }

    struct MyCollectionItem
    {
        public override bool Equals(object obj)
        {
            throw new System.NotImplementedException();
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_WithReferenceType_ImplementsGetHashCode()
    {
        var original = @"
using System.Collections.Generic;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var list = new List<MyCollectionItem>();
            var s = list.Contains({|#0:default|});
        }
    }

    class MyCollectionItem
    {
        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()"));
    }

    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_WithValueType_ImplementsGetHashCode()
    {
        var original = @"
using System.Collections.Generic;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var list = new List<MyCollectionItem>();
            var s = list.Contains({|#0:default|});
        }
    }

    struct MyCollectionItem
    {
        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_WithReferenceType_ImplementsMethods()
    {
        var original = @"
using System.Collections.Generic;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var list = new List<MyCollectionItem>();
            var s = list.Contains(default);
        }
    }

    class MyCollectionItem
    {
        public override bool Equals(object obj)
        {
            throw new System.NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_WithValueType_ImplementsMethods()
    {
        var original = @"
using System.Collections.Generic;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var list = new List<MyCollectionItem>();
            var s = list.Contains(default);
        }
    }

    struct MyCollectionItem
    {
        public override bool Equals(object obj)
        {
            throw new System.NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_Dictionary_BothDoNotImplementMethods()
    {
        var original = @"
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var list = new Dictionary<MyCollectionItem, MyCollectionItem>();
            var s = list.ContainsKey({|#0:new MyCollectionItem()|});
        }
    }

    class MyCollectionItem {}
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()"));
    }

    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_Dictionary_OneDoesNotImplementMethods_UsedInCall()
    {
        var original = @"
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var list = new Dictionary<MyCollectionItem, int>();
            var s = list.ContainsKey({|#0:new MyCollectionItem()|});
        }
    }

    class MyCollectionItem {}
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()"));
    }

    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_Dictionary_OneDoesNotImplementMethods_NotUsedInCall()
    {
        var original = @"
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var list = new Dictionary<int, MyCollectionItem>();
            var s = list.ContainsKey(5);
        }
    }

    class MyCollectionItem {}
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_TypeParameterWithoutObjectCreation()
    {
        var original = @"
using System.Linq;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var list = Enumerable.Empty<MyCollectionItem>();
            var s = list.Contains({|#0:default|});
        }
    }

    class MyCollectionItem {}
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()"));
    }

    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_GenericTypeFromClass()
    {
        var original = @"
using System.Collections.Generic;
namespace ConsoleApplication1
{
    public class MyClass<T>
    {
        public static List<T> Method()
        {
            var newList = new List<T>();
            var s = newList.Contains(default);
            return newList;
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_GenericTypeFromMethod()
    {
        var original = @"
using System.Collections.Generic;
namespace ConsoleApplication1
{
    public class MyClass
    {
        public static List<T1> Method<T1>()
        {
            var newList = new List<T1>();
            var s = newList.Contains(default);
            return newList;
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_WithEnum()
    {
        var original = @"
using System.Collections.Generic;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var list = new List<SomeEnum>();
            var s = list.Contains(default);
        }
    }

    enum SomeEnum {}
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_Object()
    {
        var original = @"
using System.Collections.Generic;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var list = new List<object>();
            var s = list.Contains(default);
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_WithArray()
    {
        var original = @"
using System.Collections.Generic;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var list = new List<SomeClass[]>();
            var s = list.Contains(default);
        }
    }

    class SomeClass {}
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/99")]
    [DataRow("KeyValuePair<int, int>")]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_WithSystemTypes(string type)
    {
        var original = $@"
using System.Collections.Generic;

var list = new List<{type}>();
var s = list.Contains(default);";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    [DataRow("new Dictionary<MyCollectionItem, int>().ContainsKey({|#0:new MyCollectionItem()|})")]
    [DataRow("new Dictionary<int, MyCollectionItem>().ContainsValue({|#0:new MyCollectionItem()|})")]
    [DataRow("new Dictionary<MyCollectionItem, int>()[{|#0:new MyCollectionItem()|}]")]
    [DataRow("new Dictionary<MyCollectionItem, int>().TryGetValue({|#0:new MyCollectionItem()|}, out _)")]
    [DataRow("new List<MyCollectionItem>().Contains({|#0:new MyCollectionItem()|})")]
    [DataRow("new HashSet<MyCollectionItem>().Add({|#0:new MyCollectionItem()|})")]
    [DataRow("new Dictionary<MyCollectionItem, int>(); x.Add({|#0:new MyCollectionItem()|}, 5)")]
    [DataRow("{|#0:new List<MyCollectionItem>().Distinct()|}")]
    [DataRow("{|#0:new List<MyCollectionItem>().ToHashSet()|}")]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_SupportedInvocations(string invocation)
    {
        var original = @$"
using System.Collections.Generic;
using System.Linq;

var x = {invocation};

class MyCollectionItem {{}}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()"));
    }

    [TestMethod]
    [DataRow("new Dictionary<MyCollectionItem, int>().ContainsKey({|#0:new MyCollectionItem()|})")]
    [DataRow("new List<MyCollectionItem>().Contains({|#0:new MyCollectionItem()|})")]
    [DataRow("new HashSet<MyCollectionItem>().Contains({|#0:new MyCollectionItem()|})")]
    [DataRow("new ReadOnlyCollection<MyCollectionItem>(new[] { new MyCollectionItem() }).Contains({|#0:new MyCollectionItem()|})")]
    [DataRow("new Queue<MyCollectionItem>().Contains({|#0:new MyCollectionItem()|})")]
    [DataRow("new Stack<MyCollectionItem>().Contains({|#0:new MyCollectionItem()|})")]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_SupportedTypes(string invocation)
    {
        var original = @$"
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

var x = {invocation};

class MyCollectionItem {{}}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()"));
    }

    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_WithOptionalAccess()
    {
        var original = @"
using System.Collections.Generic;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var list = new List<MyCollectionItem>();
            var s = list?.Contains({|#0:default|});
        }
    }

    class MyCollectionItem {}
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()"));
    }

    [TestMethod]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_WithSuppressAccess()
    {
        var original = @"
using System.Collections.Generic;
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var list = new List<MyCollectionItem>();
            var s = list!.Contains({|#0:default|});
        }
    }

    class MyCollectionItem {}
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()"));
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/198")]
    [DataRow("int")]
    [DataRow("uint")]
    [DataRow("short")]
    [DataRow("long")]
    [DataRow("ulong")]
    [DataRow("ushort")]
    [DataRow("float")]
    [DataRow("double")]
    [DataRow("Guid")]
    [DataRow("string")]
    [DataRow("nint")]
    [DataRow("nuint")]
    [DataRow("char")]
    [DataRow("bool")]
    [DataRow("decimal")]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_BasicTypes(string type)
    {
        var original = @$"
using System;
using System.Collections.Generic;
using System.Linq;

var x = new HashSet<{type}>();
if (x.Contains(default)) {{ }}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/243")]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_WithExternType()
    {
        var original = @"
using System.Runtime.InteropServices;

static class GlobalAssemblyCacheLocation
{
    [DllImport(""clr"", PreserveSig = true)]
    private static extern unsafe void DoThing(string id, byte* path);

    private static unsafe void Thing() => DoThing(""id"", null);
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/299")]
    public async Task ElementaryMethodsOfTypeInCollectionNotOverridden_WithInheritance()
    {
        var original = @"
using System.Collections.Generic;
var list = new List<MyCollectionItem>();
var s = list.Contains(default);

class MyCollectionItem : BaseClass { }
class BaseClass
{
    public override bool Equals(object obj) => true;
    public override int GetHashCode() => 5;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}