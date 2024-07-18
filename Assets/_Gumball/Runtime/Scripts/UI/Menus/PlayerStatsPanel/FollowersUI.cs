using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class FollowersUI : MonoBehaviour
    {

        [SerializeField] private AutosizeTextMeshPro followersLabel;
        
        private void OnEnable()
        {
            RefreshFollowersLabel();

            FollowersManager.onFollowersChange += OnFollowersChange;
        }
        
        private void OnDisable()
        {
            FollowersManager.onFollowersChange -= OnFollowersChange;
        }
        
        private void OnFollowersChange(int previousFollowers, int newFollowers)
        {
            RefreshFollowersLabel();
        }
        
        private void RefreshFollowersLabel()
        {
            followersLabel.text = $"{FollowersManager.CurrentFollowers}";
            this.PerformAtEndOfFrame(followersLabel.Resize);
        }

    }
}
