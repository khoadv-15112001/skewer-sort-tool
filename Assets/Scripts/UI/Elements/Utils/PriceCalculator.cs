using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

public class CurrencyFormat
{
    public bool IsPrefix;        // Vị trí mặc định của đơn vị trong locale (tham khảo)
    public bool HasSpace;        // Locale thường có khoảng trắng giữa đơn vị và số?
    public string Culture;       // Culture dùng để nhóm/thập phân khi format
    public string DecimalFormat; // Mặc định; sẽ bị override theo kiểu nhập của người dùng
    public decimal RoundingUnit; // Bước làm tròn tối thiểu (có thể override theo input)
}

public static class PriceCalculator
{
    /// <summary>
    /// Một số cấu hình mặc định theo tiền tệ phổ biến (để lấy Culture/RoundingUnit đặc thù).
    /// LƯU Ý: Hiển thị cuối cùng sẽ GIỮ NGUYÊN "unit" người dùng nhập (vd. "THB", "US$", "руб.", "BHT"...),
    /// không ép đổi sang ký hiệu locale.
    /// </summary>
    public static readonly Dictionary<string, CurrencyFormat> CurrencyFormats = new(StringComparer.Ordinal)
    {
        ["₫"] = new CurrencyFormat { IsPrefix = false, HasSpace = true, Culture = "vi-VN", DecimalFormat = "N0", RoundingUnit = 1000m },
        ["$"] = new CurrencyFormat { IsPrefix = true, HasSpace = false, Culture = "en-US", DecimalFormat = "N2", RoundingUnit = 0.01m },
        ["US$"] = new CurrencyFormat { IsPrefix = true, HasSpace = true, Culture = "en-US", DecimalFormat = "N2", RoundingUnit = 0.01m },
        ["€"] = new CurrencyFormat { IsPrefix = false, HasSpace = true, Culture = "fr-FR", DecimalFormat = "N2", RoundingUnit = 0.10m },
        ["£"] = new CurrencyFormat { IsPrefix = true, HasSpace = false, Culture = "en-GB", DecimalFormat = "N2", RoundingUnit = 0.01m },
        ["¥"] = new CurrencyFormat { IsPrefix = true, HasSpace = false, Culture = "ja-JP", DecimalFormat = "N0", RoundingUnit = 100m },
        ["₩"] = new CurrencyFormat { IsPrefix = true, HasSpace = false, Culture = "ko-KR", DecimalFormat = "N0", RoundingUnit = 100m },
        ["zł"] = new CurrencyFormat { IsPrefix = false, HasSpace = true, Culture = "pl-PL", DecimalFormat = "N2", RoundingUnit = 0.10m },
        ["₽"] = new CurrencyFormat { IsPrefix = true, HasSpace = true, Culture = "ru-RU", DecimalFormat = "N2", RoundingUnit = 0.10m },
        ["руб."] = new CurrencyFormat { IsPrefix = false, HasSpace = true, Culture = "ru-RU", DecimalFormat = "N2", RoundingUnit = 0.10m },
        ["R"] = new CurrencyFormat { IsPrefix = true, HasSpace = true, Culture = "en-ZA", DecimalFormat = "N2", RoundingUnit = 0.01m },   // Rand
        ["ZAR"] = new CurrencyFormat { IsPrefix = true, HasSpace = true, Culture = "en-ZA", DecimalFormat = "N2", RoundingUnit = 0.01m },
        ["R$"] = new CurrencyFormat { IsPrefix = true, HasSpace = true, Culture = "pt-BR", DecimalFormat = "N2", RoundingUnit = 0.10m },   // Brazil
        ["A$"] = new CurrencyFormat { IsPrefix = true, HasSpace = false, Culture = "en-AU", DecimalFormat = "N2", RoundingUnit = 0.01m },
        ["CA$"] = new CurrencyFormat { IsPrefix = true, HasSpace = false, Culture = "en-CA", DecimalFormat = "N2", RoundingUnit = 0.01m },
        ["HK$"] = new CurrencyFormat { IsPrefix = true, HasSpace = false, Culture = "zh-HK", DecimalFormat = "N2", RoundingUnit = 0.10m },
        ["NT$"] = new CurrencyFormat { IsPrefix = true, HasSpace = false, Culture = "zh-TW", DecimalFormat = "N2", RoundingUnit = 1m },
        ["S$"] = new CurrencyFormat { IsPrefix = true, HasSpace = false, Culture = "en-SG", DecimalFormat = "N2", RoundingUnit = 0.01m },
        ["MX$"] = new CurrencyFormat { IsPrefix = true, HasSpace = true, Culture = "es-MX", DecimalFormat = "N2", RoundingUnit = 0.10m },
        ["CHF"] = new CurrencyFormat { IsPrefix = true, HasSpace = true, Culture = "de-CH", DecimalFormat = "N2", RoundingUnit = 0.05m },   // 0.05 CHF
        ["kr"] = new CurrencyFormat { IsPrefix = false, HasSpace = true, Culture = "sv-SE", DecimalFormat = "N2", RoundingUnit = 0.10m },   // generic
        ["฿"] = new CurrencyFormat { IsPrefix = true, HasSpace = true, Culture = "th-TH", DecimalFormat = "N2", RoundingUnit = 1m },
        ["THB"] = new CurrencyFormat { IsPrefix = true, HasSpace = true, Culture = "th-TH", DecimalFormat = "N2", RoundingUnit = 0.01m },
        ["USD"] = new CurrencyFormat { IsPrefix = true, HasSpace = true, Culture = "en-US", DecimalFormat = "N2", RoundingUnit = 0.01m },
        ["EUR"] = new CurrencyFormat { IsPrefix = false, HasSpace = true, Culture = "fr-FR", DecimalFormat = "N2", RoundingUnit = 0.10m },
        ["JPY"] = new CurrencyFormat { IsPrefix = true, HasSpace = true, Culture = "ja-JP", DecimalFormat = "N0", RoundingUnit = 1m },
        ["KRW"] = new CurrencyFormat { IsPrefix = true, HasSpace = true, Culture = "ko-KR", DecimalFormat = "N0", RoundingUnit = 100m },
        ["VND"] = new CurrencyFormat { IsPrefix = false, HasSpace = true, Culture = "vi-VN", DecimalFormat = "N0", RoundingUnit = 1000m },
        ["PLN"] = new CurrencyFormat { IsPrefix = false, HasSpace = true, Culture = "pl-PL", DecimalFormat = "N2", RoundingUnit = 0.10m },
        ["RUB"] = new CurrencyFormat { IsPrefix = false, HasSpace = true, Culture = "ru-RU", DecimalFormat = "N2", RoundingUnit = 0.10m },
        ["BRL"] = new CurrencyFormat { IsPrefix = true, HasSpace = true, Culture = "pt-BR", DecimalFormat = "N2", RoundingUnit = 0.10m }
    };

