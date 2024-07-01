using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    /// <summary>
    /// To be applied to a IK position for additional settings.
    /// </summary>
    public class IKPositionData : MonoBehaviour
    {

        [Tooltip("Does the last bone of the chain match this rotation? This is useful if the target is moving.")]
        [SerializeField] private bool endBoneCopiesRotation;
        
        public bool EndBoneCopiesRotation => endBoneCopiesRotation;

    }
}
