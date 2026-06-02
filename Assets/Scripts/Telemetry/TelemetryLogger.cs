using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public static class TelemetryLogger
{
    private static string matchId = "not_started";
    private static bool isAbnormal = false;

    public static void BeginMatch(string newMatchId)
    {
        matchId = string.IsNullOrWhiteSpace(newMatchId) ? "unknown" : Sanitize(newMatchId);
        isAbnormal = false;
    }

    public static void SetAbnormal(bool value)
    {
        isAbnormal = value;
    }

    public static string GetObjectId(UnityEngine.Object value)
    {
        return value != null ? Sanitize(value.name) : "unknown";
    }

    public static void Emit(string eventName, Dictionary<string, object> payload)
    {
        StringBuilder line = new StringBuilder();
        line.Append("[Telemetry] event=");
        line.Append(Sanitize(eventName));
        line.Append(" | match_id=");
        line.Append(matchId);

        if (payload != null)
        {
            foreach (KeyValuePair<string, object> pair in payload)
            {
                if (pair.Key == "match_id" || pair.Key == "is_abnormal" || pair.Key == "ts")
                {
                    continue;
                }

                line.Append(" | ");
                line.Append(Sanitize(pair.Key));
                line.Append("=");
                line.Append(FormatValue(pair.Value));
            }
        }

        line.Append(" | is_abnormal=");
        line.Append(isAbnormal ? "true" : "false");
        line.Append(" | ts=");
        line.Append((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        Debug.Log(line.ToString());
    }

    private static string FormatValue(object value)
    {
        if (value == null) return "null";
        if (value is string text) return Sanitize(text);
        if (value is bool flag) return flag ? "true" : "false";
        if (value is float floatValue) return floatValue.ToString("0.###", CultureInfo.InvariantCulture);
        if (value is double doubleValue) return doubleValue.ToString("0.###", CultureInfo.InvariantCulture);
        if (value is decimal decimalValue) return decimalValue.ToString(CultureInfo.InvariantCulture);
        if (value is sbyte || value is byte ||
            value is short || value is ushort ||
            value is int || value is uint ||
            value is long || value is ulong)
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        if (value is IEnumerable values)
        {
            List<string> formattedValues = new List<string>();

            foreach (object item in values)
            {
                formattedValues.Add(FormatValue(item));
            }

            return "[" + string.Join(",", formattedValues) + "]";
        }

        return "unsupported";
    }

    private static string Sanitize(string value)
    {
        if (string.IsNullOrEmpty(value)) return "unknown";

        return value
            .Replace("|", "/")
            .Replace("\r", " ")
            .Replace("\n", " ");
    }
}
