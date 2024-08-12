using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class StanceModificationTests : IPrebuildSetup, IPostBuildCleanup
    {
        
        private const int carIndexToUse = 1; //test with the 911
        
        private bool isInitialised;

        public void Setup()
        {
            BootSceneClear.TrySetup();
            
            SingletonScriptableHelper.LazyLoadingEnabled = true;
        }

        public void Cleanup()
        {
            BootSceneClear.TryCleanup();
            
            SingletonScriptableHelper.LazyLoadingEnabled = false;
        }
        
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            DecalEditor.IsRunningTests = true;
            DataManager.EnableTestProviders(true);

            AsyncOperation loadWorkshopScene = EditorSceneManager.LoadSceneAsyncInPlayMode(TestManager.Instance.WorkshopScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            loadWorkshopScene.completed += OnSceneLoadComplete;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DataManager.EnableTestProviders(false);
            if (WarehouseManager.Instance.CurrentCar != null)
                Object.DestroyImmediate(WarehouseManager.Instance.CurrentCar.gameObject);
        }

        [SetUp]
        public void SetUp()
        {
            DataManager.RemoveAllData();
        }

        private void OnSceneLoadComplete(AsyncOperation asyncOperation)
        {
            CoroutineHelper.Instance.StartCoroutine(Initialise());
        }
        
        private IEnumerator Initialise()
        {
            //require the part managers to spawn the player car
            yield return CorePartManager.Initialise();
            yield return SubPartManager.Initialise();

            yield return WarehouseManager.Instance.SpawnCar(carIndexToUse, Vector3.zero, Quaternion.Euler(Vector3.zero),
                (carInstance) =>
                {
                    WarehouseManager.Instance.SetCurrentCar(carInstance);
                    isInitialised = true;
                });
        }
        
        [UnityTest]
        [Order(1)]
        public IEnumerator CarIsSetup()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.IsNotNull(WarehouseManager.Instance.CurrentCar);
            Assert.AreEqual(WarehouseManager.Instance.CurrentCar.CarIndex, carIndexToUse);

            //ensure all the wheels have the stance modification component
            const int numberOfWheels = 4;
            for (int index = 0; index < numberOfWheels; index++)
            {
                WheelCollider wheelCollider = WarehouseManager.Instance.CurrentCar.AllWheelColliders[index];
                StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();
                Assert.IsNotNull(stanceModification);
            }
        }
        
        [UnityTest]
        [Order(2)]
        public IEnumerator SuspensionHeight()
        {
            yield return new WaitUntil(() => isInitialised);

            const int indexToUse = 0;
            WheelCollider wheelCollider = WarehouseManager.Instance.CurrentCar.AllWheelColliders[indexToUse];
            StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();

            float valueToUse = stanceModification.SuspensionHeight.MinValue;
            stanceModification.ApplySuspensionHeight(valueToUse);
            Assert.AreEqual(valueToUse, wheelCollider.suspensionDistance);
            
            //check for persistency: reapply the saved data should be the same
            stanceModification.ApplySavedPlayerData();
            Assert.AreEqual(valueToUse, wheelCollider.suspensionDistance);
            
            //check default value works
            float defaultValue = stanceModification.SuspensionHeight.DefaultValue;
            stanceModification.ApplySuspensionHeight(defaultValue);
            Assert.AreEqual(defaultValue, wheelCollider.suspensionDistance);
        }
        
        [UnityTest]
        [Order(3)]
        public IEnumerator Camber()
        {
            yield return new WaitUntil(() => isInitialised);

            const int indexToUse = 0;
            WheelCollider wheelCollider = WarehouseManager.Instance.CurrentCar.AllWheelColliders[indexToUse];
            StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();

            float valueToUse = stanceModification.Camber.MaxValue;
            stanceModification.ApplyCamber(valueToUse);
            Assert.AreEqual(valueToUse, stanceModification.CurrentCamber);
            Assert.AreEqual(valueToUse.NormaliseAngle(), stanceModification.WheelMesh.transform.localRotation.eulerAngles.z.NormaliseAngle(), 0.01f);
            
            //check for persistency: reapply the saved data should be the same
            stanceModification.ApplySavedPlayerData();
            Assert.AreEqual(valueToUse, stanceModification.CurrentCamber);
            Assert.AreEqual(valueToUse.NormaliseAngle(), stanceModification.WheelMesh.transform.localRotation.eulerAngles.z.NormaliseAngle(), 0.01f);
            
            //check default value works
            float defaultValue = stanceModification.Camber.DefaultValue;
            stanceModification.ApplyCamber(defaultValue);
            Assert.AreEqual(defaultValue, stanceModification.CurrentCamber);
            Assert.AreEqual(defaultValue.NormaliseAngle(), stanceModification.WheelMesh.transform.localRotation.eulerAngles.z.NormaliseAngle(), 0.01f);
        }

        [UnityTest]
        [Order(4)]
        public IEnumerator Offset()
        {
            yield return new WaitUntil(() => isInitialised);

            const int indexToUse = 0;
            WheelCollider wheelCollider = WarehouseManager.Instance.CurrentCar.AllWheelColliders[indexToUse];
            StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();

            float valueToUse = stanceModification.Offset.MinValue;
            stanceModification.ApplyOffset(valueToUse);
            Assert.AreEqual(valueToUse, wheelCollider.transform.localPosition.x);
            
            //check for persistency: reapply the saved data should be the same
            stanceModification.ApplySavedPlayerData();
            Assert.AreEqual(valueToUse, wheelCollider.transform.localPosition.x);
            
            //check default value works
            float defaultValue = stanceModification.Offset.DefaultValue;
            stanceModification.ApplyOffset(defaultValue);
            Assert.AreEqual(defaultValue, wheelCollider.transform.localPosition.x);
        }
        
        [UnityTest]
        [Order(5)]
        public IEnumerator RimDiameter()
        {
            yield return new WaitUntil(() => isInitialised);

            const int indexToUse = 0;
            WheelCollider wheelCollider = WarehouseManager.Instance.CurrentCar.AllWheelColliders[indexToUse];
            StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();

            float valueToUse = stanceModification.RimDiameter.MinValue;
            stanceModification.ApplyRimDiameter(valueToUse);
            Assert.AreEqual(valueToUse, stanceModification.WheelMesh.transform.localScale.y);
            //ensure wheel collider radius changes
            Assert.AreEqual(wheelCollider.radius, GetMeshRadius(stanceModification.WheelMesh.Tyre.MeshFilter), 0.02f);
            
            //check for persistency: reapply the saved data should be the same
            stanceModification.ApplySavedPlayerData();
            Assert.AreEqual(valueToUse, stanceModification.WheelMesh.transform.localScale.y);

            //check default value works
            float defaultValue = stanceModification.RimDiameter.DefaultValue;
            stanceModification.ApplyRimDiameter(defaultValue);
            Assert.AreEqual(defaultValue, stanceModification.WheelMesh.transform.localScale.y);
            //ensure wheel collider radius changes
            Assert.AreEqual(wheelCollider.radius, GetMeshRadius(stanceModification.WheelMesh.Tyre.MeshFilter), 0.02f);
        }
        
        [UnityTest]
        [Order(6)]
        public IEnumerator RimWidth()
        {
            yield return new WaitUntil(() => isInitialised);

            const int indexToUse = 0;
            WheelCollider wheelCollider = WarehouseManager.Instance.CurrentCar.AllWheelColliders[indexToUse];
            StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();

            float valueToUse = stanceModification.RimWidth.MinValue;
            stanceModification.ApplyRimWidth(valueToUse);
            Assert.AreEqual(valueToUse, stanceModification.WheelMesh.transform.localScale.x);
            
            //check for persistency: reapply the saved data should be the same
            stanceModification.ApplySavedPlayerData();
            Assert.AreEqual(valueToUse, stanceModification.WheelMesh.transform.localScale.x);
            
            //check default value works
            float defaultValue = stanceModification.RimWidth.DefaultValue;
            stanceModification.ApplyRimWidth(defaultValue);
            Assert.AreEqual(defaultValue, stanceModification.WheelMesh.transform.localScale.x);
        }
        
        [UnityTest]
        [Order(7)]
        public IEnumerator TyreProfile()
        {
            yield return new WaitUntil(() => isInitialised);

            const int indexToUse = 0;
            WheelCollider wheelCollider = WarehouseManager.Instance.CurrentCar.AllWheelColliders[indexToUse];
            StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();

            float valueToUse = stanceModification.TyreProfile.MinValue;
            stanceModification.ApplyTyreProfile(valueToUse);
            Assert.AreEqual(valueToUse, stanceModification.WheelMesh.Tyre.transform.localScale.x);
            //ensure wheel collider radius changes
            Assert.AreEqual(wheelCollider.radius, GetMeshRadius(stanceModification.WheelMesh.Tyre.MeshFilter), 0.02f);
            
            //check for persistency: reapply the saved data should be the same
            stanceModification.ApplySavedPlayerData();
            Assert.AreEqual(valueToUse, stanceModification.WheelMesh.Tyre.transform.localScale.x);
            
            //check default value works
            float defaultValue = stanceModification.TyreProfile.DefaultValue;
            stanceModification.ApplyTyreProfile(defaultValue);
            Assert.AreEqual(defaultValue, stanceModification.WheelMesh.Tyre.transform.localScale.x);
            //ensure wheel collider radius changes
            Assert.AreEqual(wheelCollider.radius, GetMeshRadius(stanceModification.WheelMesh.Tyre.MeshFilter), 0.02f);
        }
        
        [UnityTest]
        [Order(8)]
        public IEnumerator TyreWidth()
        {
            yield return new WaitUntil(() => isInitialised);

            const int indexToUse = 0;
            WheelCollider wheelCollider = WarehouseManager.Instance.CurrentCar.AllWheelColliders[indexToUse];
            StanceModification stanceModification = wheelCollider.GetComponent<StanceModification>();

            float valueToUse = stanceModification.TyreWidth.MinValue;
            stanceModification.ApplyTyreWidth(valueToUse);
            Assert.AreEqual(valueToUse, stanceModification.WheelMesh.Tyre.transform.localScale.z);
            
            //check for persistency: reapply the saved data should be the same
            stanceModification.ApplySavedPlayerData();
            Assert.AreEqual(valueToUse, stanceModification.WheelMesh.Tyre.transform.localScale.z);
            
            //check default value works
            float defaultValue = stanceModification.TyreWidth.DefaultValue;
            stanceModification.ApplyTyreWidth(defaultValue);
            Assert.AreEqual(defaultValue, stanceModification.WheelMesh.Tyre.transform.localScale.z);
        }

        /// <summary>
        /// Gets the furthest vertex on the mesh and returns its distance/magnitude.
        /// </summary>
        private float GetMeshRadius(MeshFilter meshFilter)
        {
            int furthestVertexIndex = -1;
            float furthestDistanceSqr = 0;
            
            for (int vertexIndex = 0; vertexIndex < meshFilter.mesh.vertices.Length; vertexIndex++)
            {
                float distance = meshFilter.mesh.vertices[vertexIndex].sqrMagnitude;

                if (distance > furthestDistanceSqr)
                {
                    furthestDistanceSqr = distance;
                    furthestVertexIndex = vertexIndex;
                }
            }

            Vector3 vertexPositionWorld = meshFilter.transform.TransformPoint(meshFilter.mesh.vertices[furthestVertexIndex]);
            float radiusWorld = Vector3.Distance(meshFilter.transform.position, vertexPositionWorld);
            
            return radiusWorld;
        }
    }
}
