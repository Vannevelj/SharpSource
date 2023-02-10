# SS054 - NewtonsoftMixedWithSystemTextJson

[![Generic badge](https://img.shields.io/badge/Severity-Warning-yellow.svg)](https://shields.io/) [![Generic badge](https://img.shields.io/badge/CodeFix-No-lightgrey.svg)](https://shields.io/)

---

An attempt is made to (de-)serialize an object which combines System.Text.Json and Newtonsoft.Json. Attributes from one won't be adhered to in the other and should not be mixed.

---

## Violation
```cs
var data = Newtonsoft.Json.JsonConvert.SerializeObject(new MyData());

class MyData
{
    [System.Text.Json.Serialization.JsonPropertyName("prop")]
    public int MyProp { get; set; }
}
```