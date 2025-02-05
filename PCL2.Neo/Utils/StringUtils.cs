using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PCL2.Neo.Utils;

public static class StringUtils
{
    /// <summary>
    /// 返回一个枚举对应的字符串。
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    public static string GetEnumName(Enum enumData)
    {
        var result = Enum.GetName(enumData.GetType(), enumData);
        return string.IsNullOrEmpty(result) ? throw new ArgumentNullException() : result;
    }

    /// <summary>
    /// 将文件大小转化为适合的文本形式，如“1.28 MB”。
    /// </summary>
    public static string GetFileSizeString(long fileSize)
    {
        var isNegative = fileSize < 0;
        if (isNegative) fileSize *= -1;
        switch (fileSize)
        {
            case < 1000:
                return (isNegative ? "-" : "") + fileSize + " B";
            case < 1024 * 1000:
            {
                var roundResult = Math.Round((double)fileSize / 1024).ToString(CultureInfo.CurrentCulture);
                return (isNegative ? "-" : "") + Math.Round((double)fileSize / 1024,
                    (int)MathUtils.MathClamp(3 - roundResult.Length, 0, 2)) + " KB";
            }
            case < 1024 * 1024 * 1000:
            {
                var roundResult = Math.Round((double)fileSize / 1024 / 1024).ToString(CultureInfo.CurrentCulture);
                return (isNegative ? "-" : "") + Math.Round((double)fileSize / 1024 / 1024,
                    (int)MathUtils.MathClamp(3 - roundResult.Length, 0, 2)) + " MB";
            }
            default:
            {
                var roundResult = Math.Round((double)fileSize / 1024 / 1024 / 1024)
                    .ToString(CultureInfo.CurrentCulture);
                return (isNegative ? "-" : "") + Math.Round((double)fileSize / 1024 / 1024 / 1024,
                    (int)MathUtils.MathClamp(3 - roundResult.Length, 0, 2)) + " GB";
            }
        }
    }

    /// <summary>
    /// 将第一个字符转换为大写，其余字符转换为小写。
    /// </summary>
    public static string Capitalize(this string word)
    {
        if (string.IsNullOrEmpty(word)) return word;
        return word[..1].ToUpperInvariant() + word[1..].ToLowerInvariant();
    }

    /// <summary>
    /// 将字符串统一至某个长度，过短则以 Code 将其右侧填充，过长则截取靠左的指定长度。
    /// </summary>
    public static string StrFill(string str, string code, byte length) =>
        str.Length > length ? str[..length] : str.PadRight(length, Convert.ToChar(code));

    /// <summary>
    /// 将一个小数显示为固定的小数点后位数形式，将向零取整。
    /// 如 12 保留 2 位则输出 12.00，而 95.678 保留 2 位则输出 95.67。
    /// </summary>
    public static string StrFillNum(double num, int length)
    {
        var result = Math.Round(num, length, MidpointRounding.AwayFromZero).ToString(CultureInfo.CurrentCulture);
        return !result.Contains(".")
            ? (result + ".").PadRight(result.Length + 1 + length, '0')
            : result.PadRight(result.Split(".")[0].Length + 1 + length, '0');
    }

    /// <summary>
    /// 移除字符串首尾的标点符号、回车，以及括号中、冒号后的补充说明内容。
    /// </summary>
    public static string StrTrim(string str,bool removeQuote = true)
    {
        if (removeQuote) str = str.Split("（")[0].Split("：")[0].Split("(")[0].Split(":")[0];
        return str.Trim('.', '。', '！', ' ', '!', '?', '？', '“', '”');
    }

    /// <summary>
    /// 连接字符串。
    /// </summary>
    public static string Join<T>(this IEnumerable<T> list, string split) => string.Join(split, list);

    /// <summary>
    /// 分割字符串。
    /// </summary>
    public static string[] Split(this string fullStr, string splitStr) => fullStr.Split(splitStr);

    /// <summary>
    /// 获取字符串哈希值。
    /// </summary>
    public static ulong GetHash(string str) => (ulong)str.GetHashCode();

    /// <summary>
    /// 获取字符串 MD5。
    /// </summary>
    public static string GetStringMd5(string str)
    {
        var hashData = MD5.HashData(Encoding.UTF8.GetBytes(str));
        return hashData.Aggregate(new StringBuilder(),
            (s, b) => s.Append(b.ToString("x2"))).ToString();
    }

    /// <summary>
    /// 检查字符串中的字符是否均为 ASCII 字符。
    /// </summary>
    public static bool IsAscii(this string input) => input.All((c) => c < 128);

