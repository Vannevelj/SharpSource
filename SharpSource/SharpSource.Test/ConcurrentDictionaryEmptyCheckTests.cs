using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Test.Helpers;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.ConcurrentDictionaryEmptyCheckAnalyzer, SharpSource.Diagnostics.ConcurrentDictionaryEmptyCheckCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class ConcurrentDictionaryEmptyCheckTests
{
    [TestMethod]
    public async Task ConcurrentDictionaryEmptyCheck_IsEmpty()
    {
        var original = @"
using System;
using System.Collections.Concurrent;

var dic = new ConcurrentDictionary<int, int>();
var empty = dic.IsEmpty;
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ConcurrentDictionaryEmptyCheck_Count_NotZero()
    {
        var original = @"
using System;
using System.Collections.Concurrent;

var dic = new ConcurrentDictionary<int, int>();
var hasOne = dic.Count == 1;
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ConcurrentDictionaryEmptyCheck_Count_EqualsZero()
    {
        var original = @"
using System;
using System.Collections.Concurrent;

var dic = new ConcurrentDictionary<int, int>();
var empty = {|#0:dic.Count == 0|};
";

        var result = @"
using System;
using System.Collections.Concurrent;

var dic = new ConcurrentDictionary<int, int>();
var empty = dic.IsEmpty;
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use ConcurrentDictionary.IsEmpty to check for emptiness without locking the entire dictionary"), result);
    }

    [TestMethod]
    public async Task ConcurrentDictionaryEmptyCheck_Count_LeftOperand()
    {
        var original = @"
using System;
using System.Collections.Concurrent;

var dic = new ConcurrentDictionary<int, int>();
var empty = {|#0:0 == dic.Count|};
";

        var result = @"
using System;
using System.Collections.Concurrent;

var dic = new ConcurrentDictionary<int, int>();
var empty = dic.IsEmpty;
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use ConcurrentDictionary.IsEmpty to check for emptiness without locking the entire dictionary"), result);
    }

    [TestMethod]
    public async Task ConcurrentDictionaryEmptyCheck_Count_NotEqualsZero()
    {
        var original = @"
using System;
using System.Collections.Concurrent;

var dic = new ConcurrentDictionary<int, int>();
var empty = {|#0:dic.Count != 0|};
";

        var result = @"
using System;
using System.Collections.Concurrent;

var dic = new ConcurrentDictionary<int, int>();
var empty = !dic.IsEmpty;
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use ConcurrentDictionary.IsEmpty to check for emptiness without locking the entire dictionary"), result);
    }

    [TestMethod]
    public async Task ConcurrentDictionaryEmptyCheck_Count_GreaterThanZero()
    {
        var original = @"
using System;
using System.Collections.Concurrent;

var dic = new ConcurrentDictionary<int, int>();
var empty = {|#0:dic.Count > 0|};
";

        var result = @"
using System;
using System.Collections.Concurrent;

var dic = new ConcurrentDictionary<int, int>();
var empty = !dic.IsEmpty;
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use ConcurrentDictionary.IsEmpty to check for emptiness without locking the entire dictionary"), result);
    }

    [TestMethod]
    public async Task ConcurrentDictionaryEmptyCheck_Count_LessThan()
    {
        var original = @"
using System;
using System.Collections.Concurrent;

var dic = new ConcurrentDictionary<int, int>();
var empty = {|#0:0 < dic.Count|};
";

        var result = @"
using System;
using System.Collections.Concurrent;

var dic = new ConcurrentDictionary<int, int>();
var empty = !dic.IsEmpty;
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use ConcurrentDictionary.IsEmpty to check for emptiness without locking the entire dictionary"), result);
    }

    [TestMethod]
    public async Task ConcurrentDictionaryEmptyCheck_CountLinq_EqualsZero()
    {
        var original = @"
using System;
using System.Collections.Concurrent;
using System.Linq;

var dic = new ConcurrentDictionary<int, int>();
var empty = {|#0:dic.Count() == 0|};
";

        var result = @"
using System;
using System.Collections.Concurrent;
using System.Linq;

var dic = new ConcurrentDictionary<int, int>();
var empty = dic.IsEmpty;
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use ConcurrentDictionary.IsEmpty to check for emptiness without locking the entire dictionary"), result);
    }

    [TestMethod]
    public async Task ConcurrentDictionaryEmptyCheck_CountLinq_NotEqualsZero()
    {
        var original = @"
using System;
using System.Collections.Concurrent;
using System.Linq;

var dic = new ConcurrentDictionary<int, int>();
var empty = {|#0:dic.Count() != 0|};
";

        var result = @"
using System;
using System.Collections.Concurrent;
using System.Linq;

var dic = new ConcurrentDictionary<int, int>();
var empty = !dic.IsEmpty;
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use ConcurrentDictionary.IsEmpty to check for emptiness without locking the entire dictionary"), result);
    }

    [TestMethod]
    public async Task ConcurrentDictionaryEmptyCheck_CountLinq_GreaterThanZero()
    {
        var original = @"
using System;
using System.Collections.Concurrent;
using System.Linq;

var dic = new ConcurrentDictionary<int, int>();
var empty = {|#0:dic.Count() > 0|};
";

        var result = @"
using System;
using System.Collections.Concurrent;
using System.Linq;

var dic = new ConcurrentDictionary<int, int>();
var empty = !dic.IsEmpty;
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use ConcurrentDictionary.IsEmpty to check for emptiness without locking the entire dictionary"), result);
    }

    [TestMethod]
    public async Task ConcurrentDictionaryEmptyCheck_Any()
    {
        var original = @"
using System;
using System.Collections.Concurrent;
using System.Linq;

var dic = new ConcurrentDictionary<int, int>();
var notEmpty = {|#0:dic.Any()|};
";

        var result = @"
using System;
using System.Collections.Concurrent;
using System.Linq;

var dic = new ConcurrentDictionary<int, int>();
var notEmpty = !dic.IsEmpty;
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use ConcurrentDictionary.IsEmpty to check for emptiness without locking the entire dictionary"), result);
    }

    [TestMethod]
    public async Task ConcurrentDictionaryEmptyCheck_Count_AsChainedExpression()
    {
        var original = @"
using System;
using System.Collections.Concurrent;

var empty = {|#0:new Wrapper().GetDic().Count == 0|};

public class Wrapper
{
    public ConcurrentDictionary<int, int> GetDic() => new();
}
";

        var result = @"
using System;
using System.Collections.Concurrent;

var empty = new Wrapper().GetDic().IsEmpty;

public class Wrapper
{
    public ConcurrentDictionary<int, int> GetDic() => new();
}
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("Use ConcurrentDictionary.IsEmpty to check for emptiness without locking the entire dictionary"), result);
    }
}