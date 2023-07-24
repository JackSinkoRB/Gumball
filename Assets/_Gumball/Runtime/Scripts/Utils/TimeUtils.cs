using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class TimeUtils
{
    
    public const long MillisecondsInSecond = 1000;
    public const long SecondsInAMinute = 60;
    public const long SecondsInAnHour = SecondsInAMinute * 60;
    public const long SecondsInADay = SecondsInAnHour * 24;

    private static readonly StringBuilder stringBuilder = new();
    
    public static string ToPrettyString(this TimeSpan timeSpan, bool includeMs = false, bool longVersion = false)
    {
        stringBuilder.Clear();
        bool showMinutes = timeSpan.Minutes > 0;
        bool showSeconds = timeSpan.Seconds > 0;

        if (timeSpan.Hours > 0)
        {
            stringBuilder.Append(timeSpan.Hours).Append(longVersion ? " Hours" : "h");
            if (showMinutes || showSeconds) stringBuilder.Append(" ");
        }

        if (showMinutes)
        {
            stringBuilder.Append(timeSpan.Minutes).Append(longVersion ? " Minutes" : "m");
            if (showSeconds) stringBuilder.Append(" ");
        }

        if (showSeconds || includeMs)
        {
            if (includeMs)
            {
                float roundedDownSeconds = Mathf.FloorToInt(timeSpan.Seconds);
                float ms = (timeSpan - TimeSpan.FromSeconds(roundedDownSeconds)).Milliseconds;

                if (roundedDownSeconds > 0)
                {
                    //showing seconds AND ms
                    int msAsPercent = Mathf.RoundToInt((ms / MillisecondsInSecond) * 100f);
                    stringBuilder.Append($"{roundedDownSeconds}.{msAsPercent}{(longVersion ? " Seconds" : "s")}");
                }
                else
                {
                    //just showing ms
                    stringBuilder.Append($"{ms}ms");
                }
            }
            else
            {
                stringBuilder.Append(timeSpan.Seconds).Append(longVersion ? " Seconds" : "s");
            }
        }

        if (stringBuilder.Length == 0)
        {
            stringBuilder.Append(0).Append(longVersion ? " Seconds" : "s");
        }

        return stringBuilder.ToString();
    }
    
}
