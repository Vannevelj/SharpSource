using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Test.Helpers;
using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.ImmutableCollectionCreatedIncorrectlyAnalyzer, SharpSource.Diagnostics.ImmutableCollectionCreatedIncorrectlyCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class ImmutableCollectionCreatedIncorrectlyTests
{
    [TestMethod]
    public async Task ImmutableCollectionCreatedIncorrectly_UsingNew_WithoutArguments()
    {
        var original = @"
using System.Collections.Immutable;

class Test
{
    void Method()
    {
        var array = {|#0:new ImmutableArray<int>()|};
    }
}
";

        var expected = @"
using System.Collections.Immutable;

class Test
{
    void Method()
    {
        var array = ImmutableArray.Create<int>();
    }
}
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("ImmutableArray should be created using ImmutableArray.Create<int>() instead of new ImmutableArray<int>()"), expected);
    }

    [TestMethod]
    public async Task ImmutableCollectionCreatedIncorrectly_UsingNew_WithStringType()
    {
        var original = @"
using System.Collections.Immutable;

class Test
{
    void Method()
    {
        var array = {|#0:new ImmutableArray<string>()|};
    }
}
";

        var expected = @"
using System.Collections.Immutable;

class Test
{
    void Method()
    {
        var array = ImmutableArray.Create<string>();
    }
}
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("ImmutableArray should be created using ImmutableArray.Create<string>() instead of new ImmutableArray<string>()"), expected);
    }

    [TestMethod]
    public async Task ImmutableCollectionCreatedIncorrectly_UsingNew_WithCustomType()
    {
        var original = @"
using System.Collections.Immutable;

class MyClass { }

class Test
{
    void Method()
    {
        var array = {|#0:new ImmutableArray<MyClass>()|};
    }
}
";

        var expected = @"
using System.Collections.Immutable;

class MyClass { }

class Test
{
    void Method()
    {
        var array = ImmutableArray.Create<MyClass>();
    }
}
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("ImmutableArray should be created using ImmutableArray.Create<MyClass>() instead of new ImmutableArray<MyClass>()"), expected);
    }

    [TestMethod]
    public async Task ImmutableCollectionCreatedIncorrectly_UsingNew_AsFieldInitializer()
    {
        var original = @"
using System.Collections.Immutable;

class Test
{
    private ImmutableArray<int> _array = {|#0:new ImmutableArray<int>()|};
}
";

        var expected = @"
using System.Collections.Immutable;

class Test
{
    private ImmutableArray<int> _array = ImmutableArray.Create<int>();
}
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("ImmutableArray should be created using ImmutableArray.Create<int>() instead of new ImmutableArray<int>()"), expected);
    }

    [TestMethod]
    public async Task ImmutableCollectionCreatedIncorrectly_UsingNew_AsPropertyInitializer()
    {
        var original = @"
using System.Collections.Immutable;

class Test
{
    public ImmutableArray<int> Array { get; set; } = {|#0:new ImmutableArray<int>()|};
}
";

        var expected = @"
using System.Collections.Immutable;

class Test
{
    public ImmutableArray<int> Array { get; set; } = ImmutableArray.Create<int>();
}
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("ImmutableArray should be created using ImmutableArray.Create<int>() instead of new ImmutableArray<int>()"), expected);
    }

    [TestMethod]
    public async Task ImmutableCollectionCreatedIncorrectly_UsingNew_AsReturnValue()
    {
        var original = @"
using System.Collections.Immutable;

class Test
{
    ImmutableArray<int> Method()
    {
        return {|#0:new ImmutableArray<int>()|};
    }
}
";

        var expected = @"
using System.Collections.Immutable;

class Test
{
    ImmutableArray<int> Method()
    {
        return ImmutableArray.Create<int>();
    }
}
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("ImmutableArray should be created using ImmutableArray.Create<int>() instead of new ImmutableArray<int>()"), expected);
    }

    [TestMethod]
    public async Task ImmutableCollectionCreatedIncorrectly_UsingNew_WithExplicitType()
    {
        var original = @"
using System.Collections.Immutable;

class Test
{
    void Method()
    {
        ImmutableArray<int> array = {|#0:new ImmutableArray<int>()|};
    }
}
";

        var expected = @"
using System.Collections.Immutable;

class Test
{
    void Method()
    {
        ImmutableArray<int> array = ImmutableArray.Create<int>();
    }
}
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("ImmutableArray should be created using ImmutableArray.Create<int>() instead of new ImmutableArray<int>()"), expected);
    }

    [TestMethod]
    public async Task ImmutableCollectionCreatedIncorrectly_UsingNew_WithNestedGenericType()
    {
        var original = @"
using System.Collections.Generic;
using System.Collections.Immutable;

class Test
{
    void Method()
    {
        var array = {|#0:new ImmutableArray<List<int>>()|};
    }
}
";

        var expected = @"
using System.Collections.Generic;
using System.Collections.Immutable;

class Test
{
    void Method()
    {
        var array = ImmutableArray.Create<List<int>>();
    }
}
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("ImmutableArray should be created using ImmutableArray.Create<System.Collections.Generic.List<int>>() instead of new ImmutableArray<System.Collections.Generic.List<int>>()"), expected);
    }

    [TestMethod]
    public async Task ImmutableCollectionCreatedIncorrectly_UsingCreateMethod_NoDiagnostic()
    {
        var original = @"
using System.Collections.Immutable;

class Test
{
    void Method()
    {
        var array = ImmutableArray.Create<int>();
    }
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ImmutableCollectionCreatedIncorrectly_UsingCreateMethodWithArguments_NoDiagnostic()
    {
        var original = @"
using System.Collections.Immutable;

class Test
{
    void Method()
    {
        var array = ImmutableArray.Create(1, 2, 3);
    }
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ImmutableCollectionCreatedIncorrectly_UsingDefaultExpression_NoDiagnostic()
    {
        var original = @"
using System.Collections.Immutable;

class Test
{
    void Method()
    {
        var array = default(ImmutableArray<int>);
    }
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ImmutableCollectionCreatedIncorrectly_UsingDefaultKeyword_NoDiagnostic()
    {
        var original = @"
using System.Collections.Immutable;

class Test
{
    void Method()
    {
        ImmutableArray<int> array = default;
    }
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ImmutableCollectionCreatedIncorrectly_RegularArray_NoDiagnostic()
    {
        var original = @"
class Test
{
    void Method()
    {
        var array = new int[0];
    }
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ImmutableCollectionCreatedIncorrectly_ListCreation_NoDiagnostic()
    {
        var original = @"
using System.Collections.Generic;

class Test
{
    void Method()
    {
        var list = new List<int>();
    }
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ImmutableCollectionCreatedIncorrectly_ImmutableListCreation_NoDiagnostic()
    {
        var original = @"
using System.Collections.Immutable;

class Test
{
    void Method()
    {
        var list = new ImmutableList<int>();
    }
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ImmutableCollectionCreatedIncorrectly_WithoutImmutableCollectionsReference_NoDiagnostic()
    {
        var original = @"
class Test
{
    void Method()
    {
        var x = 5;
    }
}
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ImmutableCollectionCreatedIncorrectly_UsingNew_InLambda()
    {
        var original = @"
using System;
using System.Collections.Immutable;

class Test
{
    void Method()
    {
        Func<ImmutableArray<int>> factory = () => {|#0:new ImmutableArray<int>()|};
    }
}
";

        var expected = @"
using System;
using System.Collections.Immutable;

class Test
{
    void Method()
    {
        Func<ImmutableArray<int>> factory = () => ImmutableArray.Create<int>();
    }
}
";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("ImmutableArray should be created using ImmutableArray.Create<int>() instead of new ImmutableArray<int>()"), expected);
    }

    [TestMethod]
    public async Task ImmutableCollectionCreatedIncorrectly_UsingNew_AsArrayElement()
    {
        var original = @"
using System.Collections.Immutable;

class Test
{
    void Method()
    {
        var arrays = new[] { {|#0:new ImmutableArray<int>()|}, {|#1:new ImmutableArray<int>()|} };
    }
}
";

        var expected = @"
using System.Collections.Immutable;

class Test
{
    void Method()
    {
        var arrays = new[] { ImmutableArray.Create<int>(), ImmutableArray.Create<int>() };
    }
}
";

        await VerifyCS.VerifyCodeFix(original, new[]
        {
            VerifyCS.Diagnostic().WithLocation(0).WithMessage("ImmutableArray should be created using ImmutableArray.Create<int>() instead of new ImmutableArray<int>()"),
            VerifyCS.Diagnostic().WithLocation(1).WithMessage("ImmutableArray should be created using ImmutableArray.Create<int>() instead of new ImmutableArray<int>()")
        }, expected);
    }
}