using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MagneticScrollUtils
{
    [RequireComponent(typeof(ImageOffset))]
    public class MoveWithMagneticScroll : MonoBehaviour
    {

        [FormerlySerializedAs("modifier")]
        [SerializeField] private float speedModifier = 1;

        [SerializeField] private MagneticScroll magneticScroll;

        private ImageOffset imageOffset;

        private void Awake()
        {
            imageOffset ??= GetComponent<ImageOffset>();
        }

        private void OnEnable()
        {
            magneticScroll.OnMoveAllItems += OnMoveAllItems;
        }

        private void OnDisable()
        {
            magneticScroll.OnMoveAllItems -= OnMoveAllItems;
        }

        private void OnMoveAllItems(Vector2 amount)
        {
            imageOffset.Offset += amount * speedModifier;
        }

    }
}