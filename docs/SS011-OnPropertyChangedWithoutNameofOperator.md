# SS011 - OnPropertyChangedWithoutNameofOperator

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-Yes-green.svg)](https://shields.io/)

---

Use the `nameof()` operator in conjunction with `OnPropertyChanged()` to avoid divergence.

---

## Violation
```cs
class MyClass : INotifyPropertyChanged
{
    private bool _isEnabled;
    public bool IsEnabled
    {
        get { return _isEnabled; }
        set
        {
            _isEnabled = value;
            OnPropertyChanged(""IsEnabled"");
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
```

## Fix
```cs
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
```