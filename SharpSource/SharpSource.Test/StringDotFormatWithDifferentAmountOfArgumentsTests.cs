using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class StringDotFormatWithDifferentAmountOfArgumentsTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new StringDotFormatWithDifferentAmountOfArgumentsAnalyzer();

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithValidScenario()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {0}, def {1}"", 1, 2);
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithRepeatedPlaceholders()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {0}, def {0}"", 1);
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithExtraArguments()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {0}, def {1}"", 1, 2, 3, 4, 5);
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithLackingArguments()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {0}, def {1}"", 1);
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithLackingArguments_AndSkippedPlaceholderIndex()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {1}, def {2}"", 123, 456);
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithEqualAmountOfPlaceholdersAndArgumentsButDontMatchUp()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {0}, def {0}"", 1, 2);
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithEscapedPlaceholder()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {0}, def {{1}}"", 1);
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithPlaceholderFormatting()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {1:00}, def {1}"", 1);
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_InDifferentOrder()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {1}, def {0}"", 1);
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithoutFormatLiteral()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string format = ""abc {0}, def {1}"";
            string s = string.Format(format, 1, 2);
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithInterpolatedString()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string name = ""Jeroen"";
            string s = string.Format($""abc {name}, def {0} ghi {1}"", 1);
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithInterpolatedString_AndCultureInfo()
    {
        var original = @"
using System;
using System.Globalization;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string name = ""Jeroen"";
            string s = string.Format(CultureInfo.InvariantCulture, $""abc {name}, def {0} ghi {1}"", 1);
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithFormatProvider()
    {
        var original = @"
using System;
using System.Globalization;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(CultureInfo.InvariantCulture, ""def {0} ghi {1}"", 1);
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithFormatProvider_AndFormat_AndNoArguments()
    {
        var original = @"
using System;
using System.Globalization;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(CultureInfo.InvariantCulture, ""abc {0}"");
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithEscapedBraces()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""def {0} ghi {{1}}"", 1);
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithNestedBraces()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""{{def {0} ghi {1}}}"", 1);
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }


    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithSimilarInvocation_WithWrongTypes()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        MyClass()
        {
            Method(""{{def {0} ghi {1}}}"", 1);
        }

        void Method(string format, int x)
        {
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithSimilarInvocation_WithCorrectTypes()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        MyClass()
        {
            Method(""{{def {0} ghi {1}}}"", 1);
        }

        void Method(string format, object x)
        {
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithSimilarInvocation_WithWrongFormatParamName()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        MyClass()
        {
            Method(""{{def {0} ghi {1}}}"", 1);
        }

        void Method(string s, object x)
        {
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithoutArguments()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {0}, def {1}"");
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithoutPlaceholders()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc, def"");
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithExplicitArray()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {0}"", new object[] {""hello""});
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithExplicitArrayAndLackingArguments()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {0} {1}"", new object[] {""hello""});
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }


    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithExplicitArrayMultipleArguments()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {0} {1}"", new object[] {""hello"", ""bye""});
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithExplicitArrayReferencedThroughIdentifierReferencingAnotherMethod()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            var args = getArgs();
            string s = string.Format(""abc {0} {1}"", args);
        }

        object[] getArgs()
        {
            return new object[] {""hello"", ""bye""};
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithExplicitArrayReferencedThroughIdentifierReferencingAnotherMethod_WithLackingArgs()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            var args = getArgs();
            string s = string.Format(""abc {0} {1}"", args);
        }

        object[] getArgs()
        {
            return new object[] {""hello""};
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithExplicitArrayReferenced()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            object[] arr = new object[] {""hello"", ""bye""};
            string s = string.Format(""abc {0} {1}"", arr);
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithExplicitArrayReferencedThroughMethodCall()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {0} {1}"", getArguments());
        }

        object[] getArguments()
        {
            return new object[] {""hello"", ""bye""};
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithExplicitArrayAndAdditionalArguments()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {0} {1} {2}"", new object[] {""hello"", ""bye"", ""uhoh""}, ""test"");
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithFormatProvider_AndExplicitArray()
    {
        var original = @"
using System;
using System.Globalization;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(CultureInfo.InvariantCulture, ""abc {0}"", new object[] {""hello""});
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithFormatProvider_AndExplicitArrayWithLackingArguments()
    {
        var original = @"
using System;
using System.Globalization;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(CultureInfo.InvariantCulture, ""abc {0}{1}"", new object[] {""hello""});
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithFormatProvider_AndExplicitArrayReferencedThroughVariable()
    {
        var original = @"
using System;
using System.Globalization;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            var args = new object[] {""hello""};
            string s = string.Format(CultureInfo.InvariantCulture, ""abc {0}{1}"", args);
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithFormatProvider_AndExplicitArrayReferencedThroughMethod()
    {
        var original = @"
using System;
using System.Globalization;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(CultureInfo.InvariantCulture, ""abc {0}{1}"", getArgs());
        }

        object[] getArgs()
        {
            return new object[] {""hello""};
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithConstantFormat()
    {
        var original = @"
using System;
using System.Globalization;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            const string format = ""{0}{1}"";
            string s = string.Format(format, ""arg"");
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithConstantConcatenation()
    {
        var original = @"
using System;
using System.Globalization;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            const string a = ""{0}"";
            const string b = ""{1}"";
            const string format = a + b;
            string s = string.Format(format, ""arg"");
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithStaticImport()
    {
        var original = @"
using static System.String;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            const string a = ""{0}"";
            const string b = ""{1}"";
            const string format = a + b;
            string s = Format(format, ""arg"");
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithParenthesizedExpression()
    {
        var original = @"
using System;
using System.Globalization;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format((""{0}{1}""), ""arg"");
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithConsoleWriteLine_AndSingleObject()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            Console.WriteLine(""{0}{1}"", ""arg"");
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithConsoleWriteLine_AndParamsObject()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            Console.WriteLine(""{0}{1}{2}"", ""arg"", ""arg2"");
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithConsoleWriteLine_AndObjectArray()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            Console.WriteLine(""{0}{1}{2}"", new object[] { ""arg"", ""arg2"" });
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithConsoleWriteLine_AndObjectArrayReference()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            var args = new object[] { ""arg"", ""arg2"" };
            Console.WriteLine(""{0}{1}{2}"", args);
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithSimilarInvocation_AndExtraParameters()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            Other(""{0}{1}{2}"", new object[] { ""arg"", ""arg2"" }, 5);
        }

        void Other(string format, object[] args, int something)
        {
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithSimilarInvocation_AndExtraObjectArrayParameters()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            Other(""{0}{1}{2}"", new object[] { ""arg"", ""arg2"" }, new object[] {});
        }

        void Other(string format, object[] args, object[] args2)
        {
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithObjectArrayAsObject()
    {
        var original = @"
using System;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            var args = new object[] { ""a"", ""b""};
            string s = string.Format(""{0}{1}"", (object) args);
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithParenthesizedExplicitArray()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {0}{1}"", (new[] { 5 }));
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithParenthesizedExplicitArray_WithValidScenario()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {0}{1}"", (new[] { 5, 2 }));
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithMethod_ThatDoesNotReturnAnArray_WithLackingArguments_WithOneArgument()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {0}{1}"", Other());
        }

        int Other()
        {
            return 4;
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithMethod_WithParenthesizedMethodCall()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {0}{1}"", (Other()));
        }

        int Other()
        {
            return 4;
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithMethod_ThatDoesNotReturnAnArray_WithLackingArguments_WithTwoArguments()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {0}{1}{2}"", Other(), Other());
        }

        int Other()
        {
            return 4;
        }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithMethod_ThatDoesNotReturnAnArray()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string s = string.Format(""abc {0}{1}"", Other(), Another());
        }

        int Other()
        {
            return 4;
        }

        object Another()
        {
            return 5;
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithConcatArgs()
    {
        var original = @"
using System;
using System.Linq;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string.Format(""{0}{1}"", new[] { 1 }.Concat(new[] {2}).ToArray());
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithLackingConcatArgs()
    {
        var original = @"
using System;
using System.Linq;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string.Format(""{0}{1}{2}"", new[] { 1 }.Concat(new[] {2}).ToArray());
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithObjectInitializer()
    {
        var original = @"
using System;
using System.Linq;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method(string input)
        {
            string.Format(""{0}{1}"", new MyClass { Prop1 = 5, Prop2 = 6});
        }

	    public int Prop1 { get; set; }
	    public int Prop2 { get; set; }
    }
}";
        await VerifyDiagnostic(original, StringDotFormatWithDifferentAmountOfArgumentsAnalyzer.Rule.MessageFormat.ToString());
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithNonStringFormatType()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        string Method(SymbolDisplayFormat format)
        {
            string s = Method(new SymbolDisplayFormat());
            return s;
        }
    }

    class SymbolDisplayFormat
    {

    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithNonStringLiteralFormatType()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        string Method(int format)
        {
            string s = Method(5);
            return s;
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithOptionalFormat()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        string Method(string format = null)
        {
            string s = Method();
            return s;
        }
    }
}";
        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithOptionalFormatAndArguments()
    {
        var original = @"
using System;
using System.Text;

namespace ConsoleApplication1
{
    class MyClass
    {
        string Format(string format = null, object[] param = null)
        {
            string s = Format();
            return s;
        }
    }
}";
        await VerifyDiagnostic(original);
    }
}