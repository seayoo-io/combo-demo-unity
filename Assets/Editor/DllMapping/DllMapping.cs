using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class DllMapping
{
    // https://docs.unity3d.com/Manual/ClassIDReference.html
    // MonoScript => 115
    public const string DEFAULT_FILE_ID_OF_SCRIPT = "11500000";

    public static Dictionary<string, string> GetFielIDMappingFromDll(string dllFilePath)
    {
        var mapping = new Dictionary<string, string>();
        Assembly assembly = Assembly.LoadFrom(dllFilePath);
        Type[] array = null;
        try
        {
            array = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            array = ex.Types.Where((Type t) => t != null).ToArray();
            Exception[] loaderExceptions = ex.LoaderExceptions;
            for (int i = 0; i < loaderExceptions.Length; i++)
            {
                Debug.LogWarning(loaderExceptions[i]);
            }
        }
        Type[] array2 = array;
        foreach (Type type in array2)
        {
            mapping[type.Name] = FileIDUtil.Compute(type).ToString();
        }
        return mapping;
    }

    public static Dictionary<string, string> GetGUIDMappingFromCode(string path)
    {
        var mapping = new Dictionary<string, string>();
        foreach (string item in FindAllFileWithSuffixs(path, new string[1] { ".cs.meta" }))
        {
            mapping[GetGuidFromMeta(item)] = GetFileNameFromPath(item);
        }
        return mapping;
    }

    public static void ReplaceScriptsFromDll(string filePath, Dictionary<string, string> mapping, string dllGUID)
    {
        Debug.Log("Ready to replace: " + filePath);
        string[] array = File.ReadAllLines(filePath);
        int i = 0;
        bool flag = false;
        for (; i < array.Length; i++)
        {
            if (array[i].StartsWith("MonoBehaviour:"))
            {
                do
                {
                    i++;
                }
                while (!array[i].TrimStart().StartsWith("m_Script:"));
                flag = flag | replaceGUIDAnfFileIDFromSrcToDll(mapping, dllGUID, ref array[i]);
            }
        }
        if (flag)
        {
            File.WriteAllLines(filePath, array);
        }
    }

    public static string GetGuidFromMeta(string filePath)
    {
        string result = "";
        using (StreamReader streamReader = new StreamReader(filePath))
        {
            while (!streamReader.EndOfStream)
            {
                string text = streamReader.ReadLine();
                if (text.StartsWith("guid:"))
                {
                    result = text.Substring(text.IndexOf(":") + 2);
                    break;
                }
            }
        }
        return result;
    }

    private static bool replaceGUIDAnfFileIDFromSrcToDll(Dictionary<string, string> mapping, string dllGUID, ref string lineStr)
    {
        bool result = false;
        string gUIDFrommScriptReferenceLine = GetGUIDFrommScriptReferenceLine(lineStr);
        if (gUIDFrommScriptReferenceLine == null)
        {
            return false;
        }
        if (mapping.TryGetValue(gUIDFrommScriptReferenceLine, out var value))
        {
            Debug.Log("Replacing script reference guid: " + gUIDFrommScriptReferenceLine);
            lineStr = lineStr.Replace(DEFAULT_FILE_ID_OF_SCRIPT, value);
            lineStr = lineStr.Replace(gUIDFrommScriptReferenceLine, dllGUID);
            result = true;
        }
        return result;
    }

    public static string GetGUIDFrommScriptReferenceLine(string lineStr)
    {
        int num = lineStr.IndexOf("guid:") + "guid: ".Length;
        int num2 = lineStr.LastIndexOf(",") - num;
        if (num2 <= 0)
        {
            return null;
        }
        return lineStr.Substring(num, num2);
    }

    private static string GetFileNameFromPath(string path)
    {
        string fileNameWithCs = Path.GetFileNameWithoutExtension(path);
        return Path.GetFileNameWithoutExtension(fileNameWithCs);
    }

    private static List<string> FindAllFileWithSuffixs(string path, string[] suffixs)
    {
        List<string> resultList = new List<string>();
        FindAllFileWithSuffixs(path, suffixs, ref resultList);
        return resultList;
    }

    private static void FindAllFileWithSuffixs(string path, string[] suffixs, ref List<string> resultList)
    {
        if (File.Exists(path))
        {
            resultList.Add(path);
        }
        else
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            string[] files = Directory.GetFiles(path);
            foreach (string text in files)
            {
                foreach (string value in suffixs)
                {
                    if (text.EndsWith(value))
                    {
                        resultList.Add(text);
                        break;
                    }
                }
            }
            files = Directory.GetDirectories(path);
            foreach (string path2 in files)
            {
                FindAllFileWithSuffixs(path2, suffixs, ref resultList);
            }
        }
    }
}

