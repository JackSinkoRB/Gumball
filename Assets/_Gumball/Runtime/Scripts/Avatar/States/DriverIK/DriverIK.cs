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

        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool isDriver;
        [SerializeField, ReadOnly] private CarIKPositions currentPositions;
        
        //TODO: hands
        //public IkHandBones leftHand;
        //public IkHandBones rightHand;

        public void Initialise(bool isDriver, CarIKPositions positions)
        {
            this.isDriver = isDriver;
            currentPositions = positions;
            
            leftArmChain.Initialise(isDriver ? currentPositions.LeftHandPositionDriving : currentPositions.LeftHandPositionPassenger);
            rightArmChain.Initialise(isDriver ? currentPositions.RightHandPositionDriving : currentPositions.RightHandPositionPassenger);
            leftLegChain.Initialise(isDriver ? currentPositions.LeftFootPositionDriving : currentPositions.LeftFootPositionPassenger);
            rightLegChain.Initialise(isDriver ? currentPositions.RightFootPositionDriving : currentPositions.RightFootPositionPassenger);
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