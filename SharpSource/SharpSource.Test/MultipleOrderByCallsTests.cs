using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.MultipleOrderByCallsAnalyzer, SharpSource.Diagnostics.MultipleOrderByCallsCodeFix>;

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

        var result = @"
using System.Collections.Generic;
using System.Linq;

var data = new List<Data>();
var ordered = data.OrderBy(obj => obj.X).ThenBy(obj => obj.Y);
record Data(int X, int Y);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Successive OrderBy() calls will maintain only the last specified sort order"), result);
    }

    [TestMethod]
    public async Task MultipleOrderByCalls_Descending()
    {
        var original = @"
using System.Collections.Generic;
using System.Linq;

var data = new List<Data>();
var ordered = {|#0:data.OrderBy(obj => obj.X).OrderByDescending(obj => obj.Y)|};
record Data(int X, int Y);";

        var result = @"
using System.Collections.Generic;
using System.Linq;

var data = new List<Data>();
var ordered = data.OrderBy(obj => obj.X).ThenByDescending(obj => obj.Y);
record Data(int X, int Y);";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Successive OrderBy() calls will maintain only the last specified sort order"), result);
    }

    [TestMethod]
    [Ignore("Fix all provider keeps complaining")]
    public async Task MultipleOrderByCalls_MultipleOrderByChained()
    {
        var original = @"
using System.Collections.Generic;
using System.Linq;

var data = new List<Data>();
var ordered = {|#0:data.OrderBy(obj => obj.X).OrderBy(obj => obj.Y).OrderBy(obj => obj.Y)|};
record Data(int X, int Y);";

        var result = @"
using System.Collections.Generic;
using System.Linq;

var data = new List<Data>();
var ordered = data.OrderBy(obj => obj.X).ThenBy(obj => obj.Y).OrderBy(obj => obj.Y);
record Data(int X, int Y);";

        await VerifyCS.VerifyCodeFix(original, new DiagnosticResult[] {
            VerifyCS.Diagnostic().WithNoLocation().WithMessage("Successive OrderBy() calls will maintain only the last specified sort order").WithSpan(6, 15, 6, 63),
            VerifyCS.Diagnostic().WithNoLocation().WithMessage("Successive OrderBy() calls will maintain only the last specified sort order").WithSpan(6, 15, 6, 85)
        }, result);
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