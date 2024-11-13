using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [ExecuteAlways]
    public class DriverIK : MonoBehaviour
    {
        
        [SerializeField] private Transform pelvis;
        [SerializeField] private IkChain leftArmChain;
        [SerializeField] private IkChain rightArmChain;
        [SerializeField] private IkChain leftLegChain;
        [SerializeField] private IkChain rightLegChain;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private bool isInitialised;

        private IKPositionsInCar positions;
        
        public void Initialise(IKPositionsInCar positions)
        {
            this.positions = positions;
            
            enabled = true;
            isInitialised = true;

            leftArmChain.Initialise(positions.LeftHand);
            rightArmChain.Initialise(positions.RightHand);
            leftLegChain.Initialise(positions.LeftFoot);
            rightLegChain.Initialise(positions.RightFoot);
        }

        private void OnDisable()
        {
            leftArmChain.ResetPositions();
            rightArmChain.ResetPositions();
            leftLegChain.ResetPositions();
            rightLegChain.ResetPositions();
        }

        private void LateUpdate()
        {
            if (isInitialised)
                UpdateIkChains();
        }

        private void UpdateIkChains()
        {
            pelvis.transform.localRotation = positions.Pelvis.localRotation; //apply the additional pelvis rotation
            
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
        
    }
}