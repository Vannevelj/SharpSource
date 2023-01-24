using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.OnPropertyChangedWithoutNameOfOperatorAnalyzer, SharpSource.Diagnostics.OnPropertyChangedWithoutNameOfOperatorCodeFix>;

namespace SharpSource.Test;

[TestClass]
public class OnPropertyChangedWithoutNameOfOperatorTests
{
    [TestMethod]
    public async Task OnPropertyChangedWithoutNameOfOperator_WithIdenticalString()
    {
        var original = @"
using System;
using System.ComponentModel;

namespace ConsoleApplication1
{
    class MyClass : INotifyPropertyChanged
    {
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged({|#0:""IsEnabled""|});
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

        var expected = @"
using System;
using System.ComponentModel;

namespace ConsoleApplication1
{
    class MyClass : INotifyPropertyChanged
    {
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("OnPropertyChanged(IsEnabled) can use the nameof() operator."), expected);
    }

    [TestMethod]
    public async Task OnPropertyChangedWithoutNameOfOperator_WithDifferentlyCasedString()
    {
        var original = @"
using System;
using System.ComponentModel;

namespace ConsoleApplication1
{
    class MyClass : INotifyPropertyChanged
    {
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged({|#0:""iSeNabled""|});
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

        var expected = @"
using System;
using System.ComponentModel;

namespace ConsoleApplication1
{
    class MyClass : INotifyPropertyChanged
    {
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("OnPropertyChanged(IsEnabled) can use the nameof() operator."), expected);
    }

    [TestMethod]
    public async Task OnPropertyChangedWithoutNameOfOperator_WithDifferentStringAndNoCorrespondingProperty()
    {
        var original = @"
using System;
using System.ComponentModel;

namespace ConsoleApplication1
{
    class MyClass : INotifyPropertyChanged
    {
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged(""SomethingElse"");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task OnPropertyChangedWithoutNameOfOperator_WithDifferentStringAndCorrespondingProperty()
    {
        var original = @"
using System;
using System.ComponentModel;

namespace ConsoleApplication1
{
    class MyClass : INotifyPropertyChanged
    {
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged({|#0:""IsAnotherBoolean""|});
            }
        }

        private bool _anotherBoolean;
        public bool IsAnotherBoolean
        {
            get { return _anotherBoolean; }
            set
            {
                _anotherBoolean = value;
                OnPropertyChanged(nameof(IsAnotherBoolean));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

        var expected = @"
using System;
using System.ComponentModel;

namespace ConsoleApplication1
{
    class MyClass : INotifyPropertyChanged
    {
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsAnotherBoolean));
            }
        }

        private bool _anotherBoolean;
        public bool IsAnotherBoolean
        {
            get { return _anotherBoolean; }
            set
            {
                _anotherBoolean = value;
                OnPropertyChanged(nameof(IsAnotherBoolean));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("OnPropertyChanged(IsAnotherBoolean) can use the nameof() operator."), expected);
    }

    [TestMethod]
    public async Task OnPropertyChangedWithoutNameOfOperator_WithNameOfOperator()
    {
        var original = @"
using System;
using System.ComponentModel;

namespace ConsoleApplication1
{
    class MyClass : INotifyPropertyChanged
    {
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task OnPropertyChangedWithoutNameOfOperator_WithMultipleArguments()
    {
        var original = @"
using System;
using System.ComponentModel;

namespace ConsoleApplication1
{
    class MyClass : INotifyPropertyChanged
    {
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged({|#0:""IsEnabled""|}, true);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName, bool someBoolean)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

        var expected = @"
using System;
using System.ComponentModel;

namespace ConsoleApplication1
{
    class MyClass : INotifyPropertyChanged
    {
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled), true);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName, bool someBoolean)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("OnPropertyChanged(IsEnabled) can use the nameof() operator."), expected);
    }

    [TestMethod]
    public async Task OnPropertyChangedWithoutNameOfOperator_WithPartialClass()
    {
        var original = @"
using System;
using System.ComponentModel;

namespace ConsoleApplication1
{
    partial class MyClass : INotifyPropertyChanged
    {
	    private bool _isEnabled;
	    public bool IsEnabled
	    {
		    get { return _isEnabled; }
		    set
		    {
			    _isEnabled = value;
			    OnPropertyChanged({|#0:""IsEnabled""|});
            }
        }
    }

    partial class MyClass
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

        var expected = @"
using System;
using System.ComponentModel;

namespace ConsoleApplication1
{
    partial class MyClass : INotifyPropertyChanged
    {
	    private bool _isEnabled;
	    public bool IsEnabled
	    {
		    get { return _isEnabled; }
		    set
		    {
			    _isEnabled = value;
			    OnPropertyChanged(nameof(IsEnabled));
            }
        }
    }

    partial class MyClass
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("OnPropertyChanged(IsEnabled) can use the nameof() operator."), expected);
    }

    [TestMethod]
    public async Task OnPropertyChangedWithoutNameOfOperator_ParenthesizedExpression()
    {
        var original = @"
using System;
using System.ComponentModel;

namespace ConsoleApplication1
{
    class MyClass : INotifyPropertyChanged
    {
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged(({|#0:""IsEnabled""|}));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

        var expected = @"
using System;
using System.ComponentModel;

namespace ConsoleApplication1
{
    class MyClass : INotifyPropertyChanged
    {
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged((nameof(IsEnabled)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("OnPropertyChanged(IsEnabled) can use the nameof() operator."), expected);
    }

    [TestMethod]
    public async Task OnPropertyChangedWithoutNameOfOperator_WithPartialClass_AndDifferentProperty()
    {
        var original = @"
using System;
using System.ComponentModel;

namespace ConsoleApplication1
{
    partial class MyClass : INotifyPropertyChanged
    {
	    private bool _isEnabled;
	    public bool IsEnabled
	    {
		    get { return _isEnabled; }
		    set
		    {
			    _isEnabled = value;
			    OnPropertyChanged({|#0:""OtherBoolean""|});
            }
        }
    }

    partial class MyClass
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool OtherBoolean { get; set; }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

        var expected = @"
using System;
using System.ComponentModel;

namespace ConsoleApplication1
{
    partial class MyClass : INotifyPropertyChanged
    {
	    private bool _isEnabled;
	    public bool IsEnabled
	    {
		    get { return _isEnabled; }
		    set
		    {
			    _isEnabled = value;
			    OnPropertyChanged(nameof(OtherBoolean));
            }
        }
    }

    partial class MyClass
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool OtherBoolean { get; set; }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";

        await VerifyCS.VerifyCodeFix(original, VerifyCS.Diagnostic().WithMessage("OnPropertyChanged(OtherBoolean) can use the nameof() operator."), expected);
    }

    [TestMethod]
    public async Task OnPropertyChangedWithoutNameOfOperator_ParenthesizedExpression_WithNameof()
    {
        var original = @"
using System;
using System.ComponentModel;

namespace ConsoleApplication1
{
    class MyClass : INotifyPropertyChanged
    {
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged(((nameof(IsEnabled))));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}";
        await VerifyCS.VerifyNoDiagnostic(original);
    }
}