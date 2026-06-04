using System.Globalization;

namespace EntekhabatApi.Services;

/// <summary>
/// Converts between Jalali (Shamsi) and Gregorian dates.
/// Equivalent of jdf.php in the PHP codebase.
/// </summary>
public class JalaliService
{
    private readonly PersianCalendar _persian = new();

    private static readonly string[] MonthNames =
    {
        "فروردین","اردیبهشت","خرداد","تیر","مرداد","شهریور",
        "مهر","آبان","آذر","دی","بهمن","اسفند"
    };

    private static readonly string[] DayNames =
    {
        "یکشنبه","دوشنبه","سه‌شنبه","چهارشنبه","پنجشنبه","جمعه","شنبه"
    };

    // jdate() equivalent: format a DateTime as Jalali string
    // Supported tokens: Y n j m d H i s F l
    public string Format(DateTime dt, string fmt)
    {
        int jy = _persian.GetYear(dt);
        int jm = _persian.GetMonth(dt);
        int jd = _persian.GetDayOfMonth(dt);
        // Saturday=6 Gregorian → index 0 in Persian week
        int dow = ((int)dt.DayOfWeek + 1) % 7;

        // Replace longest tokens first to avoid double replacement
        return fmt
            .Replace("H", dt.Hour.ToString("D2"))
            .Replace("i", dt.Minute.ToString("D2"))
            .Replace("s", dt.Second.ToString("D2"))
            .Replace("Y", jy.ToString("D4"))
            .Replace("m", jm.ToString("D2"))
            .Replace("d", jd.ToString("D2"))
            .Replace("n", jm.ToString())
            .Replace("j", jd.ToString())
            .Replace("F", MonthNames[jm - 1])
            .Replace("l", DayNames[dow]);
    }

    public string FormatShort(DateTime dt) => Format(dt, "H:i Y-n-j");
    public string FormatDate(DateTime dt) => Format(dt, "Y/m/d");
    public string FormatDateTime(DateTime dt) => Format(dt, "H:i Y/m/d");
    public string FormatLong(DateTime dt) => Format(dt, "l j F Y");

    // normalizeToGregorianDateTime() equivalent
    // Accepts Jalali OR Gregorian strings like "1402-10-15 08:00" or "2024-01-05 08:00"
    public DateTime? NormalizeToGregorian(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        value = value.Replace("T", " ").Trim();
        var parts = value.Split(' ', 2);
        var datePart = parts[0];
        var timePart = parts.Length > 1 ? parts[1] : "00:00:00";

        if (timePart.Length == 5) timePart += ":00";

        var seg = datePart.Split('-');
        if (seg.Length != 3) return null;
        if (!int.TryParse(seg[0], out int y) ||
            !int.TryParse(seg[1], out int m) ||
            !int.TryParse(seg[2], out int d)) return null;

        if (!TimeSpan.TryParse(timePart, out var time)) time = TimeSpan.Zero;

        if (y < 1700) // Jalali
        {
            try { return _persian.ToDateTime(y, m, d, time.Hours, time.Minutes, time.Seconds, 0); }
            catch { return null; }
        }

        try { return new DateTime(y, m, d, time.Hours, time.Minutes, time.Seconds); }
        catch { return null; }
    }

    // jalali_to_gregorian() equivalent (used for FinalSubmit schedule checking)
    public (int gy, int gm, int gd) JalaliToGregorian(int jy, int jm, int jd)
    {
        try
        {
            var dt = _persian.ToDateTime(jy, jm, jd, 0, 0, 0, 0);
            return (dt.Year, dt.Month, dt.Day);
        }
        catch { return (0, 0, 0); }
    }

    public DateTime? ParseJalaliSchedule(string? dateStr)
    {
        if (string.IsNullOrWhiteSpace(dateStr)) return null;
        var sp = dateStr.Split(' ', 2);
        if (sp.Length != 2) return null;
        var dateSeg = sp[0].Split('-');
        if (dateSeg.Length != 3) return null;
        if (!int.TryParse(dateSeg[0], out int jy) ||
            !int.TryParse(dateSeg[1], out int jm) ||
            !int.TryParse(dateSeg[2], out int jd)) return null;
        if (!TimeSpan.TryParse(sp[1], out var t)) t = TimeSpan.Zero;
        try { return _persian.ToDateTime(jy, jm, jd, t.Hours, t.Minutes, t.Seconds, 0); }
        catch { return null; }
    }
}