    /// <summary>
    /// API chính: tính giá gốc (trước giảm) từ chuỗi giá đã giảm và % giảm.
    /// - Tách unit & number tổng quát (giữ nguyên unit gốc).
    /// - Chuẩn hoá số theo quy tắc dấu cuối là thập phân.
    /// - Làm tròn theo số chữ số thập phân xuất hiện trong input.
    /// - Format theo culture phù hợp nếu tìm được, ngược lại fallback en-US.
    /// </summary>
    public static string CalculateOriginalPriceFromText(string discountedPriceStr, int salePercent)
    {
        if (string.IsNullOrWhiteSpace(discountedPriceStr)) return null;
        if (salePercent < 0 || salePercent > 100) return null;

        // 1) TÁCH: unit + number (giữ nguyên "unit" như người dùng nhập)
        if (!TrySplitCurrencyAndNumber(discountedPriceStr, out string unit, out bool isPrefix, out string rawNumber, out bool hadSpace))
            return null;

        // 2) Chuẩn hoá & parse số
        string normalized = NormalizeNumberRobust(rawNumber);
        if (!decimal.TryParse(normalized, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                              CultureInfo.InvariantCulture, out decimal discounted))
            return null;

        // 3) Tính giá gốc
        decimal original;
        if (salePercent == 0)
        {
            // Chế độ format-only
            original = discounted;
        }
        else
        {
            decimal factor = salePercent / 100m;
            original = discounted / (1m - factor);
        }

        // 4) Suy ra số chữ số thập phân từ input -> dùng để format & bước làm tròn
        int decs = DecimalDigitsFromInput(rawNumber);
        decimal roundingUnitFromInput = decs == 0 ? 1m : 1m / (decimal)Math.Pow(10, decs);

        // 5) Lấy cấu hình theo unit (nếu có) để chọn Culture / RoundingUnit đặc thù
        var fmt = GetFormatForUnit(unit);

        // Bước làm tròn ưu tiên: lớn nhất giữa rule theo input và rule đặc thù (nếu muốn ưu tiên input thì dùng rule theo input)
        decimal roundingUnit = Math.Max(roundingUnitFromInput, fmt.RoundingUnit > 0 ? fmt.RoundingUnit : roundingUnitFromInput);

        if (roundingUnit > 0)
            original = Math.Ceiling(original / roundingUnit) * roundingUnit;

        // 6) Format theo đúng số thập phân đã suy ra từ input
        string decimalFormat = "N" + decs;
        var ci = new CultureInfo(fmt.Culture);
        string formatted = original.ToString(decimalFormat, ci);

        // 7) GHÉP: giữ nguyên unit + giữ đúng có/không khoảng trắng như input
        string space = hadSpace ? " " : "";
        return isPrefix
            ? $"{unit}{space}{formatted}"
            : $"{formatted}{space}{unit}".TrimEnd();
    }

