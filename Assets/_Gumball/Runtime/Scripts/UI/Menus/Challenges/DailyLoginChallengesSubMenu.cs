using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class DailyLoginChallengesSubMenu : ChallengesSubMenu
    {

        [SerializeField] private DailyLoginDayNode[] dayNodes;
        [SerializeField] private GridLayoutWithScreenSize dayNodesGrid;

        protected override void OnShow()
        {
            base.OnShow();

            PopulateDayNodes();
        }

        private void PopulateDayNodes()
        {
            int dayNodeCount = 0;
            foreach (DailyLoginWeekProfile week in DailyLoginManager.Instance.CurrentMonth.Weeks)
            {
                foreach (MinorDailyLoginReward day in week.MinorRewards)
                {
                    DailyLoginDayNode dayNode = dayNodes[dayNodeCount];
                    dayNode.Initialise(dayNodeCount + 1, day);
                    dayNodeCount++;
                }
            }

            this.PerformAtEndOfFrame(() => dayNodesGrid.Resize());
        }

    }
}
