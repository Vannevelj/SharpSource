using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.DisposableFieldIsNotDisposedAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class DisposableFieldIsNotDisposedTests
{
    [TestMethod]
    public async Task DisposableFieldIsNotDisposed_DisposeMethodDoesNotReferenceField()
    {
        var original = @"
using System;
using System.IO;

class Test : IDisposable
{
    private readonly MemoryStream {|#0:_stream|} = new();

    public void Dispose()
    {
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Disposable field _stream in type Test is not disposed"));
    }

    [TestMethod]
    public async Task DisposableFieldIsNotDisposed_DisposeMethodReferencesField()
    {
        var original = @"
using System;
using System.IO;

class Test : IDisposable
{
    private readonly MemoryStream _stream = new();

    public void Dispose()
    {
        _stream.Dispose();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DisposableFieldIsNotDisposed_FieldDisposedThroughDisposeBool()
    {
        var original = @"
using System;
using System.IO;

class Test : IDisposable
{
    private readonly MemoryStream _stream = new();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _stream.Dispose();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DisposableFieldIsNotDisposed_FieldDisposedThroughReachableHelperMethod()
    {
        var original = @"
using System;
using System.IO;

class Test : IDisposable
{
    private readonly MemoryStream _stream = new();

    public void Dispose()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        _stream.Dispose();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DisposableFieldIsNotDisposed_FieldDisposedThroughPropertyGetter()
    {
        var original = @"
using System;
using System.IO;

class Test : IDisposable
{
    private readonly MemoryStream _stream = new();

    private MemoryStream Stream => _stream;

    public void Dispose()
    {
        Stream.Dispose();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DisposableFieldIsNotDisposed_FieldDisposedThroughLocalFunction()
    {
        var original = @"
using System;
using System.IO;

class Test : IDisposable
{
    private readonly MemoryStream _stream = new();

    public void Dispose()
    {
        Cleanup();

        void Cleanup()
        {
            _stream.Dispose();
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DisposableFieldIsNotDisposed_DisposeAsyncDoesNotReferenceField()
    {
        var original = @"
using System;
using System.Threading.Tasks;

class AsyncDisposable : IAsyncDisposable
{
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

class Test : IAsyncDisposable
{
    private readonly AsyncDisposable {|#0:_resource|} = new();

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Disposable field _resource in type Test is not disposed"));
    }

    [TestMethod]
    public async Task DisposableFieldIsNotDisposed_DisposeAsyncReferencesField()
    {
        var original = @"
using System;
using System.Threading.Tasks;

class AsyncDisposable : IAsyncDisposable
{
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

class Test : IAsyncDisposable
{
    private readonly AsyncDisposable _resource = new();

    public async ValueTask DisposeAsync()
    {
        await _resource.DisposeAsync();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DisposableFieldIsNotDisposed_ConcreteTypeImplementsDisposableViaBaseButDoesNotOverrideIt()
    {
        var original = @"
using System;
using System.IO;

abstract class Base : IDisposable
{
    public virtual void Dispose()
    {
    }
}

class Derived : Base
{
    private readonly MemoryStream {|#0:_stream|} = new();
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Disposable field _stream in type Derived is not disposed"));
    }

    [TestMethod]
    public async Task DisposableFieldIsNotDisposed_DefaultInterfaceImplementation_DisposesThroughProperty()
    {
        var original = @"
using System;
using System.IO;

interface ITest : IDisposable
{
    MemoryStream Stream { get; }

    void IDisposable.Dispose()
    {
        Stream.Dispose();
    }
}

class Test : ITest
{
    private readonly MemoryStream _stream = new();

    public MemoryStream Stream => _stream;
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DisposableFieldIsNotDisposed_NonDisposableTypeIsIgnored()
    {
        var original = @"
using System.IO;

class Test
{
    private readonly MemoryStream _stream = new();
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DisposableFieldIsNotDisposed_MultipleFieldsOneMissing()
    {
        var original = @"
using System;
using System.IO;

class Test : IDisposable
{
    private readonly MemoryStream _stream = new();
    private readonly StringReader {|#0:_reader|} = new(""value"");

    public void Dispose()
    {
        _stream.Dispose();
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Disposable field _reader in type Test is not disposed"));
    }

    [TestMethod]
    public async Task DisposableFieldIsNotDisposed_StaticFieldIsIgnored()
    {
        var original = @"
using System;
using System.IO;

class Test : IDisposable
{
    private static readonly MemoryStream Shared = new();

    public void Dispose()
    {
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task DisposableFieldIsNotDisposed_FieldBackedPropertyNotDisposed()
    {
        var original = @"
using System;
using System.IO;

class Test : IDisposable
{
    public MemoryStream {|#0:Stream|} { get; private set; } = new();

    public void Dispose()
    {
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Disposable field Stream in type Test is not disposed"));
    }

    [TestMethod]
    public async Task DisposableFieldIsNotDisposed_DefaultInterfaceImplementation_DisposesFieldBackedProperty()
    {
        var original = @"
using System;
using System.IO;

interface ITest : IDisposable
{
    MemoryStream Stream { get; }

    void IDisposable.Dispose()
    {
        Stream.Dispose();
    }
}

class Test : ITest
{
    public MemoryStream Stream { get; private set; } = new();
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}