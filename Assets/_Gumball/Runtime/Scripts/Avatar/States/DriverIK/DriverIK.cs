using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class DriverIK : MonoBehaviour
    {
        
        [SerializeField] private IkChain leftArmChain;
        [SerializeField] private IkChain rightArmChain;
        [SerializeField] private IkChain leftLegChain;
        [SerializeField] private IkChain rightLegChain;
        
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
        
    }
}