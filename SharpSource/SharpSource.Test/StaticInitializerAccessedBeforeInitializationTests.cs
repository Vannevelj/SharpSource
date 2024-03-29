using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Test.Helpers;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.StaticInitializerAccessedBeforeInitializationAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class StaticInitializerAccessedBeforeInitializationTests
{
    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_StaticFields()
    {
        var original = @"
class Test
{
	public static int FirstField = {|#0:SecondField|};
	private static int SecondField = 5;
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("FirstField accesses SecondField but both are marked as static and SecondField will not be initialized when it is used"));
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_AccessingType()
    {
        var original = @"
class Test
{
	public static int FirstField = {|#0:Test.SecondField|};
	private static int SecondField = 5;
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("FirstField accesses SecondField but both are marked as static and SecondField will not be initialized when it is used"));
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_InstanceAndStaticField()
    {
        var original = @"
class Test
{
	public int FirstField = SecondField;
	private static int SecondField = 5;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_Property()
    {
        var original = @"
class Test
{
	public static int FirstField => SecondField;
	private static int SecondField = 5;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_PartialStruct()
    {
        var original = @"
partial struct Test
{
	public static int FirstField = {|#0:SecondField|};
}

partial struct Test
{
	private static int SecondField = 5;
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("FirstField accesses SecondField but both are marked as static and SecondField will not be initialized when it is used"));
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_PartialClass_SameFile()
    {
        var original = @"
partial class Test
{
	public static int FirstField = {|#0:SecondField|};
}

partial class Test
{
	private static int SecondField = 5;
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("FirstField accesses SecondField but both are marked as static and SecondField will not be initialized when it is used"));
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_PartialClass_DifferentFiles()
    {
        var file1 = @"
partial class Test
{
	public static int FirstField = {|#0:SecondField|};
}";

        var file2 = @"
partial class Test
{
	private static int SecondField = 5;
}";

        await VerifyCS.VerifyCodeFix(file1, new[] { VerifyCS.Diagnostic().WithMessage("FirstField accesses SecondField but both are marked as static and SecondField will not be initialized when it is used") }, file1, additionalFiles: new[] { file2 });
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_ReadOnlyFields_Instance()
    {
        var original = @"
class Test
{
	public readonly int FirstField = SecondField;
	private static readonly int SecondField = 5;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_ReadOnlyFields_Static()
    {
        var original = @"
class Test
{
	public static readonly int FirstField = {|#0:SecondField|};
	private static readonly int SecondField = 5;
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("FirstField accesses SecondField but both are marked as static and SecondField will not be initialized when it is used"));
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_AcceptableOrder()
    {
        var original = @"
class Test
{
    private static readonly int SecondField = 5;
	public static readonly int FirstField = SecondField;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_AccessesMultiple()
    {
        var original = @"
class Test
{
	public static int FirstField = {|#0:SecondField|} + {|#1:ThirdField|};
	private static int SecondField = 5;
    private static int ThirdField = 5;
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic(location: 0).WithMessage("FirstField accesses SecondField but both are marked as static and SecondField will not be initialized when it is used"),
            VerifyCS.Diagnostic(location: 1).WithMessage("FirstField accesses ThirdField but both are marked as static and ThirdField will not be initialized when it is used"));
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_SomethingInBetween()
    {
        var original = @"
class Test
{
	public static int FirstField = {|#0:ThirdField|};
	private int SecondField = 5;
    private static int ThirdField = 5;
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("FirstField accesses ThirdField but both are marked as static and ThirdField will not be initialized when it is used"));
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_StaticFieldOfDifferentType()
    {
        var original = @"
class Test
{
	public static int FirstField = Other.ThirdField;
}

class Other
{
    public static int ThirdField = 5;
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_NoInitializer()
    {
        var original = @"
class Test
{
	public static int FirstField;
    private static int ThirdField = 5;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_MultipleDeclarators()
    {
        var original = @"
class Test
{
	public static int FirstField, SecondField = {|#0:ThirdField|};
    private static int ThirdField = 5;
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("SecondField accesses ThirdField but both are marked as static and ThirdField will not be initialized when it is used"));
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_MultipleDeclarators_WithInitializers()
    {
        var original = @"
class Test
{
	public static int FirstField = {|#0:ThirdField|}, SecondField = {|#1:ThirdField|};
    private static int ThirdField = 5;
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic(location: 0).WithMessage("FirstField accesses ThirdField but both are marked as static and ThirdField will not be initialized when it is used"),
            VerifyCS.Diagnostic(location: 1).WithMessage("SecondField accesses ThirdField but both are marked as static and ThirdField will not be initialized when it is used"));
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_MultipleDeclarators_WithInitializers_SomeGood()
    {
        var original = @"
class Test
{
	public static int FirstField = 32, SecondField = {|#0:ThirdField|};
    private static int ThirdField = 5;
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("SecondField accesses ThirdField but both are marked as static and ThirdField will not be initialized when it is used"));
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_DoesNotReferenceOtherFields()
    {
        var original = @"
class Test
{
	public static int FirstField = 32;
	private static int SecondField = 5;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_ChainedAssignment()
    {
        var original = @"
class Test
{
	public static int FirstField = {|#0:SecondField|} = {|#1:ThirdField|};
	private static int SecondField = 5;
	private static int ThirdField = 32;
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic(location: 0).WithMessage("FirstField accesses SecondField but both are marked as static and SecondField will not be initialized when it is used"),
            VerifyCS.Diagnostic(location: 1).WithMessage("FirstField accesses ThirdField but both are marked as static and ThirdField will not be initialized when it is used"));
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_StaticFields_Multiple()
    {
        var original = @"
class Test
{
	public static int FirstField = {|#0:SecondField|} + {|#1:ThirdField|};
	private static int SecondField = 5;
    private static int ThirdField = 32;
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic(location: 0).WithMessage("FirstField accesses SecondField but both are marked as static and SecondField will not be initialized when it is used"),
            VerifyCS.Diagnostic(location: 1).WithMessage("FirstField accesses ThirdField but both are marked as static and ThirdField will not be initialized when it is used"));
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_Struct()
    {
        var original = @"
struct Test
{
	public static int FirstField = {|#0:SecondField|};
	public static int SecondField = 32;
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("FirstField accesses SecondField but both are marked as static and SecondField will not be initialized when it is used"));
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/172")]
    public async Task StaticInitializerAccessedBeforeInitialization_NameOf()
    {
        var original = @"
struct Test
{
	static string FirstField = nameof(SecondField);
    static int SecondField = 32;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_NameOf_WithComputation()
    {
        var original = @"
struct Test
{
    static string FirstField = nameof(Test) + {|#0:SecondField|};
    static string SecondField = ""CF"";
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("FirstField accesses SecondField but both are marked as static and SecondField will not be initialized when it is used"));
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/171")]
    public async Task StaticInitializerAccessedBeforeInitialization_MethodInvocation()
    {
        var original = @"
class Test
{
    static string FirstField = SomeFunction();
    static string SomeFunction() => string.Empty;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/314")]
    public async Task StaticInitializerAccessedBeforeInitialization_AsArgumentToMethodInvocation()
    {
        var original = @"
class Test
{
	static string FirstField = SomeFunction(SomeArg);
	static string SomeFunction(string arg) => arg;
	static string SomeArg = ""test"";
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/173")]
    public async Task StaticInitializerAccessedBeforeInitialization_Action()
    {
        var original = @"
using System;

class Test
{
	static Action<int> FirstField = DoThing;
	static void DoThing(int i) { }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/173")]
    public async Task StaticInitializerAccessedBeforeInitialization_Func()
    {
        var original = @"
using System;

class Test
{
	static Func<int> FirstField = DoThing;
	static int DoThing() => 32;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/181")]
    public async Task StaticInitializerAccessedBeforeInitialization_Lazy_WithInt()
    {
        var original = @"
using System;

class Test
{
    public static Lazy<int> FirstField = new({|#0:DoThing|});
    public static int DoThing = 32;
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("FirstField accesses DoThing but both are marked as static and DoThing will not be initialized when it is used"));
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/181")]
    public async Task StaticInitializerAccessedBeforeInitialization_Lazy_WithMethod()
    {
        var original = @"
using System;

class Test
{ 
    public static Lazy<int> FirstField = new Lazy<int>(DoThing);
    public static int DoThing() => 5;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/181")]
    public async Task StaticInitializerAccessedBeforeInitialization_Lazy_WithMethod_ShortHand()
    {
        var original = @"
using System;

class Test
{ 
    public static Lazy<int> FirstField = new(DoThing);
    public static int DoThing() => 5;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/183")]
    public async Task StaticInitializerAccessedBeforeInitialization_Const()
    {
        var original = @"
using System;

class Test
{
	public static readonly int FirstField = SecondField;
	public const int SecondField = 5;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/187")]
    public async Task StaticInitializerAccessedBeforeInitialization_ReferencesItself()
    {
        var original = @"
using System.Linq;

class Test
{
	public static int FirstField = Enumerable.Repeat(0, FirstField).First();
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/189")]
    public async Task StaticInitializerAccessedBeforeInitialization_PassingFunctionReference()
    {
        var original = @"
using System;

class Test
{
	public static Other FirstField = new Other(DoThing);
	private static void DoThing() {}
}

class Other 
{
	public Other(Action callback) { }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/188")]
    public async Task StaticInitializerAccessedBeforeInitialization_WithNestedLambda()
    {
        var original = @"
using System;

class Test
{ 
    public static Lazy<int> FirstField = new Lazy<int>(() => SomeValue);
    public static int SomeValue = 5;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_Func_New()
    {
        var original = @"
using System;

class Test
{ 
    public static Func<int> FirstField = new Func<int>(() => SomeValue);
    public static int SomeValue = 5;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_PartialStruct_Good()
    {
        var original = @"
partial struct Test
{
	public static int FirstField = 5;
}

partial struct Test
{
	private static int SecondField = FirstField;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/244")]
    public async Task StaticInitializerAccessedBeforeInitialization_DifferentTypeSameMemberName()
    {
        var original = @"
using System.Collections.Immutable;

class Other
{
    public const string Key = ""Other key"";
}

class Test
{
    public static ImmutableHashSet<string> All = ImmutableHashSet.Create(""first"", Other.Key, ""last"");
    public static string Key = ""key"";
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}