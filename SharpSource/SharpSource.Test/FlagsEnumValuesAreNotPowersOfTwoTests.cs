using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpSource.Diagnostics;
using SharpSource.Test.Helpers.Helpers;

namespace SharpSource.Test;

[TestClass]
public class FlagsEnumValuesAreNotPowersOfTwoTests : DiagnosticVerifier
{
    protected override DiagnosticAnalyzer DiagnosticAnalyzer => new FlagsEnumValuesAreNotPowersOfTwoAnalyzer();

    protected override CodeFixProvider CodeFixProvider => new FlagsEnumValuesAreNotPowersOfTwoCodeFix();

    [TestMethod]
    public void FlagsEnumValuesAreNotPowersOfTwo_ValuesAreNotPowersOfTwo()
    {
        var original = @"
using System;

[Flags]
enum Foo
{
    Bar = 0,
    Biz = 1,
    Baz = 2,
    Buz = 3,
    Boz = 4
}";

        var result = @"
using System;

[Flags]
enum Foo
{
    Bar = 0,
    Biz = 1,
    Baz = 2,
    Buz = Biz | Baz,
    Boz = 4
}";

        VerifyDiagnostic(original, "Enum Foo.Buz is marked as a [Flags] enum but contains a literal value that isn't a power of two. Change the value or use a bitwise OR expression instead.");
        VerifyFix(original, result);
    }

    [TestMethod]
    [DataRow("int")]
    [DataRow("uint")]
    [DataRow("byte")]
    [DataRow("sbyte")]
    [DataRow("long")]
    [DataRow("ulong")]
    [DataRow("short")]
    [DataRow("ushort")]
    [DataRow("UInt16")]
    [DataRow("UInt32")]
    [DataRow("UInt64")]
    public void FlagsEnumValuesAreNotPowersOfTwo_ValuesAreNotPowersOfTwo_DifferentTypes(string type)
    {
        var original = $@"
using System;

[Flags]
enum Foo : {type}
{{
    Bar = 0,
    Biz = 1,
    Baz = 2,
    Buz = 3,
    Boz = 4
}}";

        var result = $@"
using System;

[Flags]
enum Foo : {type}
{{
    Bar = 0,
    Biz = 1,
    Baz = 2,
    Buz = Biz | Baz,
    Boz = 4
}}";

        VerifyDiagnostic(original, "Enum Foo.Buz is marked as a [Flags] enum but contains a literal value that isn't a power of two. Change the value or use a bitwise OR expression instead.");
        VerifyFix(original, result);
    }

