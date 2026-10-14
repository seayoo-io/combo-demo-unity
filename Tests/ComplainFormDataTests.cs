using System;
using System.Globalization;
using System.Threading;
using Newtonsoft.Json.Linq;

// Standalone tests compile the production model directly; no Unity or SDK mocks are needed.
internal static class ComplainFormDataTests
{
    private static int assertions;
    private static void Check(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new Exception(message);
    }
    private static ComplainFormData Form() => new ComplainFormData { TargetId = "id", TargetName = "名称" };
    private static ComplainContextEntry Entry(string key, ComplainContextType type, string value) =>
        new ComplainContextEntry { Key = key, Type = type, Value = value };
    private static string Build(ComplainFormData form)
    {
        string json, error;
        Check(form.TryBuildContext(out json, out error), "Unexpected validation failure: " + error);
        return json;
    }
    private static void Reject(ComplainFormData form, string expected)
    {
        string json, error;
        Check(!form.TryBuildContext(out json, out error), "Expected validation failure");
        Check(json == null && error.Contains(expected), "Unexpected validation result: " + error);
    }
    public static void Main()
    {
        Check(Build(Form()) == "{}", "Empty context must be an object");
        foreach (var blank in new[] { null, "", " \t\n" })
        {
            var id = Form(); id.TargetId = blank; Reject(id, "目标 ID");
            var name = Form(); name.TargetName = blank; Reject(name, "目标名称");
            var key = Form(); key.Context.Add(Entry(blank, ComplainContextType.String, "x")); Reject(key, "key");
        }
        var mixed = Form();
        mixed.Context.Add(Entry(" count ", ComplainContextType.Number, "42"));
        mixed.Context.Add(Entry("ratio", ComplainContextType.Number, "-1.25e2"));
        mixed.Context.Add(Entry("yes", ComplainContextType.Boolean, "true"));
        mixed.Context.Add(Entry("no", ComplainContextType.Boolean, "false"));
        mixed.Context.Add(Entry("empty", ComplainContextType.String, ""));
        var escaped = " 中文 \"引用\"\n换行\\路径 ";
        mixed.Context.Add(Entry("特殊\"key", ComplainContextType.String, escaped));
        var previous = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
        try
        {
            var result = JObject.Parse(Build(mixed));
            Check(result["count"].Type == JTokenType.Integer && result["count"].Value<int>() == 42, "Integer type or key normalization lost");
            Check(result["ratio"].Type == JTokenType.Float && result["ratio"].Value<double>() == -125, "Number or exponent lost");
            Check(result["yes"].Type == JTokenType.Boolean && result["yes"].Value<bool>(), "true became a string");
            Check(result["no"].Type == JTokenType.Boolean && !result["no"].Value<bool>(), "false became a string");
            Check(result["empty"].Type == JTokenType.String && result["empty"].Value<string>() == "", "Empty string lost");
            Check(result["特殊\"key"].Value<string>() == escaped, "Escaping or whitespace changed");
        }
        finally { Thread.CurrentThread.CurrentCulture = previous; }
        foreach (var valid in new[] { "0", "-0", "1", "-123", " 1.5 ", "1e3", "1E-3", "2e+2", "9223372036854775808" })
        {
            var form = Form(); form.Context.Add(Entry("n", ComplainContextType.Number, valid));
            var token = JObject.Parse(Build(form))["n"];
            Check(token.Type == JTokenType.Integer || token.Type == JTokenType.Float, "Number serialized as another type");
        }
        foreach (var invalid in new[] { "", " ", "NaN", "Infinity", "-Infinity", "1e400", "1,5", "true", "1.2.3", "12abc", "01", "1\n2", "{}", "[1]" })
        {
            var form = Form(); form.Context.Add(Entry("n", ComplainContextType.Number, invalid)); Reject(form, "number");
        }
        var duplicate = Form();
        duplicate.Context.Add(Entry(" key ", ComplainContextType.String, "first"));
        duplicate.Context.Add(Entry("key", ComplainContextType.Number, "1")); Reject(duplicate, "重复");
        duplicate.Context.RemoveAt(1);
        Check(JObject.Parse(Build(duplicate))["key"].Value<string>() == "first", "Removing a row corrupts remaining data");
        var invalidBool = Form(); invalidBool.Context.Add(Entry("b", ComplainContextType.Boolean, "1")); Reject(invalidBool, "true 或 false");
        var invalidType = Form(); invalidType.Context.Add(Entry("x", (ComplainContextType)99, "1")); Reject(invalidType, "不支持");
        Console.WriteLine("ComplainFormData: " + assertions + " assertions passed");
    }
}
