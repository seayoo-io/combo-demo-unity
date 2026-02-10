using System;
using System.Text;
using Newtonsoft.Json;

public static class JWTParser
{
    /// <summary>
    /// 解析 JWT 并返回指定类型的 Payload 数据。
    /// </summary>
    /// <typeparam name="T">Payload 的目标类型。</typeparam>
    /// <param name="jwt">JWT 字符串。</param>
    /// <returns>解析后的 Payload 数据（如果解析失败，则返回默认值）。</returns>
    public static T Parse<T>(string jwt)
    {
        try
        {
            // 分割 JWT
            string[] parts = jwt.Split('.');
            if (parts.Length != 3)
            {
                throw new ArgumentException("JWT 格式无效");
            }

            // 解码
            string json = DecodeBase64(parts[1]);

            // 将 JSON 数据反序列化为指定类型 T
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JWT 解析时发生错误: {ex.Message}");
            return default;
        }
    }

    /// <summary>
    /// Base64 解码。
    /// </summary>
    /// <param name="base64String">Base64 编码的字符串。</param>
    /// <returns>解码后的字符串。</returns>
    private static string DecodeBase64(string base64String)
    {
        string paddedBase64String = base64String.Replace('-', '+').Replace('_', '/');
        switch (paddedBase64String.Length % 4)
        {
            case 2: paddedBase64String += "=="; break;
            case 3: paddedBase64String += "="; break;
        }

        byte[] data = Convert.FromBase64String(paddedBase64String);
        return Encoding.UTF8.GetString(data);
    }
}