    [TestMethod]
    public void FlagsEnumValuesAreNotPowersOfTwo_ValuesArePowersOfTwo()
    {
        var original = @"
using System;

[Flags]
enum Foo
{
    Bar = 0,
    Biz = 1,
    Baz = 2,
    Buz = 4,
    Boz = 8
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void FlagsEnumValuesAreNotPowersOfTwo_ValuesAreNotPowersOfTwo_HexValues()
    {
        var original = @"
using System;

[Flags]
enum Foo
{
    Bar = 0x0,
    Biz = 0x1,
    Baz = 0x2,
    Buz = 0x3,
    Boz = 0x4
}";

        var result = @"
using System;

[Flags]
enum Foo
{
    Bar = 0x0,
    Biz = 0x1,
    Baz = 0x2,
    Buz = Biz | Baz,
    Boz = 0x4
}";

        VerifyDiagnostic(original, "Enum Foo.Buz is marked as a [Flags] enum but contains a literal value that isn't a power of two. Change the value or use a bitwise OR expression instead.");
        VerifyFix(original, result);
    }

    [TestMethod]
    public void FlagsEnumValuesAreNotPowersOfTwo_ValuesArePowersOfTwo_HexValues()
    {
        var original = @"
using System;

[Flags]
enum Foo
{
    Bar = 0x0,
    Biz = 0x1,
    Baz = 0x2,
    Buz = 0x4,
    Boz = 0x8
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void FlagsEnumValuesAreNotPowersOfTwo_NegativeValues()
    {
        var original = @"
using System;

[Flags]
enum Foo
{
    Biz = -1,
    Baz = -2,
    Buz = -3
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void FlagsEnumValuesAreNotPowersOfTwo_ValuesAreNotPowersOfTwo_NoValues()
    {
        var original = @"
using System;

[Flags]
enum Foo
{
    Bar,
    Biz,
    Baz,
    Buz,
    Boz
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void FlagsEnumValuesAreNotPowersOfTwo_ValuesArePowersOfTwo_NotFlagsEnum()
    {
        var original = @"
enum Foo
{
    Bar = 0,
    Biz = 1,
    Baz = 2,
    Buz = 3,
    Boz = 4
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void FlagsEnumValuesAreNotPowersOfTwo_ValuesArePowersOfTwo_FlagsEnum_WithSystemNamespace()
    {
        var original = @"
[System.Flags]
enum Foo
{
    Bar = 0,
    Biz = 1,
    Baz = 2,
    Buz = 3,
    Boz = 4
}";

        var result = @"
[System.Flags]
enum Foo
{
    Bar = 0,
    Biz = 1,
    Baz = 2,
    Buz = Biz | Baz,
    Boz = 4
}";

        VerifyDiagnostic(original, "Enum Foo.Buz is marked as a [Flags] enum but contains a literal value that isn't a power of two. Change the value or use a bitwise OR expression instead.");
        VerifyFix(original, result);
    }

    [TestMethod]
    public void FlagsEnumValuesAreNotPowersOfTwo_ValuesArePowersOfTwo_BitShifting()
    {
        var original = @"
using System;

[Flags]
enum Foo
{
    Bar = 0,
    Biz = 1 << 0,
    Baz = 1 << 1,
    Buz = 1 << 2,
    Boz = 1 << 3
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void FlagsEnumValuesAreNotPowersOfTwo_ValuesAreNotPowersOfTwo_ValuesOfOtherFlags()
    {
        var original = @"
using System;

[Flags]
enum Days
{
    None = 0,
    Sunday = 1,
    Monday = 1 << 1,
    WorkweekStart = Monday,
    Tuesday = 1 << 2,
    Wednesday = 1 << 3,
    Thursday = 1 << 4,
    Friday = 1 << 5,
    WorkweekEnd = Friday,
    Saturday = 1 << 6,
    Weekend = Saturday | Sunday,
    Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday
}";
        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void FlagsEnumValuesAreNotPowersOfTwo_CharValues()
    {
        var original = @"
using System;

[Flags]
enum Foo
{
    Bar = 'a',
    Biz = 'b',
    Baz = 'c',
    Buz = 'd',
    Boz = 'e'
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    [Ignore("We need to support binary expressions on top of literals")]
    public void FlagsEnumValuesAreNotPowersOfTwo_BinaryExpressions()
    {
        var original = @"
using System;

[Flags]
enum Days
{
    None = 0,
    Sunday = 1,
    Monday = 2,
    WorkweekStart = Monday,
    Tuesday = 3,
    Wednesday = 4,
    Thursday = 5,
    Friday = 6,
    WorkweekEnd = Friday,
    Saturday = 7,
    Weekend = Saturday | Sunday,
    Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday
}";

        var result = @"
using System;

[Flags]
enum Days
{
    None = 0,
    Sunday = 1,
    Monday = 2,
    WorkweekStart = Monday,
    Tuesday = Sunday | Monday,
    Wednesday = 4,
    Thursday = Sunday | Wednesday,
    Friday = Monday | Wednesday,
    WorkweekEnd = Friday,
    Saturday = Sunday | Friday,
    Weekend = Saturday | Sunday,
    Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday
}";

        VerifyDiagnostic(original,
            "Enum Days.Tuesday is marked as a [Flags] enum but contains a literal value that isn't a power of two. Change the value or use a bitwise OR expression instead.",
            "Enum Days.Thursday is marked as a [Flags] enum but contains a literal value that isn't a power of two. Change the value or use a bitwise OR expression instead.",
            "Enum Days.Friday is marked as a [Flags] enum but contains a literal value that isn't a power of two. Change the value or use a bitwise OR expression instead.",
            "Enum Days.Saturday is marked as a [Flags] enum but contains a literal value that isn't a power of two. Change the value or use a bitwise OR expression instead.");
        VerifyFix(original, result);
    }

    [TestMethod]
    public void FlagsEnumValuesAreNotPowersOfTwo_ValuesAreNotPowersOfTwo_BitshiftedValuesNotPowersOfTwo()
    {
        var original = @"
using System;

[Flags]
enum Days
{
    None = 0,
    Sunday = 1,
    Monday = 75 << 1,
    WorkweekStart = Monday,
    Tuesday = 75 << 2,
    Wednesday = 75 << 3,
    Thursday = 75 << 4,
    Friday = 75 << 5,
    WorkweekEnd = Friday,
    Saturday = 75 << 6,
    Weekend = Saturday | Sunday,
    Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void FlagsEnumValuesAreNotPowersOfTwo_ValuesAreNotPowersOfTwo_BitOredValuesNotPowersOfTwo()
    {
        var original = @"
using System;

[Flags]
enum Days
{
    None = 0,
    Sunday = 1,
    Monday = 1 << 1,
    WorkweekStart = Monday,
    Tuesday = 1 << 2,
    Wednesday = 1 << 3,
    Thursday = 1 << 4,
    Friday = 1 << 5,
    WorkweekEnd = Friday | 63,
    Saturday = 1 << 6,
    Weekend = Saturday | Sunday,
    Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void FlagsEnumValuesAreNotPowersOfTwo_WithoutExplicitValues()
    {
        var original = @"
using System;

[Flags]
enum Foo
{
    A,
    B,
    C
}";

        VerifyDiagnostic(original);
    }

    [TestMethod]
    public void FlagsEnumValuesAreNotPowersOfTwo_ValuesArePowersOfTwo_MultipleOptions()
    {
        var original = @"
[System.Flags]
enum Foo
{
    Bar = 0,
    Biz = 1,
    Bip = 2,
    Baz = 2,
    Buz = 4,
    Boz = 8,
    Bop = 10,
}";

        var result = @"
[System.Flags]
enum Foo
{
    Bar = 0,
    Biz = 1,
    Bip = 2,
    Baz = 2,
    Buz = 4,
    Boz = 8,
    Bop = Bip | Boz,
}";

        VerifyDiagnostic(original, "Enum Foo.Bop is marked as a [Flags] enum but contains a literal value that isn't a power of two. Change the value or use a bitwise OR expression instead.");
        VerifyFix(original, result);
    }
}