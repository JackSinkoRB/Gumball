using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Gumball
{
    public static class TimeUtils
    {

        public static long CurrentEpochMilliseconds => DateTimeOffset.Now.ToUnixTimeMilliseconds() + TimeOffsetSeconds * MillisecondsInSecond;
        public static long CurrentEpochSeconds => DateTimeOffset.Now.ToUnixTimeSeconds() + TimeOffsetSeconds;

        public const long MillisecondsInSecond = 1000;
        public const long SecondsInAMinute = 60;
        public const long SecondsInAnHour = SecondsInAMinute * 60;
        public const long SecondsInADay = SecondsInAnHour * 24;

        private static readonly StringBuilder stringBuilder = new();
        
        /// <summary>
        /// How much 'fake time' is applied to the time values in this class.
        /// </summary>
        public static long TimeOffsetSeconds
        {
            get => DataManager.Settings.Get("Cheats.TimeOffset", 0);
            set => DataManager.Settings.Set("Cheats.TimeOffset", value);
        }
        
        /// <summary>
        /// Add/take some 'fake time'. Useful for testing by skipping time.
        /// </summary>
        public static void SetTimeOffset(TimeSpan timeSpan)
        {
            TimeOffsetSeconds = (long)timeSpan.TotalSeconds;
        }

        /// <summary>
        /// Add/take some 'fake time'. Useful for testing by skipping time.
        /// </summary>
        public static void SetTimeOffset(long epochSeconds)
        {
            TimeOffsetSeconds = epochSeconds;
        }
        
        public static void AddTimeOffset(TimeSpan timeSpan)
        {
            SetTimeOffset(TimeOffsetSeconds + (long)timeSpan.TotalSeconds);
        }

        /// <summary>
        /// Reset the fake time to bring the game back into real time.
        /// </summary>
        public static void ResetTimeOffset()
        {
            TimeOffsetSeconds = 0;
        }

        /// <summary>
        /// Set the time to an epoch timestamp (in seconds).
        /// </summary>
        public static void SetTime(long epochSeconds)
        {
            ResetTimeOffset();
            long currentTime = CurrentEpochSeconds;
            long difference = epochSeconds - currentTime;
            SetTimeOffset(difference);
        }

        public static string ToPrettyStringMaxUnitOnly(this TimeSpan timeSpan)
        {
            int daysRounded = Mathf.FloorToInt((float)timeSpan.TotalDays);
            int hoursRounded = Mathf.FloorToInt((float)timeSpan.TotalHours);
            int minutesRounded = Mathf.FloorToInt((float)timeSpan.TotalMinutes);
            int secondsRounded = Mathf.FloorToInt((float)timeSpan.TotalSeconds);

            if (daysRounded > 0)
                return $"{daysRounded + 1} days";
            if (hoursRounded > 0)
                return $"{hoursRounded + 1} hours";
            if (minutesRounded > 0)
                return $"{minutesRounded + 1} minutes";
            if (secondsRounded > 0)
                return $"{secondsRounded + 1} seconds";
            
            return "1 seconds";
        }

        public static string ToPrettyString(this TimeSpan timeSpan, bool includeMs = false, bool precise = true, bool longVersion = false)
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

                    if (!precise || roundedDownSeconds > 0)
                    {
                        //showing seconds AND ms
                        int msAsPercent = precise
                            ? Mathf.RoundToInt((ms / MillisecondsInSecond) * 100f)
                            : Mathf.FloorToInt((ms / MillisecondsInSecond) * 10f);
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
}