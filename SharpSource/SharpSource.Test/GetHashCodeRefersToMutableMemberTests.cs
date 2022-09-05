using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class GetHashCodeRefersToMutableMemberTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new GetHashCodeRefersToMutableMemberAnalyzer();

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_ConstantFieldAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_NonReadonlyFieldAsync()
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

        await VerifyDiagnostic(original, "GetHashCode() refers to mutable field _boo");
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_StaticFieldAsync()
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

        await VerifyDiagnostic(original, "GetHashCode() refers to mutable field _boo");
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_NonReadonlyStaticNonValueTypeFieldAsync()
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

        await VerifyDiagnostic(original, "GetHashCode() refers to mutable field _boo");
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_NonValueTypeNonStringFieldAsync()
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

        await VerifyDiagnostic(original, "GetHashCode() refers to mutable field _boo");
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_ImmutableMember_NoWarningAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_ImmutableStringMember_NoWarningAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_StaticReadonlyPropertyAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_NonValueTypeNonStringPropertyAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_SettablePropertyAsync()
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

        await VerifyDiagnostic(original, "GetHashCode() refers to mutable property Boo");
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_PropertyWithBodiedGetterAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_StaticNonValueTypeSettablePropertyWithBodiedGetterAsync()
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

        await VerifyDiagnostic(original, "GetHashCode() refers to mutable property Boo");
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_PropertyWithExpressionBodiedGetterAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_ImmutableProperty_NoDiagnosticAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_ImmutableStringProperty_NoDiagnosticAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_InOtherType_PropertyWithExpressionBodiedGetterAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_InOtherType_ImmutableProperty_NoDiagnosticAsync()
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

        await VerifyDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/3")]
    public async Task GetHashCodeRefersToMutableMember_CallsExternalPropertyAsync()
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_PartialClass_SameFileAsync()
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

        await VerifyDiagnostic(original, "GetHashCode() refers to mutable property Code");
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_PartialClass_DifferentFile()
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

        await VerifyDiagnostic(new string[] { file1, file2 }, "GetHashCode() refers to mutable property Code");
    }
}