using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Gumball
{
    public static class PinchInput
    {
        
        /// <summary>
        /// Called when there are two input touches, and supplies the distance delta between the two touches (since there have been two touches).
        /// </summary>
        public static event Action<Vector2> onPinch;
        
        public static bool IsPinching { get; private set; }
        public static Vector2 TotalOffsetSincePinching { get; private set; }
        public static Vector2 AveragePositionDelta { get; private set; }

        private static Vector2 averagePositionLastFrame;
        private static Vector2 distanceBetweenTouchesWhenPinchStarted;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialise()
        {
            onPinch = null;
            IsPinching = false;
            TotalOffsetSincePinching = Vector2.zero;
            AveragePositionDelta = Vector2.zero;
            averagePositionLastFrame = Vector2.zero;
            distanceBetweenTouchesWhenPinchStarted = Vector2.zero;
        }
        
        public static void CheckForPinch()
        {
            if (onPinch == null)
                return; //only check for pinch if there are subscribers listening for the onPinch event

            bool isPinching = Touch.activeTouches.Count == 2;
            if (isPinching)
            {
                if (!IsPinching)
                    OnStartPinch();
                OnPinch();
            } else if (IsPinching)
            {
                OnStopPinch();
            }
        }
        
        private static void OnStartPinch()
        {
            IsPinching = true;
            distanceBetweenTouchesWhenPinchStarted = GetDistanceBetweenTouches();
            TotalOffsetSincePinching = Vector2.zero;
            averagePositionLastFrame = GetAveragePosition();
            AveragePositionDelta = Vector2.zero;
            
            GlobalLoggers.InputLogger.Log($"Pinch started - offset between touches {distanceBetweenTouchesWhenPinchStarted}");
        }

        private static void OnPinch()
        {
            Vector2 distanceBetweenTouchesThisFrame = GetDistanceBetweenTouches();
            Vector2 newTotalOffset = distanceBetweenTouchesThisFrame - distanceBetweenTouchesWhenPinchStarted;
            Vector2 offsetDifference = newTotalOffset - TotalOffsetSincePinching;
            
            //invoke pinch event
            onPinch?.Invoke(offsetDifference);
            
            //update the total distance
            TotalOffsetSincePinching = newTotalOffset;
            
            //update average position delta
            Vector2 averagePosition = GetAveragePosition();
            AveragePositionDelta = averagePosition - averagePositionLastFrame;
            averagePositionLastFrame = averagePosition;
            
            if (offsetDifference.sqrMagnitude > 0.01f)
                GlobalLoggers.InputLogger.Log($"Pinch moved: {offsetDifference} - current offset between touches = {distanceBetweenTouchesThisFrame}");
        }

        private static void OnStopPinch()
        {
            IsPinching = false;
            
            GlobalLoggers.InputLogger.Log("Stopped pinching");
        }

        /// <summary>
        /// Get the offset difference between the first two touches.
        /// </summary>
        private static Vector2 GetDistanceBetweenTouches()
        {
            Touch touchA = Touch.activeTouches[0];
            Touch touchB = Touch.activeTouches[1];
            Vector2 offset = touchA.screenPosition - touchB.screenPosition;
            Vector2 offsetAbs = new Vector2(Mathf.Abs(offset.x), Mathf.Abs(offset.y));
            return offsetAbs;
        }
        
        /// <summary>
        /// Gets the average position of the two touches used when pinching.
        /// </summary>
        private static Vector2 GetAveragePosition()
        {
            if (!IsPinching)
                return Vector2.zero;
            
            Touch touchA = Touch.activeTouches[0];
            Touch touchB = Touch.activeTouches[1];
            Vector2 offset = touchA.screenPosition + touchB.screenPosition;
            return offset / 2;
        }
        
    }
}
