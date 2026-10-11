namespace AkarangaBank.Web.Services;

/// <summary>
/// Application time-zone helpers. Persist instants in UTC and use Bujumbura
/// civil time for display and day-based queries.
/// </summary>
public static class BankTime
{
    public static TimeZoneInfo TimeZone { get; } = ResolveTimeZone();

    public static DateTime Now => TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZone).DateTime;

    public static DateTime Today => Now.Date;

    /// <summary>
    /// Converts a persisted UTC timestamp to Bujumbura time. Database DateTime
    /// values often have Kind=Unspecified, so those are interpreted as UTC.
    /// </summary>
    public static DateTime ToLocal(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Local
            ? value.ToUniversalTime()
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZone);
    }

    public static (DateTime StartUtc, DateTime EndUtc) GetUtcDayRange(DateTime localDate)
    {
        var startLocal = DateTime.SpecifyKind(localDate.Date, DateTimeKind.Unspecified);
        var endLocal = startLocal.AddDays(1);
        return (
            TimeZoneInfo.ConvertTimeToUtc(startLocal, TimeZone),
            TimeZoneInfo.ConvertTimeToUtc(endLocal, TimeZone));
    }

    private static TimeZoneInfo ResolveTimeZone()
    {
        return TryFindTimeZone("Africa/Bujumbura")
            ?? TryFindTimeZone("South Africa Standard Time")
            ?? TimeZoneInfo.CreateCustomTimeZone(
                "Africa/Bujumbura",
                TimeSpan.FromHours(2),
                "Africa/Bujumbura",
                "Central Africa Time");
    }

    private static TimeZoneInfo? TryFindTimeZone(string id)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException)
        {
            return null;
        }
        catch (InvalidTimeZoneException)
        {
            return null;
        }
    }
}
