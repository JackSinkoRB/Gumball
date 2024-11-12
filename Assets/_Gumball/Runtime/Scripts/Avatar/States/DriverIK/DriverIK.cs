using System;
using System.Collections;
using System.Collections.Generic;
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

        private IKPositionsInCar positions;
        private Quaternion initialPelvisRotation;
        
        public void Initialise(IKPositionsInCar positions)
        {
            this.positions = positions;
            
            enabled = true;

            initialPelvisRotation = pelvis.transform.rotation;

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
            UpdateIkChains();
        }

        private void UpdateIkChains()
        {
            pelvis.transform.rotation = initialPelvisRotation * positions.Pelvis.rotation; //apply the additional pelvis rotation
            
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