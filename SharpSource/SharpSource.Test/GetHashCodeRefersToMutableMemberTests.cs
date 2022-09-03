using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;
using SharpSource.Test.Helpers.Helpers.CSharp;

namespace SharpSource.Test;

[TestClass]
public class GetHashCodeRefersToMutableMemberTests : CSharpDiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new GetHashCodeRefersToMutableMemberAnalyzer();

    [TestMethod]
    public void GetHashCodeRefersToMutableMember_ConstantField()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class Foo
    {
        private const char Boo = '1';

        public override int GetHashCode()
        {
            return Boo.GetHashCode();
        }
    }
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void GetHashCodeRefersToMutableMember_NonReadonlyField()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class Foo
    {
        private char _boo = '1';

        public override int GetHashCode()
        {
            return _boo.GetHashCode();
        }
    }
}";

        VerifyDiagnostic(original, "GetHashCode() refers to mutable field _boo");
    }

    [TestMethod]
    public void GetHashCodeRefersToMutableMember_StaticField()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class Foo
    {
        private static readonly char _boo = '1';

        public override int GetHashCode()
        {
            return _boo.GetHashCode();
        }
    }
}";

        VerifyDiagnostic(original, "GetHashCode() refers to mutable field _boo");
    }

    [TestMethod]
    public void GetHashCodeRefersToMutableMember_NonReadonlyStaticNonValueTypeField()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    public class Foo
    {
        private static Type _boo = typeof(Foo);

        public override int GetHashCode()
        {
            return _boo.GetHashCode();
        }
    }
}";

        VerifyDiagnostic(original, "GetHashCode() refers to mutable field _boo");
    }

    [TestMethod]
    public void GetHashCodeRefersToMutableMember_NonValueTypeNonStringField()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    public class Foo
    {
        private readonly Type _boo = typeof(Foo);

        public override int GetHashCode()
        {
            return _boo.GetHashCode();
        }
    }
}";

        VerifyDiagnostic(original, "GetHashCode() refers to mutable field _boo");
    }

    [TestMethod]
    public void GetHashCodeRefersToMutableMember_ImmutableMember_NoWarning()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    public class Foo
    {
        private readonly char _boo = '1';

        public override int GetHashCode()
        {
            return _boo.GetHashCode();
        }
    }
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void GetHashCodeRefersToMutableMember_ImmutableStringMember_NoWarning()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    public class Foo
    {
        private readonly string _boo = ""1"";

        public override int GetHashCode()
        {
            return _boo.GetHashCode();
        }
    }
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void GetHashCodeRefersToMutableMember_StaticReadonlyProperty()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class Foo
    {
        public static char Boo { get; } = '1';

        public override int GetHashCode()
        {
            return Boo.GetHashCode();
        }
    }
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void GetHashCodeRefersToMutableMember_NonValueTypeNonStringProperty()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    public class Foo
    {
        public Type Boo { get; } = typeof(Foo);

        public override int GetHashCode()
        {
            return Boo.GetHashCode();
        }
    }
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void GetHashCodeRefersToMutableMember_SettableProperty()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class Foo
    {
        public char Boo { get; set; } = '1';

        public override int GetHashCode()
        {
            return Boo.GetHashCode();
        }
    }
}";

        VerifyDiagnostic(original, "GetHashCode() refers to mutable property Boo");
    }

    [TestMethod]
    public void GetHashCodeRefersToMutableMember_PropertyWithBodiedGetter()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class Foo
    {
        public char Boo { get { return '1'; } }

        public override int GetHashCode()
        {
            return Boo.GetHashCode();
        }
    }
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void GetHashCodeRefersToMutableMember_StaticNonValueTypeSettablePropertyWithBodiedGetter()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    public class Foo
    {
        public static Type Boo { get { return typeof(Foo); } set { } }

        public override int GetHashCode()
        {
            return Boo.GetHashCode();
        }
    }
}";

        VerifyDiagnostic(original, "GetHashCode() refers to mutable property Boo");
    }

    [TestMethod]
    public void GetHashCodeRefersToMutableMember_PropertyWithExpressionBodiedGetter()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class Foo
    {
        public char Boo => '1';

        public override int GetHashCode()
        {
            return Boo.GetHashCode();
        }
    }
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void GetHashCodeRefersToMutableMember_ImmutableProperty_NoDiagnostic()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class Foo
    {
        public char Boo { get; }

        public override int GetHashCode()
        {
            return Boo.GetHashCode();
        }
    }
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void GetHashCodeRefersToMutableMember_ImmutableStringProperty_NoDiagnostic()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class Foo
    {
        public string Boo { get; }

        public override int GetHashCode()
        {
            return Boo.GetHashCode();
        }
    }
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void GetHashCodeRefersToMutableMember_InOtherType_PropertyWithExpressionBodiedGetter()
    {
        var original = @"
namespace ConsoleApplication1
{
    public struct Bar
    {
        public int Fuzz => 0;
    }

    public class Foo
    {
        private readonly Bar _bar = new Bar();
        public override int GetHashCode() => _bar.Fuzz.GetHashCode();
    }
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void GetHashCodeRefersToMutableMember_InOtherType_ImmutableProperty_NoDiagnostic()
    {
        var original = @"
namespace ConsoleApplication1
{
    public struct Bar
    {
        public int Fizz { get; }
    }

    public class Foo
    {
        private readonly Bar _bar = new Bar();
        public override int GetHashCode() => _bar.Fizz.GetHashCode();
    }
}";

        VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/3")]
    public void GetHashCodeRefersToMutableMember_CallsExternalProperty()
    {
        var original = @"
using System.Text;

namespace ConsoleApplication1
{
    class Test
    {
        private int Temp { get; set; }

        public override int GetHashCode()
        {
            return ASCIIEncoding.ASCII.GetHashCode();
        }
    }
}
";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void GetHashCodeRefersToMutableMember_PartialClass_SameFile()
    {
        var original = @"
partial class ClassX
{
    public string Code { get; set; }
}

partial class ClassX
{
    public bool Equals(ClassX other) => Code == other.Code;

    public override int GetHashCode() => Code.GetHashCode();
}";

        VerifyDiagnostic(original, "GetHashCode() refers to mutable property Code");
    }

    [TestMethod]
    public void GetHashCodeRefersToMutableMember_PartialClass_DifferentFile()
    {
        var file1 = @"
partial class ClassX
{
    public bool Equals(ClassX other) => Code == other.Code;

    public override int GetHashCode() => Code.GetHashCode();
}";

        var file2 = @"
partial class ClassX
{
    public string Code { get; set; }
}";

        VerifyDiagnostic(new string[] { file1, file2 }, "GetHashCode() refers to mutable property Code");
    }
}