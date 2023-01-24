using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.GetHashCodeRefersToMutableMemberAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class GetHashCodeRefersToMutableMemberTests
{
    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_ConstantField()
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_NonReadonlyField()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class Foo
    {
        private char _boo = '1';

        public override int GetHashCode()
        {
            return {|#0:_boo|}.GetHashCode();
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(GetHashCodeRefersToMutableMemberAnalyzer.FieldRule).WithMessage("GetHashCode() refers to mutable field _boo"));
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_StaticField()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class Foo
    {
        private static readonly char _boo = '1';

        public override int GetHashCode()
        {
            return {|#0:_boo|}.GetHashCode();
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(GetHashCodeRefersToMutableMemberAnalyzer.FieldRule).WithMessage("GetHashCode() refers to mutable field _boo"));
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_NonReadonlyStaticNonValueTypeField()
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
            return {|#0:_boo|}.GetHashCode();
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(GetHashCodeRefersToMutableMemberAnalyzer.FieldRule).WithMessage("GetHashCode() refers to mutable field _boo"));
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_NonValueTypeNonStringField()
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
            return {|#0:_boo|}.GetHashCode();
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(GetHashCodeRefersToMutableMemberAnalyzer.FieldRule).WithMessage("GetHashCode() refers to mutable field _boo"));
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_ImmutableMember_NoWarning()
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_ImmutableStringMember_NoWarning()
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_StaticReadonlyProperty()
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_NonValueTypeNonStringProperty()
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_SettableProperty()
    {
        var original = @"
namespace ConsoleApplication1
{
    public class Foo
    {
        public char Boo { get; set; } = '1';

        public override int GetHashCode()
        {
            return {|#0:Boo|}.GetHashCode();
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(GetHashCodeRefersToMutableMemberAnalyzer.PropertyRule).WithMessage("GetHashCode() refers to mutable property Boo"));
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_PropertyWithBodiedGetter()
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_StaticNonValueTypeSettablePropertyWithBodiedGetter()
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
            return {|#0:Boo|}.GetHashCode();
        }
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(GetHashCodeRefersToMutableMemberAnalyzer.PropertyRule).WithMessage("GetHashCode() refers to mutable property Boo"));
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_PropertyWithExpressionBodiedGetter()
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_ImmutableProperty_NoDiagnostic()
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_ImmutableStringProperty_NoDiagnostic()
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_InOtherType_PropertyWithExpressionBodiedGetter()
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_InOtherType_ImmutableProperty_NoDiagnostic()
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/3")]
    public async Task GetHashCodeRefersToMutableMember_CallsExternalProperty()
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

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_PartialClass_SameFile()
    {
        var original = @"
partial class ClassX
{
    public string Code { get; set; }
}

partial class ClassX
{
    public bool Equals(ClassX other) => Code == other.Code;

    public override int GetHashCode() => {|#0:Code|}.GetHashCode();
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic(GetHashCodeRefersToMutableMemberAnalyzer.PropertyRule).WithMessage("GetHashCode() refers to mutable property Code"));
    }

    [TestMethod]
    public async Task GetHashCodeRefersToMutableMember_PartialClass_DifferentFile()
    {
        var file1 = @"
partial class ClassX
{
    public bool Equals(ClassX other) => Code == other.Code;

    public override int GetHashCode() => {|#0:Code|}.GetHashCode();
}";

        var file2 = @"
partial class ClassX
{
    public string Code { get; set; }
}";

        await VerifyCS.VerifyAnalyzerAsync(file1, additionalFiles: new[] { file2 }, VerifyCS.Diagnostic(GetHashCodeRefersToMutableMemberAnalyzer.PropertyRule).WithMessage("GetHashCode() refers to mutable property Code"));
    }
}