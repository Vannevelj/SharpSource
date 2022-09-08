using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class StaticInitializerAccessedBeforeInitializationTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new StaticInitializerAccessedBeforeInitializationAnalyzer();


    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_StaticFields()
    {
        var original = @"
class Test
{
	public static int FirstField = SecondField;
	private static int SecondField = 5;
}";

        await VerifyDiagnostic(original, "FirstField accesses SecondField but both are marked as static and SecondField will not be initialized when it is used");
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_AccessingType()
    {
        var original = @"
class Test
{
	public static int FirstField = Test.SecondField;
	private static int SecondField = 5;
}";

        await VerifyDiagnostic(original, "FirstField accesses SecondField but both are marked as static and SecondField will not be initialized when it is used");
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

        await VerifyDiagnostic(original);
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_PartialStruct()
    {
        var original = @"
partial struct Test
{
	public static int FirstField = SecondField;
}

partial struct Test
{
	private static int SecondField = 5;
}";

        await VerifyDiagnostic(original, "FirstField accesses SecondField but both are marked as static and SecondField might not be initialized when it is used");
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_PartialClass_SameFile()
    {
        var original = @"
partial class Test
{
	public static int FirstField = SecondField;
}

partial class Test
{
	private static int SecondField = 5;
}";

        await VerifyDiagnostic(original, "FirstField accesses SecondField but both are marked as static and SecondField might not be initialized when it is used");
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_PartialClass_DifferentFiles()
    {
        var file1 = @"
partial class Test
{
	public static int FirstField = SecondField;
}";

        var file2 = @"
partial class Test
{
	private static int SecondField = 5;
}";

        await VerifyDiagnostic(new[] { file1, file2 }, "FirstField accesses SecondField but both are marked as static and SecondField might not be initialized when it is used");
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_ReadOnlyFields_Static()
    {
        var original = @"
class Test
{
	public static readonly int FirstField = SecondField;
	private static readonly int SecondField = 5;
}";

        await VerifyDiagnostic(original, "FirstField accesses SecondField but both are marked as static and SecondField will not be initialized when it is used");
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_AccessesMultiple()
    {
        var original = @"
class Test
{
	public static int FirstField = SecondField + ThirdField;
	private static int SecondField = 5;
    private static int ThirdField = 5;
}";

        await VerifyDiagnostic(original,
            "FirstField accesses SecondField but both are marked as static and SecondField will not be initialized when it is used",
            "FirstField accesses ThirdField but both are marked as static and ThirdField will not be initialized when it is used");
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_SomethingInBetween()
    {
        var original = @"
class Test
{
	public static int FirstField = ThirdField;
	private int SecondField = 5;
    private static int ThirdField = 5;
}";

        await VerifyDiagnostic(original, "FirstField accesses ThirdField but both are marked as static and ThirdField will not be initialized when it is used");
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

        await VerifyDiagnostic(original);
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_MultipleDeclarators()
    {
        var original = @"
class Test
{
	public static int FirstField, SecondField = ThirdField;
    private static int ThirdField = 5;
}";

        await VerifyDiagnostic(original, "SecondField accesses ThirdField but both are marked as static and ThirdField will not be initialized when it is used");
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_MultipleDeclarators_WithInitializers()
    {
        var original = @"
class Test
{
	public static int FirstField = ThirdField, SecondField = ThirdField;
    private static int ThirdField = 5;
}";

        await VerifyDiagnostic(original,
            "FirstField accesses ThirdField but both are marked as static and ThirdField will not be initialized when it is used",
            "SecondField accesses ThirdField but both are marked as static and ThirdField will not be initialized when it is used");
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_MultipleDeclarators_WithInitializers_SomeGood()
    {
        var original = @"
class Test
{
	public static int FirstField = 32, SecondField = ThirdField;
    private static int ThirdField = 5;
}";

        await VerifyDiagnostic(original, "SecondField accesses ThirdField but both are marked as static and ThirdField will not be initialized when it is used");
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

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_ChainedAssignment()
    {
        var original = @"
class Test
{
	public static int FirstField = SecondField = ThirdField;
	private static int SecondField = 5;
	private static int ThirdField = 32;
}";

        await VerifyDiagnostic(original,
            "FirstField accesses SecondField but both are marked as static and SecondField will not be initialized when it is used",
            "FirstField accesses ThirdField but both are marked as static and ThirdField will not be initialized when it is used");
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_StaticFields_Multiple()
    {
        var original = @"
class Test
{
	public static int FirstField = SecondField + ThirdField;
	private static int SecondField = 5;
    private static int ThirdField = 32;
}";

        await VerifyDiagnostic(original,
            "FirstField accesses SecondField but both are marked as static and SecondField will not be initialized when it is used",
            "FirstField accesses ThirdField but both are marked as static and ThirdField will not be initialized when it is used");
    }

    [TestMethod]
    public async Task StaticInitializerAccessedBeforeInitialization_Struct()
    {
        var original = @"
struct Test
{
	public static int FirstField = SecondField;
	public static int SecondField = 32;
}";

        await VerifyDiagnostic(original, "FirstField accesses SecondField but both are marked as static and SecondField will not be initialized when it is used");
    }
}