using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;
using SharpSource.Test.Helpers.Helpers.CSharp;

namespace SharpSource.Test;

[TestClass]
public class ElementaryMethodsOfTypeInCollectionNotOverriddenTests : CSharpDiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new ElementaryMethodsOfTypeInCollectionNotOverriddenAnalyzer();

    [TestMethod]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_WithReferenceType()
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

    class MyCollectionItem {}
}";

        VerifyDiagnostic(original, "Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()");
    }

    [TestMethod]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_WithInterfaceType()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_WithValueType()
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

    struct MyCollectionItem {}
}";

        VerifyDiagnostic(original, "Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()");
    }

    [TestMethod]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_WithReferenceType_ImplementsEquals()
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
    }
}";

        VerifyDiagnostic(original, "Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()");
    }

    [TestMethod]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_WithValueType_ImplementsEquals()
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
    }
}";

        VerifyDiagnostic(original, "Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()");
    }

    [TestMethod]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_WithReferenceType_ImplementsGetHashCode()
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
        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }
    }
}";

        VerifyDiagnostic(original, "Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()");
    }

    [TestMethod]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_WithValueType_ImplementsGetHashCode()
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
        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }
    }
}";

        VerifyDiagnostic(original, "Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()");
    }

    [TestMethod]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_WithReferenceType_ImplementsMethods()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_WithValueType_ImplementsMethods()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_Dictionary_BothDoNotImplementMethods()
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
            var s = list.ContainsKey(new MyCollectionItem());
        }
    }

    class MyCollectionItem {}
}";

        VerifyDiagnostic(original,
            "Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()");
    }

    [TestMethod]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_Dictionary_OneDoesNotImplementMethods_UsedInCall()
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
            var s = list.ContainsKey(new MyCollectionItem());
        }
    }

    class MyCollectionItem {}
}";

        VerifyDiagnostic(original, "Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()");
    }

    [TestMethod]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_Dictionary_OneDoesNotImplementMethods_NotUsedInCall()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_TypeParameterWithoutObjectCreation()
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
            var s = list.Contains(default);
        }
    }

    class MyCollectionItem {}
}";

        VerifyDiagnostic(original, "Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()");
    }

    [TestMethod]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_GenericTypeFromClass()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_GenericTypeFromMethod()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_WithEnum()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_Object()
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

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_WithArray()
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

        VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/99")]
    [DataRow("Dictionary<int, int>")]
    [DataRow("KeyValuePair<int, int>")]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_WithSystemTypes(string type)
    {
        var original = $@"
using System.Collections.Generic;

var list = new List<{type}>();
var s = list.Contains(default);
";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    [DataRow("new Dictionary<MyCollectionItem, int>().ContainsKey(new MyCollectionItem())")]
    [DataRow("new Dictionary<int, MyCollectionItem>().ContainsValue(new MyCollectionItem())")]
    [DataRow("new Dictionary<MyCollectionItem, int>()[new MyCollectionItem()]")]
    [DataRow("new Dictionary<MyCollectionItem, int>().TryGetValue(new MyCollectionItem(), out _)")]
    [DataRow("new List<MyCollectionItem>().Contains(new MyCollectionItem())")]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_SupportedInvocations(string invocation)
    {
        var original = @$"
using System.Collections.Generic;
using System.Linq;

var x = {invocation};

class MyCollectionItem {{}}
";

        VerifyDiagnostic(original, "Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()");
    }

    [TestMethod]
    [DataRow("new Dictionary<MyCollectionItem, int>().ContainsKey(new MyCollectionItem())")]
    [DataRow("new List<MyCollectionItem>().Contains(new MyCollectionItem())")]
    [DataRow("new HashSet<MyCollectionItem>().Contains(new MyCollectionItem())")]
    [DataRow("new ReadOnlyCollection<MyCollectionItem>(new[] { new MyCollectionItem() }).Contains(new MyCollectionItem())")]
    [DataRow("new Queue<MyCollectionItem>().Contains(new MyCollectionItem())")]
    [DataRow("new Stack<MyCollectionItem>().Contains(new MyCollectionItem())")]
    public void ElementaryMethodsOfTypeInCollectionNotOverridden_SupportedTypes(string invocation)
    {
        var original = @$"
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

var x = {invocation};

class MyCollectionItem {{}}
";

        VerifyDiagnostic(original, "Type MyCollectionItem is used in a collection lookup but does not override Equals() and GetHashCode()");
    }
}