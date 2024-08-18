using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Gumball
{
    public class FrameUI : MonoBehaviour
    {

        [SerializeField] private Image left;
        [SerializeField] private Image right;
        [SerializeField] private Image top;
        [SerializeField] private Image bottom;
        
        [SerializeField] private float maxRandomRotation = 4;
        
        private void OnEnable()
        {
            Randomise();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
                Randomise();
        }

        private void Randomise()
        {
            const float defaultVerticalRotation = 90;
            left.transform.rotation = Quaternion.Euler(left.transform.rotation.eulerAngles.SetZ(Random.Range(defaultVerticalRotation-maxRandomRotation, defaultVerticalRotation+maxRandomRotation)));
            right.transform.rotation = Quaternion.Euler(right.transform.rotation.eulerAngles.SetZ(Random.Range(defaultVerticalRotation-maxRandomRotation, defaultVerticalRotation+maxRandomRotation)));
            
            top.transform.rotation = Quaternion.Euler(top.transform.rotation.eulerAngles.SetZ(Random.Range(-maxRandomRotation, maxRandomRotation)));
            bottom.transform.rotation = Quaternion.Euler(bottom.transform.rotation.eulerAngles.SetZ(Random.Range(-maxRandomRotation, maxRandomRotation)));
        }
        
    }
}
