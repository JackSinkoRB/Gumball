using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorSpeedRandomiser : MonoBehaviour
    {

        [SerializeField] private MinMaxFloat randomSpeed = new(0.95f, 1.05f);

        private Animator animator => GetComponent<Animator>();
        
        private void OnEnable()
        {
            animator.speed = randomSpeed.RandomInRange();
        }
        
    }
}