    // ======================
    // Helpers
    // ======================

    /// <summary>
    /// Tách phần đơn vị & phần số theo cách tổng quát.
    /// Quy tắc:
    /// - Tìm khối số hợp lệ (có thể có dấu âm, dấu nhóm, thập phân).
    /// - Phần còn lại bên trái/phải là đơn vị. Ưu tiên phía có ký hiệu tiền hoặc chữ cái.
    /// - Giữ nguyên unit đúng như người dùng gõ (không ép đổi).
    /// </summary>
    private static bool TrySplitCurrencyAndNumber(
        string input,
        out string unit, out bool isPrefix, out string rawNumber, out bool hadSpace)
    {
        unit = null; isPrefix = false; rawNumber = null; hadSpace = false;
        if (string.IsNullOrWhiteSpace(input)) return false;

        string s = input.Trim().Replace("\u00A0", " ");

        // Bắt khối số:
        //  - cho phép dạng: 1 234,56  |  1.234,56  |  1,234.56  |  1234  |  -1.234  |  +1,234.56
        //  - regex tìm một "cụm số" lớn nhất có thể
        var m = Regex.Match(
            s,
            @"(?<num>[+\-]?(?:\d{1,3}(?:[ \u00A0.,]\d{3})+|\d+)(?:[.,]\d+)? )",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant
        );

        if (!m.Success) return false;

        rawNumber = m.Groups["num"].Value.Trim();

        string left = s[..m.Index];
        string right = s[(m.Index + m.Length)..];

        // Có khoảng trắng tách giữa unit và số?
        hadSpace = (left.EndsWith(" ") || right.StartsWith(" "));

        string leftTrim = left.Trim();
        string rightTrim = right.Trim();

        bool leftHas = leftTrim.Length > 0;
        bool rightHas = rightTrim.Length > 0;

        bool LooksCurrency(string t) =>
            Regex.IsMatch(t, @"[\p{Sc}\p{L}]+", RegexOptions.CultureInvariant);

        if (leftHas && !rightHas) { unit = leftTrim; isPrefix = true; return true; }
        if (!leftHas && rightHas) { unit = rightTrim; isPrefix = false; return true; }

        if (leftHas && rightHas)
        {
            bool leftCur = LooksCurrency(leftTrim);
            bool rightCur = LooksCurrency(rightTrim);
            if (leftCur && !rightCur) { unit = leftTrim; isPrefix = true; return true; }
            if (!leftCur && rightCur) { unit = rightTrim; isPrefix = false; return true; }
            // Cả hai đều có vẻ là unit → mặc định chọn prefix (trái)
            unit = leftTrim; isPrefix = true; return true;
        }

        // Không thấy unit rõ ràng → unit rỗng, coi như prefix
        unit = ""; isPrefix = true; return true;
    }

    /// <summary>
    /// Quy tắc normalize: Dấu phân cách cuối cùng (',' hoặc '.') coi là thập phân; các dấu còn lại là hàng nghìn.
    /// Trường hợp chỉ có 1 dấu và phía sau đúng 3 chữ số ⇒ coi là hàng nghìn.
    /// </summary>
    private static string NormalizeNumberRobust(string raw)
    {
        string s = raw.Replace("\u00A0", "").Replace(" ", "");
        bool neg = s.StartsWith("-");
        bool pos = s.StartsWith("+");
        if (neg || pos) s = s[1..];

        int lastComma = s.LastIndexOf(',');
        int lastDot = s.LastIndexOf('.');
        int lastSep = Math.Max(lastComma, lastDot);

        if (lastSep < 0)
            return (neg ? "-" : (pos ? "+" : "")) + s;

        int commaCount = s.Count(c => c == ',');
        int dotCount = s.Count(c => c == '.');

        if ((commaCount + dotCount) == 1)
        {
            char sep = commaCount == 1 ? ',' : '.';
            int idx = s.IndexOf(sep);
            int after = s.Length - idx - 1;

            if (after == 3)
            {
                s = s.Remove(idx, 1); // hàng nghìn
            }
            else
            {
                if (sep == ',') s = s.Replace(",", "."); // thập phân
            }

            return (neg ? "-" : (pos ? "+" : "")) + s;
        }

        // Nhiều dấu: dấu cuối là thập phân
        var sb = new System.Text.StringBuilder(s.Length);
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (c == ',' || c == '.')
            {
                if (i == lastSep) sb.Append('.');
                // các dấu khác bỏ qua (nghìn)
            }
            else sb.Append(c);
        }

