using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Random = UnityEngine.Random;

namespace Gumball.Runtime.Tests
{
    public class TimeUtilsTests : BaseRuntimeTests
    {
        
        [SetUp]
        public void SetUp()
        {
            DataManager.RemoveAllData();
        }
        
        [Test]
        public void SetTime()
        {
            long timeAtStart = TimeUtils.CurrentEpochSeconds;
            const int attempts = 100;
            for (int count = 0; count < attempts; count++)
            {
                const int randomAmount = 10000;
                int randomOffset = Random.Range(-randomAmount, randomAmount);
                long desiredTime = timeAtStart + randomOffset;
                Stopwatch stopwatch = Stopwatch.StartNew();
                TimeUtils.SetTime(desiredTime);
                long actualTime = TimeUtils.CurrentEpochSeconds;
                Assert.AreEqual(desiredTime, actualTime, Mathf.CeilToInt((float)stopwatch.Elapsed.TotalSeconds));
            }
        }
        
        [Test]
        public void TimeUtilsToPrettyStringMaxUnitOnly()
        {
            TimeSpan timeSpan = new TimeSpan(2, 2, 50, 35, 150);
            Assert.AreEqual("3 days", timeSpan.ToPrettyStringMaxUnitOnly());
            timeSpan = new TimeSpan(1, 2, 50, 35, 150);
            Assert.AreEqual("2 days", timeSpan.ToPrettyStringMaxUnitOnly());

            timeSpan = new TimeSpan(0, 23, 50, 35, 150);
            Assert.AreEqual("24 hours", timeSpan.ToPrettyStringMaxUnitOnly());
            timeSpan = new TimeSpan(0, 1, 50, 35, 150);
            Assert.AreEqual("2 hours", timeSpan.ToPrettyStringMaxUnitOnly());
            
            timeSpan = new TimeSpan(0, 0, 50, 35, 150);
            Assert.AreEqual("51 minutes", timeSpan.ToPrettyStringMaxUnitOnly());
            timeSpan = new TimeSpan(0, 0, 1, 35, 150);
            Assert.AreEqual("2 minutes", timeSpan.ToPrettyStringMaxUnitOnly());
            
            timeSpan = new TimeSpan(0, 0, 0, 35, 150);
            Assert.AreEqual("36 seconds", timeSpan.ToPrettyStringMaxUnitOnly());
            timeSpan = new TimeSpan(0, 0, 0, 1, 150);
            Assert.AreEqual("2 seconds", timeSpan.ToPrettyStringMaxUnitOnly());
            
            timeSpan = new TimeSpan(0, 0, 0, 0, 150);
            Assert.AreEqual("1 seconds", timeSpan.ToPrettyStringMaxUnitOnly());
        }
        
        [Test]
        public void TimeUtilsToPrettyStringShort()
        {
            TimeSpan timeSpan = new TimeSpan(0, 2, 50, 35, 150);
            Assert.AreEqual("2h 50m 35s", timeSpan.ToPrettyString());
        }
        
        [Test]
        public void TimeUtilsToPrettyStringShortWithMilliseconds()
        {
            TimeSpan timeSpan = new TimeSpan(0, 2, 50, 35, 150);
            Assert.AreEqual("2h 50m 35.15s", timeSpan.ToPrettyString(true));
        }
        
        [Test]
        public void TimeUtilsToPrettyStringLong()
        {
            TimeSpan timeSpan = new TimeSpan(0, 2, 50, 35, 150);
            Assert.AreEqual("2 Hours 50 Minutes 35 Seconds", timeSpan.ToPrettyString(longVersion: true));
        }
        
        [Test]
        public void TimeUtilsToPrettyStringLongWithMilliseconds()
        {
            TimeSpan timeSpan = new TimeSpan(0, 2, 50, 35, 150);
            Assert.AreEqual("2 Hours 50 Minutes 35.15 Seconds", timeSpan.ToPrettyString(true, longVersion: true));
        }
        
        [Test]
        public void TimeUtilsToPrettyStringNonPreciseMilliseconds()
        {
            TimeSpan timeSpan1 = new TimeSpan(0, 0, 0, 9, 160);
            Assert.AreEqual("9.1s", timeSpan1.ToPrettyString(true, false));
            
            TimeSpan timeSpan2 = new TimeSpan(0, 0, 0, 7, 60);
            Assert.AreEqual("7.0s", timeSpan2.ToPrettyString(true, false));
            
            TimeSpan timeSpan3 = new TimeSpan(0, 0, 0, 9, 990);
            Assert.AreEqual("9.9s", timeSpan3.ToPrettyString(true, false));
        }
        
        [Test]
        public void TimeUtilsToPrettyStringSecondsAndMilliseconds()
        {
            TimeSpan timeSpan = new TimeSpan(0, 0, 0, 3, 200);
            Assert.AreEqual("3.20s", timeSpan.ToPrettyString(true));
        }
        
        [Test]
        public void TimeUtilsToPrettyStringMilliseconds()
        {
            TimeSpan timeSpan = new TimeSpan(0, 0, 0, 0, 250);
            Assert.AreEqual("250ms", timeSpan.ToPrettyString(true));
        }
        
    }
}
