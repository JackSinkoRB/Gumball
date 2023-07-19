using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class CameraController : Singleton<CameraController>
    {
        
        [SerializeField] private Transform target;

        [SerializeField] private float moveSpeed = 5;
        [SerializeField] private float rotateSpeed = 5;

        public Transform Target => target;
        
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
        
        private void LateUpdate()
        {
            if (target == null) return;

            transform.position = Vector3.Lerp(transform.position, target.position, moveSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(target.position + target.forward - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }

    }
}
