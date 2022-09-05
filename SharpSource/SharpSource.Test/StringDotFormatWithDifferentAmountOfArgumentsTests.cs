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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithValidScenarioAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithRepeatedPlaceholdersAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithExtraArgumentsAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithLackingArgumentsAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithLackingArguments_AndSkippedPlaceholderIndexAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithEqualAmountOfPlaceholdersAndArgumentsButDontMatchUpAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithEscapedPlaceholderAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithPlaceholderFormattingAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_InDifferentOrderAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithoutFormatLiteralAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithInterpolatedStringAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithInterpolatedString_AndCultureInfoAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithFormatProviderAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithFormatProvider_AndFormat_AndNoArgumentsAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithEscapedBracesAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithNestedBracesAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithSimilarInvocation_WithWrongTypesAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithSimilarInvocation_WithCorrectTypesAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithSimilarInvocation_WithWrongFormatParamNameAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithoutArgumentsAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithoutPlaceholdersAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithExplicitArrayAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithExplicitArrayAndLackingArgumentsAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithExplicitArrayMultipleArgumentsAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithExplicitArrayReferencedThroughIdentifierReferencingAnotherMethodAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithExplicitArrayReferencedThroughIdentifierReferencingAnotherMethod_WithLackingArgsAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithExplicitArrayReferencedAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithExplicitArrayReferencedThroughMethodCallAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithExplicitArrayAndAdditionalArgumentsAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithFormatProvider_AndExplicitArrayAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithFormatProvider_AndExplicitArrayWithLackingArgumentsAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithFormatProvider_AndExplicitArrayReferencedThroughVariableAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithFormatProvider_AndExplicitArrayReferencedThroughMethodAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithConstantFormatAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithConstantConcatenationAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithStaticImportAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithParenthesizedExpressionAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithConsoleWriteLine_AndSingleObjectAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithConsoleWriteLine_AndParamsObjectAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithConsoleWriteLine_AndObjectArrayAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithConsoleWriteLine_AndObjectArrayReferenceAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithSimilarInvocation_AndExtraParametersAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithSimilarInvocation_AndExtraObjectArrayParametersAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithObjectArrayAsObjectAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithParenthesizedExplicitArrayAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithParenthesizedExplicitArray_WithValidScenarioAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithMethod_ThatDoesNotReturnAnArray_WithLackingArguments_WithOneArgumentAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithMethod_WithParenthesizedMethodCallAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithMethod_ThatDoesNotReturnAnArray_WithLackingArguments_WithTwoArgumentsAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithMethod_ThatDoesNotReturnAnArrayAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithConcatArgsAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithLackingConcatArgsAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithObjectInitializerAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithNonStringFormatTypeAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithNonStringLiteralFormatTypeAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithOptionalFormatAsync()
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
    public async Task StringDotFormatWithDifferentAmountOfArguments_WithOptionalFormatAndArgumentsAsync()
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