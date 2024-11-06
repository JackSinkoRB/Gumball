using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class DailyLoginTests : BaseRuntimeTests
    {

        private bool isInitialised;

        [SetUp]
        public void SetUp()
        {
            DataManager.RemoveAllData();
            CoroutineHelper.Instance.StartCoroutine(Initialise());
        }
        
        [TearDown]
        public void TearDown()
        {
            isInitialised = false;
            TimeUtils.SetTimeOffset(0);
        }

        private IEnumerator Initialise()
        {
            yield return DailyLoginManager.LoadInstanceAsync();
            isInitialised = true;
        }

        // [UnityTest]
        // [Order(1)]
        // public IEnumerator CurrentMonthLoops()
        // {
        //     yield return new WaitUntil(() => isInitialised);
        //
        //     TimeUtils.SetTime(0);
        //     
        //     const int iterations = 2;
        //     for (int count = 0; count < DailyLoginManager.Instance.MonthProfiles.Length * iterations; count++)
        //     {
        //         Assert.AreEqual(DailyLoginManager.Instance.MonthProfiles[count % DailyLoginManager.Instance.MonthProfiles.Length], DailyLoginManager.Instance.CurrentMonth);
        //         TimeUtils.AddTimeOffset(new TimeSpan(DailyLoginManager.DaysInMonth, 0, 0, 0));
        //     }
        // }
        //
        // [UnityTest]
        // [Order(2)]
        // public IEnumerator SecondsPassedInCurrentMonth()
        // {
        //     yield return new WaitUntil(() => isInitialised);
        //
        //     Stopwatch stopwatch = Stopwatch.StartNew();
        //     TimeUtils.SetTime(0);
        //     Assert.AreEqual(0, DailyLoginManager.Instance.SecondsPassedInCurrentMonth, Mathf.CeilToInt((float)stopwatch.Elapsed.TotalSeconds));
        //     
        //     stopwatch.Restart();
        //     TimeUtils.AddTimeOffset(TimeSpan.FromSeconds(100));
        //     Assert.AreEqual(100, DailyLoginManager.Instance.SecondsPassedInCurrentMonth, Mathf.CeilToInt((float)stopwatch.Elapsed.TotalSeconds));
        //     
        //     stopwatch.Restart();
        //     TimeUtils.SetTime(0);
        //     TimeUtils.AddTimeOffset(new TimeSpan(DailyLoginManager.DaysInMonth, 0, 0, 0));
        //     Assert.AreEqual(0, DailyLoginManager.Instance.SecondsPassedInCurrentMonth, Mathf.CeilToInt((float)stopwatch.Elapsed.TotalSeconds));
        //     
        //     stopwatch.Restart();
        //     TimeUtils.AddTimeOffset(TimeSpan.FromSeconds(100));
        //     Assert.AreEqual(100, DailyLoginManager.Instance.SecondsPassedInCurrentMonth, Mathf.CeilToInt((float)stopwatch.Elapsed.TotalSeconds));
        // }
        //
        // [UnityTest]
        // [Order(3)]
        // public IEnumerator DaysPassedInCurrentMonth()
        // {
        //     yield return new WaitUntil(() => isInitialised);
        //
        //     TimeUtils.SetTime(0);
        //     Assert.AreEqual(0, DailyLoginManager.Instance.DaysPassedInCurrentMonth);
        //
        //     TimeUtils.AddTimeOffset(new TimeSpan(1, 1, 0, 0));
        //     Assert.AreEqual(1, DailyLoginManager.Instance.DaysPassedInCurrentMonth);
        //     
        //     TimeUtils.SetTime(0);
        //     TimeUtils.AddTimeOffset(new TimeSpan(DailyLoginManager.DaysInMonth, 0, 0, 0));
        //     Assert.AreEqual(0, DailyLoginManager.Instance.DaysPassedInCurrentMonth);
        //     
        //     TimeUtils.AddTimeOffset(new TimeSpan(1, 1, 0, 0));
        //     Assert.AreEqual(1, DailyLoginManager.Instance.DaysPassedInCurrentMonth);
        // }

    }
}
