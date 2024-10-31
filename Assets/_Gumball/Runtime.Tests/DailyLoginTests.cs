using System;
using System.Collections;
using System.Collections.Generic;
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

        [UnityTest]
        [Order(1)]
        public IEnumerator CurrentMonthLoops()
        {
            yield return new WaitUntil(() => isInitialised);

            TimeUtils.SetTime(0);
            
            const int iterations = 2;
            for (int count = 0; count < DailyLoginManager.Instance.MonthProfiles.Length * iterations; count++)
            {
                LogAssert.Expect(LogType.Error, "Cannot get server time because it hasn't been retrieved.");
                Assert.AreEqual(DailyLoginManager.Instance.MonthProfiles[count % DailyLoginManager.Instance.MonthProfiles.Length], DailyLoginManager.Instance.CurrentMonth);
                TimeUtils.AddTimeOffset(new TimeSpan(DailyLoginManager.DaysInMonth, 0, 0, 0));
            }
        }
        
    }
}
