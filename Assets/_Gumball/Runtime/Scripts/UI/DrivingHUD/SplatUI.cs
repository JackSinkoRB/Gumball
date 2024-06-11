using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Gumball
{
    [RequireComponent(typeof(Image))]
    public class SplatUI : MonoBehaviour
    {
        
        [SerializeField] private Sprite[] splatSprites;
        [SerializeField] private float timeBetweenSpriteChange = 0.1f;

        private Cooldown spriteChangeCooldown;
        
        private Image image => GetComponent<Image>();

        private void Start()
        {
            spriteChangeCooldown = new Cooldown(timeBetweenSpriteChange, false);
            
            UpdateSplat();
        }

        private void Update()
        {
            if (spriteChangeCooldown.IsReady)
            {
                UpdateSplat();
                spriteChangeCooldown.Reset();
            }
        }

        private void UpdateSplat()
        {
            image.sprite = splatSprites.GetRandom();
            
            image.rectTransform.Rotate(0, 0, Random.Range(-180f, 180f));
        }
        
    }
}
