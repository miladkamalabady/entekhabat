namespace EntekhabatApi.Services;

/// <summary>
/// تبدیل شمسی↔میلادی — پیاده‌سازی مستقیم از jdf.php نسخه 2.55
/// </summary>
public class JalaliService
{
    private static readonly string[] MonthNames =
    {
        "فروردین","اردیبهشت","خرداد","تیر","مرداد","شهریور",
        "مهر","آبان","آذر","دی","بهمن","اسفند"
    };
    private static readonly string[] DayNames =
    {
        "یکشنبه","دوشنبه","سه‌شنبه","چهارشنبه","پنجشنبه","جمعه","شنبه"
    };

    // ─── Format (معادل jdate) ──────────────────────────────────────

    public string Format(DateTime dt, string fmt)
    {
        GregorianToJalali(dt.Year, dt.Month, dt.Day, out int jy, out int jm, out int jd);
        int dow = ((int)dt.DayOfWeek + 1) % 7; // شنبه=0

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

    public string FormatShort(DateTime dt)    => Format(dt, "H:i Y-n-j");
    public string FormatDate(DateTime dt)     => Format(dt, "Y/m/d");
    public string FormatDateTime(DateTime dt) => Format(dt, "H:i Y/m/d");

    // ─── NormalizeToGregorian ──────────────────────────────────────

    // MySqlConnector تاریخ‌های شمسی ذخیره شده در DATETIME را به DateTime تبدیل می‌کند
    // باید آن را به‌عنوان شمسی تفسیر و به میلادی تبدیل کنیم
    public DateTime? NormalizeToGregorian(object? value)
    {
        if (value == null || value is DBNull) return null;
        if (value is DateTime dt)
            // InvariantCulture ضروری است: در locale فارسی، ToString() شمسی برمی‌گرداند
            return NormalizeToGregorian(dt.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
        return NormalizeToGregorian(value.ToString());
    }

    public DateTime? NormalizeToGregorian(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        value = value.Replace("T", " ").Trim();
        var parts = value.Split(' ', 2);
        var datePart = parts[0];
        var timePart = parts.Length > 1 ? parts[1] : "00:00:00";
        if (timePart.Length == 5) timePart += ":00";

        var seg = datePart.Split('-', '/');
        if (seg.Length != 3) return null;
        if (!int.TryParse(seg[0], out int y) ||
            !int.TryParse(seg[1], out int m) ||
            !int.TryParse(seg[2], out int d)) return null;

        if (!TimeSpan.TryParse(timePart, out var time)) time = TimeSpan.Zero;

        if (y < 1700) // شمسی
        {
            var gd = JalaliToGregorianDate(y, m, d);
            if (gd == null) return null;
            try { return new DateTime(gd.Value.gy, gd.Value.gm, gd.Value.gd, time.Hours, time.Minutes, time.Seconds); }
            catch { return null; }
        }

        try { return new DateTime(y, m, d, time.Hours, time.Minutes, time.Seconds); }
        catch { return null; }
    }

    public DateTime? ParseJalaliSchedule(string? dateStr) => NormalizeToGregorian(dateStr);

    // ─── jalali_to_gregorian (معادل مستقیم jdf.php) ───────────────

    public static (int gy, int gm, int gd)? JalaliToGregorianDate(int jy, int jm, int jd)
    {
        try
        {
            int d4   = (jy + 1) % 4;
            int doyJ = jm < 7 ? (jm - 1) * 31 + jd : (jm - 7) * 30 + jd + 186;
            int d33  = (int)(((jy - 55) % 132) * 0.0305);
            int a    = (d33 != 3 && d4 <= d33) ? 287 : 286;
            int b    = ((d33 == 1 || d33 == 2) && (d33 == d4 || d4 == 1)) ? 78
                      : ((d33 == 3 && d4 == 0) ? 80 : 79);
            if ((jy - 19) / 63 == 20) { a--; b++; }

            int gy, gd2;
            if (doyJ <= a) { gy = jy + 621; gd2 = doyJ + b; }
            else           { gy = jy + 622; gd2 = doyJ - a; }

            // تعیین ماه و روز میلادی
            int[] mo = { 0, 31, gy % 4 == 0 ? 29 : 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            int gm2 = 0;
            for (gm2 = 0; gm2 < mo.Length; gm2++)
            {
                if (gd2 <= mo[gm2]) break;
                gd2 -= mo[gm2];
            }
            return (gy, gm2, gd2);
        }
        catch { return null; }
    }

    // ─── gregorian_to_jalali (معادل مستقیم jdf.php) ───────────────

    public static void GregorianToJalali(int gy, int gm, int gd, out int jy, out int jm, out int jd)
    {
        int d4 = gy % 4;
        int[] ga = { 0, 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 };
        int doyG = ga[gm] + gd;
        if (d4 == 0 && gm > 2) doyG++;

        int d33 = (int)(((gy - 16) % 132) * 0.0305);
        int a   = (d33 == 3 || d33 < d4 - 1 || d4 == 0) ? 286 : 287;
        int b   = ((d33 == 1 || d33 == 2) && (d33 == d4 || d4 == 1)) ? 78
                 : ((d33 == 3 && d4 == 0) ? 80 : 79);
        if ((gy - 10) / 63 == 30) { a--; b++; }

        int doyJ;
        if (doyG > b) { jy = gy - 621; doyJ = doyG - b; }
        else          { jy = gy - 622; doyJ = doyG + a; }

        if (doyJ < 187)
        {
            jm  = (doyJ - 1) / 31;
            jd  = doyJ - 31 * jm;
            jm++;
        }
        else
        {
            jm  = (doyJ - 187) / 30;
            jd  = doyJ - 186 - jm * 30;
            jm += 7;
        }
    }
}
