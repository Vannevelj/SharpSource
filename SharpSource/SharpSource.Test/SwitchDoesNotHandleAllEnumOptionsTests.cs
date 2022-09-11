using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers;

namespace SharpSource.Test;

[TestClass]
public class SwitchDoesNotHandleAllEnumOptionsTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new SwitchDoesNotHandleAllEnumOptionsAnalyzer();
    protected override CodeFixProvider CodeFixProvider => new SwitchDoesNotHandleAllEnumOptionsCodeFix();

    [TestMethod]
    public async Task SwitchDoesNotHandleAllEnumOptions_MissingEnumStatement()
    {
        var original = @"
namespace ConsoleApplication1
{
    enum MyEnum
    {
        Fizz, Buzz, FizzBuzz
    }

    class MyClass
    {
        void Method()
        {
            var e = MyEnum.Fizz;
            switch (e)
            {
                case MyEnum.Fizz:
                case MyEnum.Buzz:
                    break;
            }
        }
    }
}";

        var result = @"
namespace ConsoleApplication1
{
    enum MyEnum
    {
        Fizz, Buzz, FizzBuzz
    }

    class MyClass
    {
        void Method()
        {
            var e = MyEnum.Fizz;
            switch (e)
            {
                case MyEnum.FizzBuzz:
                    throw new System.NotImplementedException();
                case MyEnum.Fizz:
                case MyEnum.Buzz:
                    break;
            }
        }
    }
}";

        await VerifyDiagnostic(original, SwitchDoesNotHandleAllEnumOptionsAnalyzer.Rule.MessageFormat.ToString());
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchDoesNotHandleAllEnumOptions_AllEnumStatements()
    {
        var original = @"
namespace ConsoleApplication1
{
    enum MyEnum
    {
        Fizz, Buzz, FizzBuzz
    }

    class MyClass
    {
        void Method()
        {
            var e = MyEnum.Fizz;
            switch (e)
            {
                case MyEnum.Fizz:
                case MyEnum.Buzz:
                case MyEnum.FizzBuzz:
                    break;
            }
        }
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task SwitchDoesNotHandleAllEnumOptions_CaseStatementsNotEnum()
    {
        var original = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            switch (""test"")
            {
                case ""Fizz"":
                case ""Buzz"":
                case ""FizzBuzz"":
                    break;
            }
        }
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task SwitchDoesNotHandleAllEnumOptions_CaseHasDefaultStatement_NewStatementsAreAddedAboveDefault()
    {
        var original = @"
namespace ConsoleApplication1
{
    enum MyEnum
    {
        Fizz, Buzz, FizzBuzz
    }

    class MyClass
    {
        void Method()
        {
            var e = MyEnum.Fizz;
            switch (e)
            {
                default:
                    break;
            }
        }
    }
}";

        var result = @"
namespace ConsoleApplication1
{
    enum MyEnum
    {
        Fizz, Buzz, FizzBuzz
    }

    class MyClass
    {
        void Method()
        {
            var e = MyEnum.Fizz;
            switch (e)
            {
                case MyEnum.FizzBuzz:
                    throw new System.NotImplementedException();
                case MyEnum.Buzz:
                    throw new System.NotImplementedException();
                case MyEnum.Fizz:
                    throw new System.NotImplementedException();
                default:
                    break;
            }
        }
    }
}";

        await VerifyDiagnostic(original, SwitchDoesNotHandleAllEnumOptionsAnalyzer.Rule.MessageFormat.ToString());
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchDoesNotHandleAllEnumOptions_MissingEnumStatement_MultipleSections()
    {
        var original = @"
namespace ConsoleApplication1
{
    enum MyEnum
    {
        Fizz, Buzz, FizzBuzz
    }

    class MyClass
    {
        void Method()
        {
            var e = MyEnum.Fizz;
            switch (e)
            {
                case MyEnum.Fizz:
                    break;
                case MyEnum.Buzz:
                    break;
            }
        }
    }
}";

        var result = @"
namespace ConsoleApplication1
{
    enum MyEnum
    {
        Fizz, Buzz, FizzBuzz
    }

    class MyClass
    {
        void Method()
        {
            var e = MyEnum.Fizz;
            switch (e)
            {
                case MyEnum.FizzBuzz:
                    throw new System.NotImplementedException();
                case MyEnum.Fizz:
                    break;
                case MyEnum.Buzz:
                    break;
            }
        }
    }
}";

        await VerifyDiagnostic(original, SwitchDoesNotHandleAllEnumOptionsAnalyzer.Rule.MessageFormat.ToString());
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchDoesNotHandleAllEnumOptions_UsingStaticEnum_MissingEnumStatements()
    {
        var original = @"
using System.IO;
using static System.IO.FileOptions;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = DeleteOnClose;
            switch (e)
            {
                case Asynchronous:
                case DeleteOnClose:
                    break;
            }
        }
    }
}";

        var result = @"
using System.IO;
using static System.IO.FileOptions;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = DeleteOnClose;
            switch (e)
            {
                case Encrypted:
                    throw new System.NotImplementedException();
                case SequentialScan:
                    throw new System.NotImplementedException();
                case FileOptions.RandomAccess:
                    throw new System.NotImplementedException();
                case WriteThrough:
                    throw new System.NotImplementedException();
                case None:
                    throw new System.NotImplementedException();
                case Asynchronous:
                case DeleteOnClose:
                    break;
            }
        }
    }
}";

        await VerifyDiagnostic(original, SwitchDoesNotHandleAllEnumOptionsAnalyzer.Rule.MessageFormat.ToString());
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchDoesNotHandleAllEnumOptions_MissingEnumStatement_AddsAllMissingStatements()
    {
        var original = @"
namespace ConsoleApplication1
{
    enum MyEnum
    {
        Fizz, Buzz, FizzBuzz
    }

    class MyClass
    {
        void Method()
        {
            var e = MyEnum.Fizz;
            switch (e)
            {
                case MyEnum.Buzz:
                    break;
                default:
                    break;
            }
        }
    }
}";

        var result = @"
namespace ConsoleApplication1
{
    enum MyEnum
    {
        Fizz, Buzz, FizzBuzz
    }

    class MyClass
    {
        void Method()
        {
            var e = MyEnum.Fizz;
            switch (e)
            {
                case MyEnum.FizzBuzz:
                    throw new System.NotImplementedException();
                case MyEnum.Fizz:
                    throw new System.NotImplementedException();
                case MyEnum.Buzz:
                    break;
                default:
                    break;
            }
        }
    }
}";

        await VerifyDiagnostic(original, SwitchDoesNotHandleAllEnumOptionsAnalyzer.Rule.MessageFormat.ToString());
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchDoesNotHandleAllEnumOptions_UsingStaticEnum_NoEnumStatements()
    {
        var original = @"
using System.IO;
using static System.IO.FileOptions;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = DeleteOnClose;
            switch (e)
            {
            }
        }
    }
}";

        var result = @"
