using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class CollisionCarAudio : CarAudio
    {
        
        public override void Initialise(CarAudioManager managerBelongsTo)
        {
            base.Initialise(managerBelongsTo);
            
            source.playOnAwake = false;
            gameObject.SetActive(true);
            
            if (managerBelongsTo.CarBelongsTo.IsPlayer)
            {
                GameSession.onSessionStart += OnSessionStart;
                GameSession.onSessionEnd += OnSessionEnd;
            }
        }
        
        private void OnDestroy()
        {
            GameSession.onSessionStart -= OnSessionStart;
            GameSession.onSessionEnd -= OnSessionEnd;

            if (managerBelongsTo != null && managerBelongsTo.CarBelongsTo != null)
            {
                managerBelongsTo.CarBelongsTo.onCollisionEnter -= OnCollision;
            }
        }

        private void OnSessionStart(GameSession session)
        {
            managerBelongsTo.CarBelongsTo.onCollisionEnter += OnCollision;
        }

        private void OnSessionEnd(GameSession session, GameSession.ProgressStatus progress)
        {
            managerBelongsTo.CarBelongsTo.onCollisionEnter -= OnCollision;
        }

        private void OnCollision(Collision collision)
        {
            const float maxCollisionImpulseAllowed = 500;

            float magnitudeSqr = collision.impulse.sqrMagnitude;
            float maxMagnitudeSqrRequired = maxCollisionImpulseAllowed * maxCollisionImpulseAllowed;

            if (magnitudeSqr >= maxMagnitudeSqrRequired)
                source.Play();
        }
        
    }
}
