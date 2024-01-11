using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Gumball.Runtime.Tests
{
    public class AvatarEditorTests : IPrebuildSetup, IPostBuildCleanup
    {

        private bool isInitialised;

        public void Setup()
        {
            BootSceneClear.TrySetup();
        }

        public void Cleanup()
        {
            BootSceneClear.TryCleanup();
        }
        
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            DataManager.EnableTestProviders(true);

            AsyncOperation loadMainScene = EditorSceneManager.LoadSceneAsyncInPlayMode(TestManager.Instance.AvatarEditorScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            loadMainScene.completed += OnSceneLoadComplete;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DataManager.EnableTestProviders(false);
        }

        [SetUp]
        public void SetUp()
        {
            DataManager.RemoveAllData();
        }
        
        private void OnSceneLoadComplete(AsyncOperation asyncOperation)
        {
            CoroutineHelper.Instance.StartCoroutine(SpawnAvatars());
        }

        private IEnumerator SpawnAvatars()
        {
            TrackedCoroutine driverAvatarLoadCoroutine = new TrackedCoroutine(AvatarManager.Instance.SpawnDriver(Vector3.zero, Quaternion.Euler(Vector3.zero)));
            TrackedCoroutine coDriverAvatarLoadCoroutine = new TrackedCoroutine(AvatarManager.Instance.SpawnCoDriver(Vector3.zero, Quaternion.Euler(Vector3.zero)));
            
            yield return new WaitUntil(() => !driverAvatarLoadCoroutine.IsPlaying && !coDriverAvatarLoadCoroutine.IsPlaying);
            
            isInitialised = true;
        }
        
        [UnityTest]
        public IEnumerator AvatarEditorIsSetup()
        {
            yield return new WaitUntil(() => isInitialised);
            Assert.IsTrue(AvatarEditor.ExistsRuntime);
        }
        
        [UnityTest]
        public IEnumerator AllDriversAndBodiesExistDuringEditorButNotOutside()
        {
            yield return new WaitUntil(() => isInitialised);

            yield return AvatarEditor.Instance.StartSession();
            
            Assert.IsNotNull(AvatarManager.Instance.DriverAvatar);
            Assert.IsNotNull(AvatarManager.Instance.CoDriverAvatar);
            
            Assert.IsNotNull(AvatarManager.Instance.DriverAvatar.CurrentMaleBody);
            Assert.IsNotNull(AvatarManager.Instance.DriverAvatar.CurrentFemaleBody);
            Assert.IsNotNull(AvatarManager.Instance.CoDriverAvatar.CurrentMaleBody);
            Assert.IsNotNull(AvatarManager.Instance.CoDriverAvatar.CurrentFemaleBody);
            
            AvatarEditor.Instance.EndSession();
            
            Assert.IsNotNull(AvatarManager.Instance.DriverAvatar);
            Assert.IsNotNull(AvatarManager.Instance.CoDriverAvatar);
            
            //ensure they have the default body type as none has been set
            Assert.AreEqual(Avatar.DefaultBodyType, AvatarManager.Instance.DriverAvatar.CurrentBody.BodyType);
            Assert.AreEqual(Avatar.DefaultBodyType, AvatarManager.Instance.CoDriverAvatar.CurrentBody.BodyType);

            AvatarBody driverBody = Avatar.DefaultBodyType == AvatarBodyType.MALE ? AvatarManager.Instance.DriverAvatar.CurrentMaleBody : AvatarManager.Instance.DriverAvatar.CurrentFemaleBody;
            AvatarBody driverBodyNotUsed = Avatar.DefaultBodyType == AvatarBodyType.MALE ? AvatarManager.Instance.DriverAvatar.CurrentFemaleBody : AvatarManager.Instance.DriverAvatar.CurrentMaleBody;
            Assert.IsNotNull(driverBody);
            Assert.IsNull(driverBodyNotUsed);
            
            AvatarBody coDriverBody = Avatar.DefaultBodyType == AvatarBodyType.MALE ? AvatarManager.Instance.CoDriverAvatar.CurrentMaleBody : AvatarManager.Instance.CoDriverAvatar.CurrentFemaleBody;
            AvatarBody coDriverBodyNotUsed = Avatar.DefaultBodyType == AvatarBodyType.MALE ? AvatarManager.Instance.CoDriverAvatar.CurrentFemaleBody : AvatarManager.Instance.CoDriverAvatar.CurrentMaleBody;
            Assert.IsNotNull(coDriverBody);
            Assert.IsNull(coDriverBodyNotUsed);
        }

        [UnityTest]
        public IEnumerator StartWithDriverSelected()
        {
            yield return new WaitUntil(() => isInitialised);

            yield return AvatarEditor.Instance.StartSession();
            
            Assert.AreEqual(AvatarManager.Instance.DriverAvatar, AvatarEditor.Instance.CurrentSelectedAvatar);
        }
        
        [UnityTest]
        public IEnumerator BodyTypeCosmeticIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);

            yield return AvatarEditor.Instance.StartSession();
            
            Avatar avatarToCheck = AvatarEditor.Instance.CurrentSelectedAvatar;
            Assert.AreEqual(Avatar.DefaultBodyType, avatarToCheck.CurrentBody.BodyType);

            const int indexToUse = 1;
            BodyTypeCosmetic bodyTypeCosmetic = avatarToCheck.CurrentBody.GetCosmetic<BodyTypeCosmetic>();
            bodyTypeCosmetic.Apply(indexToUse);

            Assert.AreEqual(bodyTypeCosmetic.Options[indexToUse].Type, avatarToCheck.CurrentBody.BodyType);

            AvatarEditor.Instance.EndSession();

            Assert.AreEqual(bodyTypeCosmetic.Options[indexToUse].Type, avatarToCheck.CurrentBody.BodyType);
        }
        
        [UnityTest]
        public IEnumerator SkinCosmeticIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);

            yield return AvatarEditor.Instance.StartSession();
            
            Avatar avatarToCheck = AvatarEditor.Instance.CurrentSelectedAvatar;
            Assert.AreEqual(Avatar.DefaultBodyType, avatarToCheck.CurrentBody.BodyType);

            const int indexToUse = 5;
            SkinColourCosmetic skinCosmetic = avatarToCheck.CurrentBody.GetCosmetic<SkinColourCosmetic>();
            skinCosmetic.Apply(indexToUse);
            
            AvatarEditor.Instance.EndSession();

            bool allMaterialsHaveColour = true;
            foreach (Material material in skinCosmetic.MaterialsToEffect)
            {
                if (material.GetColor(SkinColourCosmetic.SkinTintProperty) != skinCosmetic.Colors[indexToUse])
                {
                    allMaterialsHaveColour = false;
                    break;
                }
            }
            
            Assert.IsTrue(allMaterialsHaveColour);
        }

    }
}