public static class FileIDUtil
{
    public static int Compute(Type t)
    {
        int num = 0;
        string s = "s\0\0\0" + t.Namespace + t.Name;
        using (HashAlgorithm hashAlgorithm = new MD4())
        {
            byte[] array = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(s));
            for (int num2 = 3; num2 >= 0; num2--)
            {
                num <<= 8;
                num |= array[num2];
            }
        }
        return num;
    }
}

public class MD4 : HashAlgorithm
{
    private uint _a;

    private uint _b;

    private uint _c;

    private uint _d;

    private uint[] _x;

    private int _bytesProcessed;

    public MD4()
    {
        _x = new uint[16];
        Initialize();
    }

    public override void Initialize()
    {
        _a = 1732584193u;
        _b = 4023233417u;
        _c = 2562383102u;
        _d = 271733878u;
        _bytesProcessed = 0;
    }

    protected override void HashCore(byte[] array, int offset, int length)
    {
        ProcessMessage(Bytes(array, offset, length));
    }

    protected override byte[] HashFinal()
    {
        try
        {
            ProcessMessage(Padding());
            return new uint[4] { _a, _b, _c, _d }.SelectMany((uint word) => Bytes(word)).ToArray();
        }
        finally
        {
            Initialize();
        }
    }

    private void ProcessMessage(IEnumerable<byte> bytes)
    {
        foreach (byte @byte in bytes)
        {
            int num = _bytesProcessed & 0x3F;
            int num2 = num >> 2;
            int num3 = (num & 3) << 3;
            _x[num2] = (_x[num2] & (uint)(~(255 << num3))) | (uint)(@byte << num3);
            if (num == 63)
            {
                Process16WordBlock();
            }
            _bytesProcessed++;
        }
    }

    private static IEnumerable<byte> Bytes(byte[] bytes, int offset, int length)
    {
        for (int i = offset; i < length; i++)
        {
            yield return bytes[i];
        }
    }

    private IEnumerable<byte> Bytes(uint word)
    {
        yield return (byte)(word & 0xFFu);
        yield return (byte)((word >> 8) & 0xFFu);
        yield return (byte)((word >> 16) & 0xFFu);
        yield return (byte)((word >> 24) & 0xFFu);
    }

    private IEnumerable<byte> Repeat(byte value, int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return value;
        }
    }

    private IEnumerable<byte> Padding()
    {
        return Repeat(128, 1).Concat(Repeat(0, ((_bytesProcessed + 8) & 0x7FFFFFC0) + 55 - _bytesProcessed)).Concat(Bytes((uint)(_bytesProcessed << 3))).Concat(Repeat(0, 4));
    }

    private void Process16WordBlock()
    {
        uint num = _a;
        uint num2 = _b;
        uint num3 = _c;
        uint num4 = _d;
        int[] array = new int[4] { 0, 4, 8, 12 };
        foreach (int num5 in array)
        {
            num = Round1Operation(num, num2, num3, num4, _x[num5], 3);
            num4 = Round1Operation(num4, num, num2, num3, _x[num5 + 1], 7);
            num3 = Round1Operation(num3, num4, num, num2, _x[num5 + 2], 11);
            num2 = Round1Operation(num2, num3, num4, num, _x[num5 + 3], 19);
        }
        array = new int[4] { 0, 1, 2, 3 };
        foreach (int num6 in array)
        {
            num = Round2Operation(num, num2, num3, num4, _x[num6], 3);
            num4 = Round2Operation(num4, num, num2, num3, _x[num6 + 4], 5);
            num3 = Round2Operation(num3, num4, num, num2, _x[num6 + 8], 9);
            num2 = Round2Operation(num2, num3, num4, num, _x[num6 + 12], 13);
        }
        array = new int[4] { 0, 2, 1, 3 };
        foreach (int num7 in array)
        {
            num = Round3Operation(num, num2, num3, num4, _x[num7], 3);
            num4 = Round3Operation(num4, num, num2, num3, _x[num7 + 8], 9);
            num3 = Round3Operation(num3, num4, num, num2, _x[num7 + 4], 11);
            num2 = Round3Operation(num2, num3, num4, num, _x[num7 + 12], 15);
        }
        _a += num;
        _b += num2;
        _c += num3;
        _d += num4;
    }

    private static uint ROL(uint value, int numberOfBits)
    {
        return (value << numberOfBits) | (value >> 32 - numberOfBits);
    }

    private static uint Round1Operation(uint a, uint b, uint c, uint d, uint xk, int s)
    {
        return ROL(a + ((b & c) | (~b & d)) + xk, s);
    }

    private static uint Round2Operation(uint a, uint b, uint c, uint d, uint xk, int s)
    {
        return ROL(a + ((b & c) | (b & d) | (c & d)) + xk + 1518500249, s);
    }

    private static uint Round3Operation(uint a, uint b, uint c, uint d, uint xk, int s)
    {
        return ROL(a + (b ^ c ^ d) + xk + 1859775393, s);
    }
}
