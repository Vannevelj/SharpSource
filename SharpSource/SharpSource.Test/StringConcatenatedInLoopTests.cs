using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Test.Helpers;
using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.StringConcatenatedInLoopAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class StringConcatenatedInLoopTests
{
    [TestMethod]
    public async Task StringConcatenatedInLoopTests_ForEach()
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
    public async Task StringConcatenatedInLoopTests_For()
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
    public async Task StringConcatenatedInLoopTests_While()
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
    public async Task StringConcatenatedInLoopTests_DoWhile()
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
    public async Task StringConcatenatedInLoopTests_ScopedInsideLoop()
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
    public async Task StringConcatenatedInLoopTests_OutsideLoop()
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
    public async Task StringConcatenatedInLoopTests_ReferencesProperty()
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
    public async Task StringConcatenatedInLoopTests_NonCompoundOperator()
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
    public async Task StringConcatenatedInLoopTests_NonStringType()
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
    public async Task StringConcatenatedInLoopTests_NoBodyBraces()
    {
        var original = @"
var res = string.Empty;
while (true)
    {|#0:res += ""test""|};
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("A string was concatenated in a loop which introduces intermediate allocations. Consider using a StringBuilder or pre-allocated string instead."));
    }

    [BugVerificationTest(IssueUrl = "https://github.com/Vannevelj/SharpSource/issues/290")]
    public async Task StringConcatenatedInLoopTests_NoConcatenation()
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
    public async Task StringConcatenatedInLoopTests_PropertyAssignment()
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
    public async Task StringConcatenatedInLoopTests_AssignmentAndConcatenationSeparated()
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
}