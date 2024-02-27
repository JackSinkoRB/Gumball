using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    //Based off https://www.youtube.com/watch?v=qqOAzn05fvk
    public class DriverIK : MonoBehaviour
    {
        
        [SerializeField] private IkChain leftArmChain;
        [SerializeField] private IkChain rightArmChain;
        [SerializeField] private IkChain leftLegChain;
        [SerializeField] private IkChain rightLegChain;

        //TODO: hands
        //public IkHandBones leftHand;
        //public IkHandBones rightHand;

        public void Initialise(IKPositionsInCar positions)
        {
            leftArmChain.Initialise(positions.LeftHand);
            rightArmChain.Initialise(positions.RightHand);
            leftLegChain.Initialise(positions.LeftFoot);
            rightLegChain.Initialise(positions.RightFoot);
        }

        private void LateUpdate()
        {
            UpdateIkChains();
        }

        private void UpdateIkChains()
        {
            leftArmChain.ResolveIK();
            rightArmChain.ResolveIK();
            leftLegChain.ResolveIK();
            rightLegChain.ResolveIK();
        }

        private void OnDrawGizmos()
        {
            leftArmChain.DrawGizmos();
            rightArmChain.DrawGizmos();
            leftLegChain.DrawGizmos();
            rightLegChain.DrawGizmos();
        }

        //TODO: hands
        // internal void SetHand(IkHandBones hand, string positionName)
        // {
        //     //Maybe store a dictionary for these
        //     for (int i = 0; i < fingerReference.positions.Length; ++i)
        //     {
        //         if (fingerReference.positions[i].left != hand.isLeft || !string.Equals(positionName, fingerReference.positions[i].name)) continue;
        //
        //         for (int h = 0; h < hand.thumb.Length; ++h)
        //             hand.thumb[h].localRotation = fingerReference.positions[i].thumb[h];
        //
        //         for (int h = 0; h < hand.fingers.Length; ++h)
        //             hand.fingers[h].localRotation = fingerReference.positions[i].fingers[h];
        //
        //         return;
        //     }
        // }
    }
}