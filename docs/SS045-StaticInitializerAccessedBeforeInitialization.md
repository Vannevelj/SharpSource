# SS045 - StaticInitializerAccessedBeforeInitialization

[![Generic badge](https://img.shields.io/badge/Severity-Error-red.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

A `static` field relies on the value of another `static` field which is defined in the same type. `static` fields are initialized in order of appearance.

---

## Violation
```cs
class Test
{
	public static int FirstField = SecondField;
	private static int SecondField = 5;
}
```