using System.IO;
using static System.IO.FileOptions;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = DeleteOnClose;
            switch (e)
            {
                case Encrypted:
                    throw new System.NotImplementedException();
                case SequentialScan:
                    throw new System.NotImplementedException();
                case DeleteOnClose:
                    throw new System.NotImplementedException();
                case FileOptions.RandomAccess:
                    throw new System.NotImplementedException();
                case Asynchronous:
                    throw new System.NotImplementedException();
                case WriteThrough:
                    throw new System.NotImplementedException();
                case None:
                    throw new System.NotImplementedException();
            }
        }
    }
}";

        await VerifyDiagnostic(original, SwitchDoesNotHandleAllEnumOptionsAnalyzer.Rule.MessageFormat.ToString());
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchDoesNotHandleAllEnumOptions_UsingStaticEnum_MissingEnumStatement_MixedExpandedEnumStatements()
    {
        var original = @"
using System.IO;
using static System.IO.FileOptions;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = DeleteOnClose;
            switch (e)
            {
                case FileOptions.Encrypted:
                    break;
                case SequentialScan:
                    break;
                case FileOptions.RandomAccess:
                    break;
            }
        }
    }
}";

        var result = @"
using System.IO;
using static System.IO.FileOptions;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = DeleteOnClose;
            switch (e)
            {
                case DeleteOnClose:
                    throw new System.NotImplementedException();
                case Asynchronous:
                    throw new System.NotImplementedException();
                case WriteThrough:
                    throw new System.NotImplementedException();
                case None:
                    throw new System.NotImplementedException();
                case Encrypted:
                    break;
                case SequentialScan:
                    break;
                case FileOptions.RandomAccess:
                    break;
            }
        }
    }
}";

        await VerifyDiagnostic(original, SwitchDoesNotHandleAllEnumOptionsAnalyzer.Rule.MessageFormat.ToString());
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchDoesNotHandleAllEnumOptions_UsingStaticEnum_MissingEnumStatement_AllExpandedEnumStatements()
    {
        var original = @"
using System.IO;
using static System.IO.FileOptions;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = DeleteOnClose;
            switch (e)
            {
                case FileOptions.Encrypted:
                    break;
                case FileOptions.SequentialScan:
                    break;
                case FileOptions.RandomAccess:
                    break;
            }
        }
    }
}";

        var result = @"
