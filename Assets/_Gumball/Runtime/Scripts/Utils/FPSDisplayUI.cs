
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gumball
{
    public class FPSDisplayUI : MonoBehaviour
    {
        
        [SerializeField] private TextMeshProUGUI label;

        private readonly Dictionary<int, string> cachedNumberStrings = new();
        private int[] _frameRateSamples;
        private int _cacheNumbersAmount = 300;
        private int _averageFromAmount = 30;
        private int _averageCounter = 0;
        private int _currentAveraged;
 
        private void Awake()
        {
            CacheCollections();
        }
        
        private void CacheCollections()
        {
            for (int i = 0; i < _cacheNumbersAmount; i++)
                cachedNumberStrings[i] = i.ToString();

            _frameRateSamples = new int[_averageFromAmount];
        }
        
        private void Update()
        {
            //sample
            int currentFrame = (int)Math.Round(1f / Time.smoothDeltaTime);
            _frameRateSamples[_averageCounter] = currentFrame;

            //average
            float average = 0f;

            foreach (int frameRate in _frameRateSamples)
                average += frameRate;

            _currentAveraged = (int)Math.Round(average / _averageFromAmount);
            _averageCounter = (_averageCounter + 1) % _averageFromAmount;

            //update label
            label.text = _currentAveraged switch
            {
                var x when x >= 0 && x < _cacheNumbersAmount => $"FPS: {cachedNumberStrings[x]}",
                var x when x >= _cacheNumbersAmount => $"FPS: >{_cacheNumbersAmount}",
                var x when x < 0 => "FPS: <0",
                _ => "?"
            };
        }
    }
}
