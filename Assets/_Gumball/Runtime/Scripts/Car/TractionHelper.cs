using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    // This script is meant to help set up controllable cars.
    // What it does is to detect situations where the car is fishtailing, and in that case, remove grip 
    // from the front wheels. This will cause the car to slip more in the front then in the rear,
    // thus recovering from the oversteer.

    // This is not quite physically realitic, and may cause the gameplay to feel somewhat more acrade
    // like, but is similar to how ESP systems work in real life
    // (those will just apply the brakes to remove grip from wheels).
    public class TractionHelper : MonoBehaviour
    {
        // assign car's front wheels here.
        public Wheel[] front;

        private Rigidbody _rb;

        // how strong oversteer is compensated for
        public float compensationFactor = 0.1f;

        // state
        private float oldGrip;
        private float angle;
        private float angularVelo;

        private void Start()
        {
            oldGrip = front[0].grip;
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            Vector3 driveDir = transform.forward;
            Vector3 veloDir = _rb.velocity;
            veloDir -= transform.up * Vector3.Dot(veloDir, transform.up);
            veloDir.Normalize();

            angle = -Mathf.Asin(Vector3.Dot(Vector3.Cross(driveDir, veloDir), transform.up));

            angularVelo = _rb.angularVelocity.y;

            foreach (Wheel w in front)
            {
                if (angle * w.steering < 0)
                    w.grip = oldGrip * (1.2f - Mathf.Clamp01(compensationFactor * Mathf.Abs(angularVelo)));
                else
                    w.grip = oldGrip;
            }
        }
    }
}