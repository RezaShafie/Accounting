using System.Globalization;

namespace Accounting.Web.Extensions;

public static class DateExtensions
{
    private static readonly PersianCalendar Pc = new();

    public static string ToPersianDateString(this DateOnly date)
    {
        // Convert DateOnly to DateTime for PersianCalendar compatibility
        var dt = date.ToDateTime(TimeOnly.MinValue);

        var year = Pc.GetYear(dt);
        var month = Pc.GetMonth(dt);
        var day = Pc.GetDayOfMonth(dt);

        // Returns format: 1402/05/12
        return $"{year}/{month:00}/{day:00}";
    }

    public static string ToPersianDateString(this DateTime date)
    {
        var year = Pc.GetYear(date);
        var month = Pc.GetMonth(date);
        var day = Pc.GetDayOfMonth(date);

        return $"{year}/{month:00}/{day:00}";
    }
}