using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class DailyLoginChallengesSubMenu : ChallengesSubMenu
    {

        [SerializeField] private RectTransform contentHolder;
        [SerializeField] private float contentHolderExtraHeight = 15;
        [SerializeField] private DailyLoginMinorRewardNode[] minorRewardNodes;
        [SerializeField] private GridLayoutWithScreenSize minorRewardsGrid;
        [SerializeField] private RectTransform majorRewardsHolder;
        [SerializeField] private DailyLoginMajorRewardNode majorReward1;
        [SerializeField] private DailyLoginMajorRewardNode majorReward2;
        [SerializeField] private DailyLoginMajorRewardNode majorReward3;
        [SerializeField] private DailyLoginMajorRewardNode majorReward4;
        
        protected override void OnShow()
        {
            base.OnShow();

            DailyLoginManager.onCurrentMonthChange += OnCurrentMonthChange;
            
            RefreshNodes();
        }

        protected override void OnHide()
        {
            base.OnHide();
            
            DailyLoginManager.onCurrentMonthChange -= OnCurrentMonthChange;
        }

        public void RefreshNodes()
        {
            PopulateMinorRewards();
            PopulateMajorRewards();
            
            //set content size to the grid size for scrolling
            contentHolder.sizeDelta = contentHolder.sizeDelta.SetY(minorRewardsGrid.GetComponent<RectTransform>().sizeDelta.y + contentHolderExtraHeight);
        }

        private void PopulateMinorRewards()
        {
            int dayNodeCount = 0;
            int dayNumber = 1;
            foreach (DailyLoginWeekProfile week in DailyLoginManager.Instance.CurrentMonth.Weeks)
            {
                foreach (MinorDailyLoginReward day in week.MinorRewards)
                {
                    DailyLoginMinorRewardNode minorRewardNode = minorRewardNodes[dayNodeCount];
                    minorRewardNode.Initialise(dayNumber, day);
                    dayNodeCount++;
                    dayNumber++;
                }

                dayNumber++; //add 1 at end of week for the major rewards
            }

            Canvas.ForceUpdateCanvases();

            minorRewardsGrid.Resize();
        }

        private void PopulateMajorRewards()
        {
            //set the size as the same as the minor rewards grid to set inline
            majorRewardsHolder.sizeDelta = majorRewardsHolder.sizeDelta.SetY(minorRewardsGrid.GetComponent<RectTransform>().sizeDelta.y);

            //copy the minor reward positions
            majorReward1.GetComponent<RectTransform>().anchoredPosition = majorReward1.GetComponent<RectTransform>().anchoredPosition.SetY(minorRewardNodes[0].GetComponent<RectTransform>().anchoredPosition.y);
            majorReward2.GetComponent<RectTransform>().anchoredPosition = majorReward2.GetComponent<RectTransform>().anchoredPosition.SetY(minorRewardNodes[7].GetComponent<RectTransform>().anchoredPosition.y);
            majorReward3.GetComponent<RectTransform>().anchoredPosition = majorReward3.GetComponent<RectTransform>().anchoredPosition.SetY(minorRewardNodes[14].GetComponent<RectTransform>().anchoredPosition.y);
            majorReward4.GetComponent<RectTransform>().anchoredPosition = majorReward4.GetComponent<RectTransform>().anchoredPosition.SetY(minorRewardNodes[21].GetComponent<RectTransform>().anchoredPosition.y);
            
            //set the heights
            float sizeY = minorRewardNodes[0].GetComponent<RectTransform>().sizeDelta.y; //all nodes have same height
            majorReward1.GetComponent<RectTransform>().sizeDelta = majorReward1.GetComponent<RectTransform>().sizeDelta.SetY(sizeY);
            majorReward2.GetComponent<RectTransform>().sizeDelta = majorReward2.GetComponent<RectTransform>().sizeDelta.SetY(sizeY);
            majorReward3.GetComponent<RectTransform>().sizeDelta = majorReward3.GetComponent<RectTransform>().sizeDelta.SetY(sizeY);
            majorReward4.GetComponent<RectTransform>().sizeDelta = majorReward4.GetComponent<RectTransform>().sizeDelta.SetY(sizeY);
                
            //update the major rewards
            majorReward1.Initialise(7, DailyLoginManager.Instance.CurrentMonth.Weeks[0].MajorReward);
            majorReward2.Initialise(14, DailyLoginManager.Instance.CurrentMonth.Weeks[1].MajorReward);
            majorReward3.Initialise(21, DailyLoginManager.Instance.CurrentMonth.Weeks[2].MajorReward);
            majorReward4.Initialise(28, DailyLoginManager.Instance.CurrentMonth.Weeks[3].MajorReward);
        }
        
        private void OnCurrentMonthChange(int previousMonthIndex, int newMonthIndex)
        {
            RefreshNodes();
        }
        
    }
}
