using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

internal enum ComplainContextType
{
    Number,
    String,
    Boolean
}

internal class ComplainContextEntry
{
    public string Key;
    public ComplainContextType Type;
    public string Value;
}

// 独立于 UI 和 SDK 的表单校验，确保 Context 中的数字、布尔值保留 JSON 类型。
internal class ComplainFormData
{
    public string TargetId;
    public string TargetName;
    public readonly List<ComplainContextEntry> Context = new List<ComplainContextEntry>();

    public bool TryBuildContext(out string json, out string error)
    {
        json = null;
        error = null;
        if (string.IsNullOrWhiteSpace(TargetId))
        {
            error = "请填写目标 ID";
            return false;
        }
        if (string.IsNullOrWhiteSpace(TargetName))
        {
            error = "请填写目标名称";
            return false;
        }

        var result = new JObject();
        var keys = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < Context.Count; index++)
        {
            var entry = Context[index];
            var key = entry.Key == null ? "" : entry.Key.Trim();
            var prefix = $"Context 第 {index + 1} 项：";
            if (key.Length == 0)
            {
                error = prefix + "请填写 key";
                return false;
            }
            if (!keys.Add(key))
            {
                error = prefix + "key 重复";
                return false;
            }

            JToken value;
            switch (entry.Type)
            {
                case ComplainContextType.String:
                    value = new JValue(entry.Value ?? "");
                    break;
                case ComplainContextType.Boolean:
                    bool boolean;
                    if (!bool.TryParse(entry.Value, out boolean))
                    {
                        error = prefix + "请选择 true 或 false";
                        return false;
                    }
                    value = new JValue(boolean);
                    break;
                case ComplainContextType.Number:
                    var number = (entry.Value ?? "").Trim();
                    // JSON 数值语法使用小数点，与系统区域设置无关。
                    if (!Regex.IsMatch(number, @"\A-?(?:0|[1-9][0-9]*)(?:\.[0-9]+)?(?:[eE][+-]?[0-9]+)?\z"))
                    {
                        error = prefix + "请输入有效的 number";
                        return false;
                    }
                    try
                    {
                        value = JToken.Parse(number);
                        if (value.Type == JTokenType.Float &&
                            (double.IsInfinity(value.Value<double>()) || double.IsNaN(value.Value<double>())))
                            throw new FormatException();
                    }
                    catch (Exception ex) when (ex is JsonException || ex is FormatException || ex is OverflowException)
                    {
                        error = prefix + "请输入有限的 number";
                        return false;
                    }
                    break;
                default:
                    error = prefix + "不支持的类型";
                    return false;
            }
            result.Add(key, value);
        }
        json = result.ToString(Formatting.None);
        return true;
    }
}