using System.IO;
using static System.IO.FileOptions;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = DeleteOnClose;
            switch (e)
            {
                case DeleteOnClose:
                    throw new System.NotImplementedException();
                case Asynchronous:
                    throw new System.NotImplementedException();
                case WriteThrough:
                    throw new System.NotImplementedException();
                case None:
                    throw new System.NotImplementedException();
                case FileOptions.Encrypted:
                    break;
                case FileOptions.SequentialScan:
                    break;
                case FileOptions.RandomAccess:
                    break;
            }
        }
    }
}";

        await VerifyDiagnostic(original, SwitchDoesNotHandleAllEnumOptionsAnalyzer.Rule.MessageFormat.ToString());
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchDoesNotHandleAllEnumOptions_MissingEnumStatement_NoRedundantQualifierIfUsingSystemDirectiveExists()
    {
        var original = @"
using System;
namespace ConsoleApplication1
{
    enum MyEnum
    {
        Fizz, Buzz, FizzBuzz
    }

    class MyClass
    {
        void Method()
        {
            var e = MyEnum.Fizz;
            switch (e)
            {
                case MyEnum.Fizz:
                case MyEnum.Buzz:
                    break;
            }
        }
    }
}";

        var result = @"
using System;
namespace ConsoleApplication1
{
    enum MyEnum
    {
        Fizz, Buzz, FizzBuzz
    }

    class MyClass
    {
        void Method()
        {
            var e = MyEnum.Fizz;
            switch (e)
            {
                case MyEnum.FizzBuzz:
                    throw new NotImplementedException();
                case MyEnum.Fizz:
                case MyEnum.Buzz:
                    break;
            }
        }
    }
}";

        await VerifyDiagnostic(original, SwitchDoesNotHandleAllEnumOptionsAnalyzer.Rule.MessageFormat.ToString());
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchDoesNotHandleAllEnumOptions_MissingEnumStatement_UsingAliasForSystem()
    {
        var original = @"
using Fizz = System;    // seriously...
namespace ConsoleApplication1
{
    enum MyEnum
    {
        Fizz, Buzz, FizzBuzz
    }

    class MyClass
    {
        void Method()
        {
            var e = MyEnum.Fizz;
            switch (e)
            {
                case MyEnum.Fizz:
                case MyEnum.Buzz:
                    break;
            }
        }
    }
}";

        var result = @"
using Fizz = System;    // seriously...
namespace ConsoleApplication1
{
    enum MyEnum
    {
        Fizz, Buzz, FizzBuzz
    }

    class MyClass
    {
        void Method()
        {
            var e = MyEnum.Fizz;
            switch (e)
            {
                case MyEnum.FizzBuzz:
                    throw new Fizz.NotImplementedException();
                case MyEnum.Fizz:
                case MyEnum.Buzz:
                    break;
            }
        }
    }
}";

        await VerifyDiagnostic(original, SwitchDoesNotHandleAllEnumOptionsAnalyzer.Rule.MessageFormat.ToString());
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchDoesNotHandleAllEnumOptions_UsingStaticEnum_MissingEnumStatement_SimplifiesAllStatementsWhenParentDirectiveNotIncluded()
    {
        var original = @"
using static System.IO.FileOptions;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = DeleteOnClose;
            switch (e)
            {
                case Encrypted:
                    break;
                case SequentialScan:
                    break;
                case RandomAccess:
                    break;
            }
        }
    }
}";

        var result = @"
