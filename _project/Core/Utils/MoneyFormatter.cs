using System;
using System.Globalization;

namespace Core.Utils
{
    // Money is int64 paisa across every service (1 INR = 100 paisa). Centralize rendering
    // so we never accidentally treat a wallet field as rupees, and so dotted/comma locales
    // don't surprise us.
    public static class MoneyFormatter
    {
        public static string FormatPaisa(long paisa, string currency = "INR")
        {
            decimal rupees = paisa / 100m;
            return currency == "INR"
                ? $"₹{rupees.ToString("F2", CultureInfo.InvariantCulture)}"
                : $"{rupees.ToString("F2", CultureInfo.InvariantCulture)} {currency}";
        }

        public static string FormatPaisaRoundedRupees(long paisa)
        {
            long rupees = paisa / 100;
            return $"₹{rupees.ToString(CultureInfo.InvariantCulture)}";
        }

        public static bool TryParseDate(string iso, out DateTime localTime)
        {
            if (DateTime.TryParse(
                    iso,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var utc))
            {
                localTime = utc.ToLocalTime();
                return true;
            }
            localTime = default;
            return false;
        }
    }
}