    /// <summary>
    /// 获取在子字符串第一次出现之前的部分，例如对 2024/11/08 拆切 / 会得到 2024。
    /// 如果未找到子字符串则不裁切。
    /// </summary>
    public static string BeforeFirst(this string str, string text, bool ignoreCase = false)
    {
        var pos = string.IsNullOrEmpty(text)
            ? -1
            : str.LastIndexOf(text, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        return pos >= 0 ? str[..pos] : str;
    }

    /// <summary>
    /// 获取在子字符串最后一次出现之前的部分，例如对 2024/11/08 拆切 / 会得到 2024/11。
    /// 如果未找到子字符串则不裁切。
    /// </summary>
    public static string BeforeLast(this string str, string text, bool ignoreCase = false)
    {
        var pos = string.IsNullOrEmpty(text)
            ? -1
            : str.LastIndexOf(text, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        return pos >= 0 ? str[..pos] : str;
    }

    /// <summary>
    /// 获取在子字符串第一次出现之后的部分，例如对 2024/11/08 拆切 / 会得到 11/08。
    /// 如果未找到子字符串则不裁切。
    /// </summary>
    public static string AfterFirst(this string str, string text, bool ignoreCase = false)
    {
        var pos = string.IsNullOrEmpty(text)
            ? -1
            : str.LastIndexOf(text, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        return pos >= 0 ? str[(pos + text.Length)..] : str;
    }
    
    /// <summary>
    /// 获取在子字符串最后一次出现之后的部分，例如对 2024/11/08 拆切 / 会得到 08。
    /// 如果未找到子字符串则不裁切。
    /// </summary>
    public static string AfterLast(this string str, string text, bool ignoreCase = false)
    {
        var pos = string.IsNullOrEmpty(text)
            ? -1
            : str.LastIndexOf(text, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        return pos >= 0 ? str[(pos + text.Length)..] : str;
    }

    /// <summary>
    /// 获取处于两个子字符串之间的部分，裁切尽可能多的内容。
    /// 如果未找到子字符串则不裁切。
    /// </summary>
    /// <param name="str"></param>
    /// <param name="after"></param>
    /// <param name="before"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    public static string Between(this string str, string after, string before, bool ignoreCase = false)
    {
        var startPos = string.IsNullOrEmpty(after)
            ? -1
            : str.LastIndexOf(after, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        if (startPos >= 0) startPos += after.Length;
        else startPos = 0;
        var endPos = string.IsNullOrEmpty(before)
            ? -1
            : str.IndexOf(before, startPos, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        if (endPos >= 0) return str.Substring(startPos, endPos - startPos);
        else if (startPos > 0) return str[startPos..];
        else return str;
    }

    /// <summary>
    /// 不会报错的 Val。
    /// 如果输入有误，返回 0。
    /// </summary>
    public static double ToDouble(this string str)
    {
        try
        {
            return double.Parse(str);
        }
        catch (Exception)
        {
            return 0;
        }
    }

    /// <summary>
    /// 为字符串进行 XML 转义。
    /// </summary>
    public static string EscapeXml(string str)
    {
        if (str.StartsWith("{")) str = "{}" + str;
        return str.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;")
            .Replace("\"", "&quot;").Replace("\r\n", "&#xa;");
    }

    /// <summary>
    /// 搜索字符串中的所有正则匹配项。
    /// </summary>
    public static List<string> RegexSearch(string str, string regex, RegexOptions options = RegexOptions.None)
    {
        try
        {
            return Regex.Matches(str, regex, options).Select(m => m.Value).ToList();
        }
        catch (Exception)
        {
            // TODO Log(ex, "正则匹配全部项出错");
            return [];
        }
    }

    /// <summary>
    /// 获取字符串中的第一个正则匹配项，若无匹配则返回 Nothing。
    /// </summary>
    public static string? RegexSeek(string str, string regex, RegexOptions options = RegexOptions.None)
    {
        try
        {
            var maech = Regex.Match(str, regex, options);
            return maech.Success ? maech.Value : null;
        }
        catch (Exception)
        {
            //TODO Log(ex, "正则匹配第一项出错");
            return null;
        }
    }
    
    /// <summary>
    /// 检查字符串是否匹配某正则模式。
    /// </summary>
    public static bool RegexCheck(string str, string regex, RegexOptions options = RegexOptions.None)
    {
        try
        {
            return Regex.IsMatch(str, regex, options);
        }
        catch (Exception)
        {
            //TODO Log(ex, "正则检查出错");
            return false;
        }
    }
}