using static System.IO.FileOptions;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = DeleteOnClose;
            switch (e)
            {
                case DeleteOnClose:
                    throw new System.NotImplementedException();
                case Asynchronous:
                    throw new System.NotImplementedException();
                case WriteThrough:
                    throw new System.NotImplementedException();
                case None:
                    throw new System.NotImplementedException();
                case Encrypted:
                    break;
                case SequentialScan:
                    break;
                case RandomAccess:
                    break;
            }
        }
    }
}";

        await VerifyDiagnostic(original, SwitchDoesNotHandleAllEnumOptionsAnalyzer.Rule.MessageFormat.ToString());
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchDoesNotHandleAllEnumOptions_UsingStaticEnum_MissingEnumStatement_SimplifiesAllStatementsWhenUsingDirectiveIncludesWhitespace()
    {
        var original = @"
using static System .       IO      . FileOptions;  // Happy maintaining

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = DeleteOnClose;
            switch (e)
            {
                case Encrypted:
                    break;
                case SequentialScan:
                    break;
                case RandomAccess:
                    break;
            }
        }
    }
}";

        var result = @"
using static System .       IO      . FileOptions;  // Happy maintaining

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = DeleteOnClose;
            switch (e)
            {
                case DeleteOnClose:
                    throw new System.NotImplementedException();
                case Asynchronous:
                    throw new System.NotImplementedException();
                case WriteThrough:
                    throw new System.NotImplementedException();
                case None:
                    throw new System.NotImplementedException();
                case Encrypted:
                    break;
                case SequentialScan:
                    break;
                case RandomAccess:
                    break;
            }
        }
    }
}";

        await VerifyDiagnostic(original, SwitchDoesNotHandleAllEnumOptionsAnalyzer.Rule.MessageFormat.ToString());
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchDoesNotHandleAllEnumOptions_MissingEnumStatement_NestedEnum()
    {
        var original = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        enum MyEnum
        {
            Fizz, Buzz, FizzBuzz
        }

        void Method()
        {
            var e = MyEnum.Fizz;
            switch (e)
            {
                case MyEnum.Fizz:
                case MyEnum.Buzz:
                    break;
            }
        }
    }
}";

        var result = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        enum MyEnum
        {
            Fizz, Buzz, FizzBuzz
        }

        void Method()
        {
            var e = MyEnum.Fizz;
            switch (e)
            {
                case MyEnum.FizzBuzz:
                    throw new System.NotImplementedException();
                case MyEnum.Fizz:
                case MyEnum.Buzz:
                    break;
            }
        }
    }
}";

        await VerifyDiagnostic(original, SwitchDoesNotHandleAllEnumOptionsAnalyzer.Rule.MessageFormat.ToString());
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchDoesNotHandleAllEnumOptions_MissingEnumStatements_CaseValueIsCastToEnumType()
    {
        var original = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        enum MyEnum
        {
            Fizz, Buzz, FizzBuzz
        }

        void Method()
        {
            var e = MyEnum.Fizz;
            switch (e)
            {
                case (MyEnum) 0:
                    break;
            }
        }
    }
}";

        await VerifyDiagnostic(original);
    }

    [TestMethod]
    public async Task SwitchDoesNotHandleAllEnumOptions_NoCaseStatements_NoUsings()
    {
        var original = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = System.IO.FileOptions.DeleteOnClose;
            switch (e)
            {
            }
        }
    }
}";

        var result = @"
namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = System.IO.FileOptions.DeleteOnClose;
            switch (e)
            {
                case System.IO.FileOptions.Encrypted:
                    throw new System.NotImplementedException();
                case System.IO.FileOptions.SequentialScan:
                    throw new System.NotImplementedException();
                case System.IO.FileOptions.DeleteOnClose:
                    throw new System.NotImplementedException();
                case System.IO.FileOptions.RandomAccess:
                    throw new System.NotImplementedException();
                case System.IO.FileOptions.Asynchronous:
                    throw new System.NotImplementedException();
                case System.IO.FileOptions.WriteThrough:
                    throw new System.NotImplementedException();
                case System.IO.FileOptions.None:
                    throw new System.NotImplementedException();
            }
        }
    }
}";

        await VerifyDiagnostic(original, SwitchDoesNotHandleAllEnumOptionsAnalyzer.Rule.MessageFormat.ToString());
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchDoesNotHandleAllEnumOptions_NoCaseStatements_NormalUsing()
    {
        var original = @"
using System.IO;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = FileOptions.DeleteOnClose;
            switch (e)
            {
            }
        }
    }
}";

        var result = @"
using System.IO;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = FileOptions.DeleteOnClose;
            switch (e)
            {
                case FileOptions.Encrypted:
                    throw new System.NotImplementedException();
                case FileOptions.SequentialScan:
                    throw new System.NotImplementedException();
                case FileOptions.DeleteOnClose:
                    throw new System.NotImplementedException();
                case FileOptions.RandomAccess:
                    throw new System.NotImplementedException();
                case FileOptions.Asynchronous:
                    throw new System.NotImplementedException();
                case FileOptions.WriteThrough:
                    throw new System.NotImplementedException();
                case FileOptions.None:
                    throw new System.NotImplementedException();
            }
        }
    }
}";

        await VerifyDiagnostic(original, SwitchDoesNotHandleAllEnumOptionsAnalyzer.Rule.MessageFormat.ToString());
        await VerifyFix(original, result);
    }

    [TestMethod]
    public async Task SwitchDoesNotHandleAllEnumOptions_NoCaseStatements_UsingStatic()
    {
        var original = @"
using static System.IO.FileOptions;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = DeleteOnClose;
            switch (e)
            {
            }
        }
    }
}";

        var result = @"
using static System.IO.FileOptions;

namespace ConsoleApplication1
{
    class MyClass
    {
        void Method()
        {
            var e = DeleteOnClose;
            switch (e)
            {
                case Encrypted:
                    throw new System.NotImplementedException();
                case SequentialScan:
                    throw new System.NotImplementedException();
                case DeleteOnClose:
                    throw new System.NotImplementedException();
                case RandomAccess:
                    throw new System.NotImplementedException();
                case Asynchronous:
                    throw new System.NotImplementedException();
                case WriteThrough:
                    throw new System.NotImplementedException();
                case None:
                    throw new System.NotImplementedException();
            }
        }
    }
}";

        await VerifyDiagnostic(original, SwitchDoesNotHandleAllEnumOptionsAnalyzer.Rule.MessageFormat.ToString());
        await VerifyFix(original, result);
    }
}