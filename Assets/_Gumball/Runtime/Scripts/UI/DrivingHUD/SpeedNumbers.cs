using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class SpeedNumbers : MonoBehaviour
    {

        [SerializeField] private GameObject numbersKmh;
        [SerializeField] private GameObject numbersMph;

        private void LateUpdate()
        {
            if (UnitOfSpeedSetting.UseMiles && (!numbersMph.activeSelf || numbersKmh.activeSelf))
            {
                numbersKmh.gameObject.SetActive(false);
                numbersMph.gameObject.SetActive(true);
            }
            else if (!UnitOfSpeedSetting.UseMiles && (!numbersKmh.activeSelf || numbersMph.activeSelf))
            {
                numbersKmh.gameObject.SetActive(true);
                numbersMph.gameObject.SetActive(false);
            }
        }
        
    }
}
