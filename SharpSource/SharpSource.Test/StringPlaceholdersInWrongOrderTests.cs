using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Test.Helpers;
using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.StringPlaceholdersInWrongOrderAnalyzer, SharpSource.Diagnostics.StringPlaceHoldersInWrongOrderCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class StringPlaceholdersInWrongOrderTests
{
    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_InCorrectOrder_WithSingleOccurrence()
    {
        var original = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = string.Format(""Hello {0}, my name is {1}"", ""Mr. Test"", ""Mr. Tester"");
            }
        }
    }";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_InCorrectOrder_WithMultipleOccurrences()
    {
        var original = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = string.Format(""Hello {0}, my name is {1}. Yes you heard that right, {1}."", ""Mr. Test"", ""Mr. Tester"");
            }
        }
    }";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_InIncorrectOrder_WithMultipleOccurrences()
    {
        var original = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = {|#0:string.Format(""Hello {1}, my name is {0}. Yes you heard that right, {0}."", ""Mr. Test"", ""Mr. Tester"")|};
            }
        }
    }";

        var expected = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = string.Format(""Hello {0}, my name is {1}. Yes you heard that right, {1}."", ""Mr. Tester"", ""Mr. Test"");
            }
        }
    }"
        ;

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("string.Format() Placeholders are not in ascending order."), expected);
    }

    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_InIncorrectOrder_WithSingleOccurrence()
    {
        var original = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = {|#0:string.Format(""Hello {1}, my name is {0}."", ""Mr. Test"", ""Mr. Tester"")|};
            }
        }
    }";

        var expected = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = string.Format(""Hello {0}, my name is {1}."", ""Mr. Tester"", ""Mr. Test"");
            }
        }
    }";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("string.Format() Placeholders are not in ascending order."), expected);
    }

    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_InIncorrectOrder_WithUnusedPlaceholder()
    {
        var original = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = {|#0:string.Format(""Hello {2}, my name is {1}. Yes you heard that right, {1}."", ""Mr. Test"", ""Mr. Tester"", ""Mrs. Testing"")|};
            }
        }
    }";

        var expected = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = string.Format(""Hello {0}, my name is {1}. Yes you heard that right, {1}."", ""Mrs. Testing"", ""Mr. Tester"", ""Mr. Test"");
            }
        }
    }";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("string.Format() Placeholders are not in ascending order."), expected);
    }

    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_InIncorrectOrder_WithMultiplePlaceholders()
    {
        var original = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = {|#0:string.Format(""abc {2} def {0} ghi {1}"", ""x"", ""y"", ""z"")|};
            }
        }
    }";

        var expected = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = string.Format(""abc {0} def {1} ghi {2}"", ""z"", ""x"", ""y"");
            }
        }
    }";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("string.Format() Placeholders are not in ascending order."), expected);
    }

    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_InIncorrectOrder_WithSinglePlaceholder()
    {
        var original = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = string.Format(""abc {1}"", ""x"", ""y"", ""z"");
            }
        }
    }";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_WithFormatDefinedSeparately()
    {
        var original = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string format = ""Hello {0}, my name is {1}"";
                string s = string.Format(format, ""Mr. Test"", ""Mr. Tester"");
            }
        }
    }";
        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_WithInterpolatedString()
    {
        var original = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string name = ""Jeroen"";
                string s = string.Format($""haha {name}, you're so {0}!"", ""funny"");
            }
        }
    }";
        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_InIncorrectOrder_WithFormattedString()
    {
        var original = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                DateTime date = DateTime.Now;
                string formattedDate = {|#0:string.Format(""Hello {1}, it's {0:hh:mm:ss t z}"", date, ""Jeroen"")|};
            }
        }
    }";

        var expected = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                DateTime date = DateTime.Now;
                string formattedDate = string.Format(""Hello {0}, it's {1:hh:mm:ss t z}"", ""Jeroen"", date);
            }
        }
    }";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("string.Format() Placeholders are not in ascending order."), expected);
    }

    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_InIncorrectOrder_WithFormatProvider()
    {
        var original = @"
    using System;
    using System.Text;
    using System.Globalization;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = {|#0:string.Format(CultureInfo.InvariantCulture, ""Hello {1}, my name is {0}."", ""Mr. Test"", ""Mr. Tester"")|};
            }
        }
    }";

        var expected = @"
    using System;
    using System.Text;
    using System.Globalization;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = string.Format(CultureInfo.InvariantCulture, ""Hello {0}, my name is {1}."", ""Mr. Tester"", ""Mr. Test"");
            }
        }
    }";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("string.Format() Placeholders are not in ascending order."), expected);
    }

    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_WithEscapedCurlyBrace()
    {
        var original = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = string.Format(""Hello {{Jeroen}}, my name is {0}"", ""Mr. Test"");
            }
        }
    }";
        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_WithDoubleEscapedCurlyBrace()
    {
        var original = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = {|#0:string.Format(""Hello {{{1}}}, my name is {0}"", ""Mr. Test"", ""Mr. Tester"")|};
            }
        }
    }";

        var expected = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = string.Format(""Hello {{{0}}}, my name is {1}"", ""Mr. Tester"", ""Mr. Test"");
            }
        }
    }";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("string.Format() Placeholders are not in ascending order."), expected);
    }

    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_WithNestedCurlyBraces()
    {
        var original = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = {|#0:string.Format(""{{Hello {1}, my name is {0}}}"", ""Mr. Test"", ""Mr. Tester"")|};
            }
        }
    }";

        var expected = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = string.Format(""{{Hello {0}, my name is {1}}}"", ""Mr. Tester"", ""Mr. Test"");
            }
        }
    }";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("string.Format() Placeholders are not in ascending order."), expected);
    }

    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_WithCommentedPlaceholder_AlsoUsedValidly()
    {
        var original = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = string.Format(""Hello {{0}}, my name is {0}."", ""Mr. Test"");
            }
        }
    }";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_InIncorrectOrder_WithInvalidIndex()
    {
        var original = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = string.Format(""{0} {1} {4} {3}"", ""a"", ""b"", ""c"", ""d"");
            }
        }
    }";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_InIncorrectOrder_WithDifferentMethodName()
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
                Method(""{1} {0}"", 2, 3);
            }

            void Method(string s, int x, int y)
            {
            }
        }
    }";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_WithReusedPlaceholderInDescendingOrder()
    {
        var original = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                string s = string.Format(""{0} {1} {0}"", ""a"", ""b"");
            }
        }
    }";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_StringsAreVariables()
    {
        var original = @"
    using System;
    using System.Text;

    namespace ConsoleApplication1
    {
        class MyClass
        {
            void Method()
            {
                var foo = ""{0} {1}"";
                var bar = ""bizz"";
                var baz = ""buzz"";
                var s = string.Format(foo, bar, baz);
            }
        }
    }";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StringPlaceholdersInWrongOrder_InIncorrectOrder_WithoutStringFormat()
    {
        var original = @"
using System;
using System.Text;

class MyClass
{
    static string Format(string text, params string[] args) => string.Empty;

    void Method()
    {
        string s = MyClass.Format(""Hello {1}, my name is {0}."", ""Mr. Test"", ""Mr. Tester"");
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/297")]
    public async Task StringPlaceholdersInWrongOrder_WithManyPlaceholders()
    {
        var original = @"string s = {|#0:string.Format(""{15}{14}{13}{12}{11}{10}{9}{8}{7}{6}{5}{4}{3}{2}{1}{0}"", 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15)|};";

        var expected = @"string s = string.Format(""{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}"", 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("string.Format() Placeholders are not in ascending order."), expected);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/306")]
    public async Task StringPlaceholdersInWrongOrder_WithReferenceToArguments()
    {
        var original = @"
object[] arguments = new object[] { 1, 2, 3 };
{|#0:string.Format(""{1}{2}{0}"", arguments)|};
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}