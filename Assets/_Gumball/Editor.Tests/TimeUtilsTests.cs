using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Gumball.Editor.Tests
{
    public class TimeUtilsTests
    {

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
            Assert.AreEqual("2 Hours 50 Minutes 35.15 Seconds", timeSpan.ToPrettyString(true, true));
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
