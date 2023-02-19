using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Test.Helpers;
using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.StringConcatenatedInLoopAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class StringConcatenatedInLoopTests
{
    [TestMethod]
    public async Task StringConcatenatedInLoop_ForEach()
    {
        var original = @"
using System.Linq;

var res = string.Empty;
foreach (var item in Enumerable.Empty<int>())
{
    {|#0:res += ""test""|};
}
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("A string was concatenated in a loop which introduces intermediate allocations. Consider using a StringBuilder or pre-allocated string instead."));
    }

    [TestMethod]
    public async Task StringConcatenatedInLoop_For()
    {
        var original = @"
var res = string.Empty;
for (var i = 0; i < 10; i++)
{
    {|#0:res += ""test""|};
}
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("A string was concatenated in a loop which introduces intermediate allocations. Consider using a StringBuilder or pre-allocated string instead."));
    }

    [TestMethod]
    public async Task StringConcatenatedInLoop_While()
    {
        var original = @"
var res = string.Empty;
while (true)
{
    {|#0:res += ""test""|};
}
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("A string was concatenated in a loop which introduces intermediate allocations. Consider using a StringBuilder or pre-allocated string instead."));
    }

    [TestMethod]
    public async Task StringConcatenatedInLoop_DoWhile()
    {
        var original = @"
var res = string.Empty;
do
{
    {|#0:res += ""test""|};
} while (true);
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("A string was concatenated in a loop which introduces intermediate allocations. Consider using a StringBuilder or pre-allocated string instead."));
    }

    [TestMethod]
    public async Task StringConcatenatedInLoop_ScopedInsideLoop()
    {
        var original = @"
while (true)
{
    var res = string.Empty;
    res += ""test"";
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StringConcatenatedInLoop_OutsideLoop()
    {
        var original = @"
while (true)
{
    
}
var res = string.Empty;
res += ""test"";
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StringConcatenatedInLoop_ReferencesProperty()
    {
        var original = @"
class Test
{
    public string Result { get; set; }

    void Method()
    {
        while (true)
        {
            {|#0:Result += ""test""|};
        }
    }
}
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("A string was concatenated in a loop which introduces intermediate allocations. Consider using a StringBuilder or pre-allocated string instead."));
    }

    [TestMethod]
    public async Task StringConcatenatedInLoop_NonCompoundOperator()
    {
        var original = @"
var res = string.Empty;
while (true)
{
    {|#0:res = res + ""test""|};
}
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("A string was concatenated in a loop which introduces intermediate allocations. Consider using a StringBuilder or pre-allocated string instead."));
    }

    [TestMethod]
    public async Task StringConcatenatedInLoop_NonStringType()
    {
        var original = @"
var res = 0;
while (true)
{
    res += 10;
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StringConcatenatedInLoop_NoBodyBraces()
    {
        var original = @"
var res = string.Empty;
while (true)
    {|#0:res += ""test""|};
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("A string was concatenated in a loop which introduces intermediate allocations. Consider using a StringBuilder or pre-allocated string instead."));
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/290")]
    public async Task StringConcatenatedInLoop_NoConcatenation()
    {
        var original = @"
var res = string.Empty;
while (true)
{
    res = ""hello"";
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/290")]
    public async Task StringConcatenatedInLoop_PropertyAssignment()
    {
        var original = @"
Test res = null;
while (true)
{
    res = new Test { MyProp = ""hello"" };
}

class Test
{
    public string MyProp { get; set; }
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StringConcatenatedInLoop_AssignmentAndConcatenationSeparated()
    {
        var original = @"
var res = string.Empty;
while (true)
{
    {|#0:res = res + ""test""|};
}
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("A string was concatenated in a loop which introduces intermediate allocations. Consider using a StringBuilder or pre-allocated string instead."));
    }

    [TestMethod]
    public async Task StringConcatenatedInLoop_AssignmentAndConcatenationSeparated_Multiple()
    {
        var original = @"
var res = string.Empty;
while (true)
{
    {|#0:res = res + ""test"" + ""other"" + res + ""another""|};
}
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("A string was concatenated in a loop which introduces intermediate allocations. Consider using a StringBuilder or pre-allocated string instead."));
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/292")]
    public async Task StringConcatenatedInLoop_AssignmentInWhileCondition()
    {
        var original = @"
using System;
using System.IO;

using (StreamReader reader = File.OpenText(""file.txt""))
{
    string line;
    while ((line = await reader.ReadLineAsync()) != null)
    {
        Console.Write(line);
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/308")]
    public async Task StringConcatenatedInLoop_ConcatenationInsideObjectCreation()
    {
        var original = @"
for(var i = 0; i < 10; i++)
{
    System.Console.WriteLine(new Test
    {
        SomeProp = ""file_"" + i
    });
}

class Test
{
    public string SomeProp { get; set; }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/309")]
    public async Task StringConcatenatedInLoop_AssignmentToLoopVariable()
    {
        var original = @"
void Method(Test[] tests)
{
    foreach (var test in tests)
    {
        test.Id += ""_"" + ""hello"";
    }
}

class Test
{
    public string Id { get; set; }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StringConcatenatedInLoop_AssignmentToLoopVariable_Field()
    {
        var original = @"
void Method(Test[] tests)
{
    foreach (var test in tests)
    {
        test._id += ""_"" + ""hello"";
    }
}

class Test
{
    public string _id;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StringConcatenatedInLoop_AssignmentToLoopVariable_Nested()
    {
        var original = @"
void Method(Test[] tests)
{
    foreach (var test in tests)
    {
        test.Id.Id += ""_"" + ""hello"";
    }
}

class Test
{
    public TestTwo Id { get; set; } = new();
}

class TestTwo
{
    public string Id { get; set; }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/312")]
    public async Task StringConcatenatedInLoop_ConcatenationWithoutReferencingTarget()
    {
        var original = @"
string s = string.Empty;

while (true)
{
    s = ""test"" + "".txt"";
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StringConcatenatedInLoop_Return()
    {
        var original = @"
var res = string.Empty;
while (true)
{
    {|#0:res += ""test""|};
    return;
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task StringConcatenatedInLoop_Break()
    {
        var original = @"
var res = string.Empty;
while (true)
{
    {|#0:res += ""test""|};
    break;
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}