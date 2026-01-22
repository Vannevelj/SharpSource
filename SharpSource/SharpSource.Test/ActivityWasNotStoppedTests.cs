using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.ActivityWasNotStoppedAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class ActivityWasNotStoppedTests
{
    [TestMethod]
    public async Task ActivityWasNotStopped_StartActivityWithoutStopOrUsing()
    {
        var original = @"
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    void Method()
    {
        var activity = {|#0:Source.StartActivity(""test"")|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Activity activity was started but is not being stopped or disposed"));
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_StartActivityWithUsing()
    {
        var original = @"
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    void Method()
    {
        using var activity = Source.StartActivity(""test"");
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_StartActivityWithUsingStatement()
    {
        var original = @"
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    void Method()
    {
        using (var activity = Source.StartActivity(""test""))
        {
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_StartActivityWithExplicitStop()
    {
        var original = @"
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    void Method()
    {
        var activity = Source.StartActivity(""test"");
        activity?.Stop();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_StartActivityWithExplicitStopNoNullCheck()
    {
        var original = @"
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    void Method()
    {
        var activity = Source.StartActivity(""test"");
        activity.Stop();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_StartActivityWithExplicitDispose()
    {
        var original = @"
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    void Method()
    {
        var activity = Source.StartActivity(""test"");
        activity?.Dispose();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_ActivityReturnedFromMethod()
    {
        var original = @"
#nullable enable
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    Activity? Method()
    {
        var activity = Source.StartActivity(""test"");
        return activity;
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_ActivityPassedToAnotherMethod()
    {
        var original = @"
#nullable enable
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    void Method()
    {
        var activity = Source.StartActivity(""test"");
        ProcessActivity(activity);
    }

    void ProcessActivity(Activity? activity) { }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_ActivityAssignedToField()
    {
        var original = @"
#nullable enable
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");
    private Activity? _activity;

    void Method()
    {
        _activity = Source.StartActivity(""test"");
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_ActivityAssignedToProperty()
    {
        var original = @"
#nullable enable
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");
    public Activity? CurrentActivity { get; set; }

    void Method()
    {
        CurrentActivity = Source.StartActivity(""test"");
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_MultipleActivitiesOneMissing()
    {
        var original = @"
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    void Method()
    {
        using var activity1 = Source.StartActivity(""test1"");
        var activity2 = {|#0:Source.StartActivity(""test2"")|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Activity activity2 was started but is not being stopped or disposed"));
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_ConditionallyStoppedInIfStatement_NotReportedDueToNoControlFlowAnalysis()
    {
        // This test documents that we don't do control flow analysis - if Stop() appears anywhere, we don't warn
        var original = @"
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    void Method(bool condition)
    {
        var activity = Source.StartActivity(""test"");
        if (condition)
        {
            activity?.Stop();
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_StoppedWithinTryFinally()
    {
        var original = @"
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    void Method()
    {
        var activity = Source.StartActivity(""test"");
        try
        {
            // Do work
        }
        finally
        {
            activity?.Stop();
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_ActivityInAsyncMethod()
    {
        var original = @"
using System.Diagnostics;
using System.Threading.Tasks;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    async Task MethodAsync()
    {
        var activity = {|#0:Source.StartActivity(""test"")|};
        await Task.Delay(100);
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Activity activity was started but is not being stopped or disposed"));
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_ActivityInAsyncMethodWithUsing()
    {
        var original = @"
using System.Diagnostics;
using System.Threading.Tasks;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    async Task MethodAsync()
    {
        using var activity = Source.StartActivity(""test"");
        await Task.Delay(100);
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_StartActivityDirectlyWithoutAssignment()
    {
        var original = @"
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    void Method()
    {
        {|#0:Source.StartActivity(""test"")|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Activity  was started but is not being stopped or disposed"));
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_ActivityStoredInOutParameter()
    {
        var original = @"
#nullable enable
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    void Method(out Activity? activity)
    {
        activity = Source.StartActivity(""test"");
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_SelfDefinedActivityClass()
    {
        var original = @"
class ActivitySource
{
    public ActivitySource(string name) { }
    public Activity StartActivity(string name) => new Activity();
}

class Activity { }

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    void Method()
    {
        var activity = Source.StartActivity(""test"");
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_NullCheckWithEarlyReturn()
    {
        var original = @"
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    void Method()
    {
        var activity = Source.StartActivity(""test"");
        if (activity == null)
            return;
        activity.Stop();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_StartActivityWithActivityKind()
    {
        var original = @"
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    void Method()
    {
        var activity = {|#0:Source.StartActivity(""test"", ActivityKind.Server)|};
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Activity activity was started but is not being stopped or disposed"));
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_GlobalStatement()
    {
        var original = @"
using System.Diagnostics;

var source = new ActivitySource(""Test"");
var activity = {|#0:source.StartActivity(""test"")|};
";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Activity activity was started but is not being stopped or disposed"));
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_GlobalStatementWithUsing()
    {
        var original = @"
using System.Diagnostics;

var source = new ActivitySource(""Test"");
using var activity = source.StartActivity(""test"");
";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_LocalFunction()
    {
        var original = @"
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    void Method()
    {
        void LocalMethod()
        {
            var activity = {|#0:Source.StartActivity(""test"")|};
        }
        LocalMethod();
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Activity activity was started but is not being stopped or disposed"));
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_Lambda()
    {
        var original = @"
using System;
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    void Method()
    {
        Action action = () =>
        {
            var activity = {|#0:Source.StartActivity(""test"")|};
        };
        action();
    }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Activity activity was started but is not being stopped or disposed"));
    }

    [TestMethod]
    public async Task ActivityWasNotStopped_SetStatusAndStop()
    {
        var original = @"
using System.Diagnostics;

class Test
{
    private static readonly ActivitySource Source = new(""Test"");

    void Method()
    {
        var activity = Source.StartActivity(""test"");
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.Stop();
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}