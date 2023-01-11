using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = SharpSource.Test.CSharpCodeFixVerifier<SharpSource.Diagnostics.NewtonsoftMixedWithSystemTextJsonAnalyzer, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace SharpSource.Test;

[TestClass]
public class NewtonsoftMixedWithSystemTextJsonTests
{
    [TestMethod]
    public async Task NewtonsoftMixedWithSystemTextJson_Serialize_SystemText_ThroughNewtonsoft()
    {
        var original = @"
using System.Text.Json.Serialization;
using Newtonsoft.Json;

var data = {|#0:Newtonsoft.Json.JsonConvert.SerializeObject(new MyData())|};

class MyData
{
    [JsonPropertyName(""prop"")]
    public int MyProp { get; set; }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Attempting to serialize an object annotated with System.Text.Json.Serialization through Newtonsoft.Json"));
    }

    [TestMethod]
    public async Task NewtonsoftMixedWithSystemTextJson_Serialize_Newtonsoft_ThroughNewtonsoft()
    {
        var original = @"
using System.Text.Json.Serialization;
using Newtonsoft.Json;

var data = Newtonsoft.Json.JsonConvert.SerializeObject(new MyData());

class MyData
{
    [JsonProperty(""prop"")]
    public int MyProp { get; set; }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task NewtonsoftMixedWithSystemTextJson_Serialize_Newtonsoft_ThroughSystemText()
    {
        var original = @"
using System.Text.Json.Serialization;
using Newtonsoft.Json;

var data = {|#0:System.Text.Json.JsonSerializer.Serialize(new MyData())|};

class MyData
{
    [JsonProperty(""prop"")]
    public int MyProp { get; set; }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Attempting to serialize an object annotated with Newtonsoft.Json through System.Text.Json"));
    }

    [TestMethod]
    public async Task NewtonsoftMixedWithSystemTextJson_Serialize_SystemText_ThroughSystemText()
    {
        var original = @"
using System.Text.Json.Serialization;
using Newtonsoft.Json;

var data = System.Text.Json.JsonSerializer.Serialize(new MyData());

class MyData
{
    [JsonPropertyName(""prop"")]
    public int MyProp { get; set; }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task NewtonsoftMixedWithSystemTextJson_Deserialize_SystemText_ThroughNewtonsoft()
    {
        var original = @"
using System.Text.Json.Serialization;
using Newtonsoft.Json;

var data = {|#0:Newtonsoft.Json.JsonConvert.DeserializeObject<MyData>(string.Empty)|};

class MyData
{
    [JsonPropertyName(""prop"")]
    public int MyProp { get; set; }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Attempting to deserialize an object annotated with System.Text.Json.Serialization through Newtonsoft.Json"));
    }

    [TestMethod]
    public async Task NewtonsoftMixedWithSystemTextJson_Deserialize_Newtonsoft_ThroughNewtonsoft()
    {
        var original = @"
using System.Text.Json.Serialization;
using Newtonsoft.Json;

var data = Newtonsoft.Json.JsonConvert.DeserializeObject<MyData>(string.Empty);

class MyData
{
    [JsonProperty(""prop"")]
    public int MyProp { get; set; }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task NewtonsoftMixedWithSystemTextJson_Deserialize_Newtonsoft_ThroughSystemText()
    {
        var original = @"
using System.Text.Json.Serialization;
using Newtonsoft.Json;

var data = {|#0:System.Text.Json.JsonSerializer.Deserialize<MyData>(string.Empty)|};

class MyData
{
    [JsonProperty(""prop"")]
    public int MyProp { get; set; }
}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Attempting to deserialize an object annotated with Newtonsoft.Json through System.Text.Json"));
    }

    [TestMethod]
    public async Task NewtonsoftMixedWithSystemTextJson_Deserialize_SystemText_ThroughSystemText()
    {
        var original = @"
using System.Text.Json.Serialization;
using Newtonsoft.Json;

var data = System.Text.Json.JsonSerializer.Deserialize<MyData>(string.Empty);

class MyData
{
    [JsonPropertyName(""prop"")]
    public int MyProp { get; set; }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    [DataRow("JsonPropertyName")]
    [DataRow("System.Text.Json.Serialization.JsonPropertyName")]
    [DataRow("JsonPropertyNameAttribute")]
    [DataRow("System.Text.Json.Serialization.JsonPropertyNameAttribute")]
    public async Task NewtonsoftMixedWithSystemTextJson_DifferentAttributeNotations(string attribute)
    {
        var original = $@"
using System.Text.Json.Serialization;
using Newtonsoft.Json;

var data = {{|#0:Newtonsoft.Json.JsonConvert.SerializeObject(new MyData())|}};

class MyData
{{
    [{attribute}(""prop"")]
    public int MyProp {{ get; set; }}
}}";

        await VerifyCS.VerifyDiagnosticWithoutFix(original, VerifyCS.Diagnostic().WithMessage("Attempting to serialize an object annotated with System.Text.Json.Serialization through Newtonsoft.Json"));
    }

    [TestMethod]
    public async Task NewtonsoftMixedWithSystemTextJson_Serialize_SystemText()
    {
        var original = @"
using System.Text.Json.Serialization;

var data = System.Text.Json.JsonSerializer.Deserialize<MyData>(string.Empty);

class MyData
{
    [JsonPropertyName(""prop"")]
    public int MyProp { get; set; }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }

    [TestMethod]
    public async Task NewtonsoftMixedWithSystemTextJson_Serialize_Newtonsoft()
    {
        var original = @"
using Newtonsoft.Json;

var data = Newtonsoft.Json.JsonConvert.SerializeObject(new MyData());

class MyData
{
    [JsonProperty(""prop"")]
    public int MyProp { get; set; }
}";

        await VerifyCS.VerifyNoDiagnostic(original);
    }
}