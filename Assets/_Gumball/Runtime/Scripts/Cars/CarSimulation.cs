using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MyBox;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gumball
{
    [RequireComponent(typeof(AICar))]
    public class CarSimulation : MonoBehaviour
    {

        private static Scene physicsCalculationsScene;
        private static GameObject plane;

        [SerializeField, ReadOnly] private float topSpeed;
        [SerializeField, ReadOnly] private float zeroTo100Time;
        
        private AICar car => GetComponent<AICar>();
        
        public bool IsSimulating { get; private set; }

        [ButtonMethod]
        public void CalculateTopSpeed()
        {
            if (!Application.isPlaying)
                throw new InvalidOperationException("Game must be playing to simulate.");
            
            Stopwatch stopwatch = Stopwatch.StartNew();

            topSpeed = 0;
            zeroTo100Time = 0;
            
            //create a physics scene and cache it if not already
            if (!physicsCalculationsScene.IsValid())
            {
                physicsCalculationsScene = UnityEngine.SceneManagement.SceneManager.CreateScene("PhysicsCalculations", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
                if (plane != null)
                    Destroy(plane);
                plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(plane, physicsCalculationsScene);
                const float planeSize = 100000;
                plane.transform.localScale = Vector3.one * planeSize;
                plane.transform.position = new Vector3(0, -1, 0);
            }
            plane.SetActive(true);
            
            IsSimulating = true;
            
            //move the car
            car.Rigidbody.constraints = RigidbodyConstraints.None;
            car.Rigidbody.isKinematic = false;
            Scene originalScene = gameObject.scene;
            Vector3 originalPosition = transform.position;
            Vector3 originalVelocity = car.Rigidbody.velocity;
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gameObject, physicsCalculationsScene);
            transform.position = new Vector3(0, 1, 0);
            car.Rigidbody.Move(new Vector3(0, 1, 0), Quaternion.Euler(Vector3.zero));

            GlobalLoggers.LoadingLogger.Log($"Setup took {stopwatch.Elapsed.ToPrettyString(true)}.");
            stopwatch.Restart();
            
            //simulate
            Physics.simulationMode = SimulationMode.Script;
            const int maxFrames = 2000;
            float totalTime = 0;
            for (int frame = 0; frame <= maxFrames; frame++)
            {
                car.SimulateMovement();
                physicsCalculationsScene.GetPhysicsScene().Simulate(Time.fixedDeltaTime);
                totalTime += Time.fixedDeltaTime;

                if (car.Speed > 100 && zeroTo100Time == 0)
                    zeroTo100Time = totalTime;
            }
            IsSimulating = false;

            topSpeed = car.Speed;
            
            GlobalLoggers.LoadingLogger.Log($"Simulation took {stopwatch.Elapsed.ToPrettyString(true)}.");
            stopwatch.Restart();

            //restore
            if (originalScene.IsValid())
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gameObject, originalScene);
            else DontDestroyOnLoad(gameObject);
            
            transform.position = originalPosition;
            car.Rigidbody.Move(originalPosition, Quaternion.Euler(Vector3.zero));
            car.Rigidbody.velocity = originalVelocity;
            Physics.simulationMode = SimulationMode.FixedUpdate;
            
            plane.SetActive(false);

            GlobalLoggers.LoadingLogger.Log($"Cleanup took {stopwatch.Elapsed.ToPrettyString(true)}.");
        }
        
    }
}
