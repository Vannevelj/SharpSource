using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.MultipleOrderByCallsAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class MultipleOrderByCallsTests
{
    [TestMethod]
    public async Task MultipleOrderByCalls()
    {
        var original = @"
using System.Collections.Generic;
using System.Linq;

var data = new List<Data>();
var ordered = {|#0:data.OrderBy(obj => obj.X).OrderBy(obj => obj.Y)|};
record Data(int X, int Y);";

//        var result = @"
//using System.Collections.Generic;
//using System.Linq;

//var data = new List<Data>();
//var ordered = data.OrderBy(obj => obj.X).ThenBy(obj => obj.Y);
//record Data(int X, int Y);";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Successive OrderBy() calls will maintain only the last specified sort order"));
        //await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Successive OrderBy() calls will maintain only the last specified sort order"), result);
    }

    [TestMethod]
    public async Task MultipleOrderByCalls_EvenMore()
    {
        var original = @"
using System.Collections.Generic;
using System.Linq;

var data = new List<Data>();
var ordered = {|#0:data.OrderBy(obj => obj.X).OrderBy(obj => obj.Y).OrderBy(obj => obj.Y)|};
record Data(int X, int Y);";

//        var result = @"
//using System.Collections.Generic;
//using System.Linq;

//var data = new List<Data>();
//var ordered = data.OrderBy(obj => obj.X).ThenBy(obj => obj.Y).ThenBy(obj => obj.Y);
//record Data(int X, int Y);";

        await VerifyCS.VerifyDiagnosticWithoutFix(original,
            VerifyCS.Diagnostic().WithNoLocation().WithMessage("Successive OrderBy() calls will maintain only the last specified sort order").WithSpan(6, 15, 6, 63),
            VerifyCS.Diagnostic().WithNoLocation().WithMessage("Successive OrderBy() calls will maintain only the last specified sort order").WithSpan(6, 15, 6, 85)
        );
        //await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Successive OrderBy() calls will maintain only the last specified sort order"), result);
    }

    [TestMethod]
    public async Task MultipleOrderByCalls_JustOne()
    {
        var original = @"
using System.Collections.Generic;
using System.Linq;

var data = new List<Data>();
var ordered = data.OrderBy(obj => obj.X);
record Data(int X, int Y);";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task MultipleOrderByCalls_ThenBy()
    {
        var original = @"
using System.Collections.Generic;
using System.Linq;

var data = new List<Data>();
var ordered = data.OrderBy(obj => obj.X).ThenBy(obj => obj.Y);
record Data(int X, int Y);";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}