        return (neg ? "-" : (pos ? "+" : "")) + sb.ToString();
    }

    /// <summary>
    /// Suy ra số chữ số thập phân từ input để quyết định format và bước làm tròn tối thiểu.
    /// </summary>
    private static int DecimalDigitsFromInput(string raw)
    {
        string s = raw.Replace("\u00A0", "").Replace(" ", "");
        int lastComma = s.LastIndexOf(',');
        int lastDot = s.LastIndexOf('.');
        int lastSep = Math.Max(lastComma, lastDot);
        if (lastSep < 0) return 0;

        int digitsAfter = s.Length - lastSep - 1;
        int commaCount = s.Count(c => c == ',');
        int dotCount = s.Count(c => c == '.');

        // Nếu chỉ có 1 dấu và sau nó đúng 3 số → coi là dấu nghìn
        if ((commaCount + dotCount) == 1 && digitsAfter == 3) return 0;

        return Math.Max(0, digitsAfter);
    }

    /// <summary>
    /// Trả về cấu hình theo "unit" đã nhập. Thử:
    /// 1) Khớp đúng key trong CurrencyFormats (case-sensitive),
    /// 2) Khớp không phân biệt hoa thường,
    /// 3) Nếu "unit" là ISO code chữ cái (1–4) → cố gắng suy ra culture qua RegionInfo,
    /// 4) Fallback en-US.
    /// </summary>
    private static CurrencyFormat GetFormatForUnit(string unit)
    {
        if (!string.IsNullOrEmpty(unit))
        {
            if (CurrencyFormats.TryGetValue(unit, out var fmtExact))
                return Clone(fmtExact);

            // thử case-insensitive
            var kv = CurrencyFormats.FirstOrDefault(kv =>
                kv.Key.Equals(unit, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(kv.Key))
                return Clone(kv.Value);

            // Nếu là ISO-like (chỉ chữ, 1–4 ký tự) → thử resolve culture
            if (Regex.IsMatch(unit, @"^[A-Za-z]{1,4}$"))
            {
                var isoFmt = ResolveFormatFromIso(unit.ToUpperInvariant());
                if (isoFmt != null) return isoFmt;
            }
        }

        // Fallback chung
        return new CurrencyFormat
        {
            IsPrefix = true,
            HasSpace = true,
            Culture = "en-US",
            DecimalFormat = "N2",
            RoundingUnit = 0.01m
        };
    }

    /// <summary>
    /// Thử map ISO (USD, EUR, THB, JPY, ...) → chọn một culture phù hợp.
    /// Đồng thời set DecimalFormat/RoundingUnit phổ biến (ví dụ JPY/KRW/VND không có thập phân).
    /// </summary>
    private static CurrencyFormat ResolveFormatFromIso(string iso)
    {
        string culture = "en-US";
        try
        {
            foreach (var c in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                try
                {
                    var ri = new RegionInfo(c.Name);
                    if (ri.ISOCurrencySymbol.Equals(iso, StringComparison.OrdinalIgnoreCase))
                    {
                        culture = c.Name;
                        break;
                    }
                }
                catch { /* ignore */ }
            }
        }
        catch { /* ignore */ }

        // Các ISO hay không có thập phân
        var zeroDecimals = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "JPY", "KRW", "VND", "IDR", "CLP", "ISK", "HUF" };

        bool zero = zeroDecimals.Contains(iso);

        return new CurrencyFormat
        {
            IsPrefix = true,
            HasSpace = true,
            Culture = culture,
            DecimalFormat = zero ? "N0" : "N2",
            RoundingUnit = zero ? 1m : 0.01m
        };
    }

    private static CurrencyFormat Clone(CurrencyFormat x) => new CurrencyFormat
    {
        IsPrefix = x.IsPrefix,
        HasSpace = x.HasSpace,
        Culture = x.Culture,
        DecimalFormat = x.DecimalFormat,
        RoundingUnit = x.RoundingUnit
    };
}
