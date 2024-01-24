using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using CC;
using MyBox;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
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
            Assert.AreEqual(AvatarManager.DefaultDriverBodyType, AvatarManager.Instance.DriverAvatar.CurrentBody.BodyType);
            Assert.AreEqual(AvatarManager.DefaultCoDriverBodyType, AvatarManager.Instance.CoDriverAvatar.CurrentBody.BodyType);

            AvatarBody driverBody = AvatarManager.DefaultDriverBodyType == AvatarBodyType.MALE ? AvatarManager.Instance.DriverAvatar.CurrentMaleBody : AvatarManager.Instance.DriverAvatar.CurrentFemaleBody;
            AvatarBody driverBodyNotUsed = AvatarManager.DefaultDriverBodyType == AvatarBodyType.MALE ? AvatarManager.Instance.DriverAvatar.CurrentFemaleBody : AvatarManager.Instance.DriverAvatar.CurrentMaleBody;
            Assert.IsNotNull(driverBody);
            Assert.IsNull(driverBodyNotUsed);
            
            AvatarBody coDriverBody = AvatarManager.DefaultCoDriverBodyType == AvatarBodyType.MALE ? AvatarManager.Instance.CoDriverAvatar.CurrentMaleBody : AvatarManager.Instance.CoDriverAvatar.CurrentFemaleBody;
            AvatarBody coDriverBodyNotUsed = AvatarManager.DefaultCoDriverBodyType == AvatarBodyType.MALE ? AvatarManager.Instance.CoDriverAvatar.CurrentFemaleBody : AvatarManager.Instance.CoDriverAvatar.CurrentMaleBody;
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
            Assert.AreEqual(AvatarManager.DefaultDriverBodyType, avatarToCheck.CurrentBody.BodyType);

            const int indexToUse = 1;
            BodyTypeCosmetic bodyTypeCosmeticBefore = avatarToCheck.CurrentBody.GetCosmetic<BodyTypeCosmetic>();
            bodyTypeCosmeticBefore.Apply(indexToUse);

            Assert.AreEqual(bodyTypeCosmeticBefore.Options[indexToUse].Type, avatarToCheck.CurrentBody.BodyType);

            AvatarEditor.Instance.EndSession();

            BodyTypeCosmetic bodyTypeCosmeticAfter = avatarToCheck.CurrentBody.GetCosmetic<BodyTypeCosmetic>();
            Assert.AreEqual(bodyTypeCosmeticAfter.Options[indexToUse].Type, avatarToCheck.CurrentBody.BodyType);
            
            //ensure it is saved in persistent data
            Assert.AreEqual(indexToUse, bodyTypeCosmeticAfter.GetSavedIndex());
            
            //ensure it is current
            Assert.AreEqual(indexToUse, bodyTypeCosmeticAfter.CurrentIndex);
        }
        
        [UnityTest]
        public IEnumerator SkinCosmeticIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);

            yield return AvatarEditor.Instance.StartSession();
            
            Avatar avatarToCheck = AvatarEditor.Instance.CurrentSelectedAvatar;

            const int indexToUse = 5;
            SkinColourCosmetic skinCosmetic = avatarToCheck.CurrentBody.GetCosmetic<SkinColourCosmetic>();
            skinCosmetic.Apply(indexToUse);
            
            AvatarEditor.Instance.EndSession();

            foreach (Material material in skinCosmetic.GetMaterialsToEffect())
            {
                Color actualColor = material.GetColor(SkinColourCosmetic.SkinTintProperty);
                Color desiredColor = skinCosmetic.SkinColors[indexToUse];
                
                if (actualColor != desiredColor)
                    Assert.Fail($"{material.name} ({actualColor.ToString()}) doesn't match (should be {desiredColor.ToString()})");
            }
            
            //ensure it is saved in persistent data
            Assert.AreEqual(indexToUse, skinCosmetic.GetSavedIndex());
            
            //ensure it is current
            Assert.AreEqual(indexToUse, skinCosmetic.CurrentIndex);
        }
        
        [UnityTest]
        public IEnumerator FrecklesCosmeticIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);

            yield return AvatarEditor.Instance.StartSession();
            
            Avatar avatarToCheck = AvatarEditor.Instance.CurrentSelectedAvatar;

            const int indexToUse = 2;
            FrecklesCosmetic frecklesCosmetic = avatarToCheck.CurrentBody.GetCosmetic<FrecklesCosmetic>();
            frecklesCosmetic.Apply(indexToUse);
            
            AvatarEditor.Instance.EndSession();

            foreach (Material material in frecklesCosmetic.MaterialsToEffect)
            {
                float actualValue = material.GetFloat(FrecklesCosmetic.FrecklesProperty);
                float desiredValue = frecklesCosmetic.Options[indexToUse].Value;
                
                if (!actualValue.Approximately(desiredValue))
                    Assert.Fail($"{material.name} ({actualValue}) doesn't match (should be {desiredValue})");
            }
            
            //ensure it is saved in persistent data
            Assert.AreEqual(indexToUse, frecklesCosmetic.GetSavedIndex());
            
            //ensure it is current
            Assert.AreEqual(indexToUse, frecklesCosmetic.CurrentIndex);
        }
        
        [UnityTest]
        public IEnumerator ItemCosmeticIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);

            yield return AvatarEditor.Instance.StartSession();
            
            Avatar avatarToCheck = AvatarEditor.Instance.CurrentSelectedAvatar;

            const int indexToUse = 2;
            //just use the upper body cosmetic for testing
            UpperBodyCosmetic upperBodyCosmetic = avatarToCheck.CurrentBody.GetCosmetic<UpperBodyCosmetic>();
            upperBodyCosmetic.Apply(indexToUse);
            
            AvatarEditor.Instance.EndSession();

            Assert.IsNotNull(upperBodyCosmetic.CurrentItem);
            
            string nameOfItemInList = upperBodyCosmetic.Items[indexToUse].Prefab.editorAsset.name;
            string nameOfCurrentItem = upperBodyCosmetic.CurrentItem.name.Replace("(Clone)", "");
            Assert.IsTrue(nameOfItemInList.Equals(nameOfCurrentItem));
            
            //ensure it is saved in persistent data
            Assert.AreEqual(indexToUse, upperBodyCosmetic.GetSavedIndex());
            
            //ensure it is current
            Assert.AreEqual(indexToUse, upperBodyCosmetic.CurrentIndex);
        }

        [UnityTest]
        public IEnumerator ItemColorIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);
        
            yield return AvatarEditor.Instance.StartSession();
            
            Avatar avatarToCheck = AvatarEditor.Instance.CurrentSelectedAvatar;
        
            const int indexToUse = 5;
            //just use hair as item cosmetic
            HairCosmetic hairCosmetic = avatarToCheck.CurrentBody.GetCosmetic<HairCosmetic>();
            hairCosmetic.ApplyColor(indexToUse);
            
            AvatarEditor.Instance.EndSession();
        
            foreach (Material material in hairCosmetic.GetMaterialsWithColorProperty())
            {
                foreach (string property in hairCosmetic.CurrentItemData.Colorable.ColorMaterialProperties)
                {
                    Color actualColor = material.GetColor(property);
                    Color desiredColor = hairCosmetic.CurrentItemData.Colorable.Colors[indexToUse];
                    
                    if (actualColor != desiredColor)
                        Assert.Fail($"{property} ({actualColor}) doesn't match (should be {desiredColor})");
                }
            }
            
            //ensure it is saved in persistent data
            Assert.AreEqual(indexToUse, hairCosmetic.GetSavedColorIndex());
            
            //ensure it is current
            Assert.AreEqual(indexToUse, hairCosmetic.CurrentColorIndex);
        }
        
        [UnityTest]
        public IEnumerator BlendShapeCosmeticIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);

            yield return AvatarEditor.Instance.StartSession();
            
            Avatar avatarToCheck = AvatarEditor.Instance.CurrentSelectedAvatar;

            const int indexToUse = 4;
            //just use head shape
            HeadShapeCosmetic headShapeCosmetic = avatarToCheck.CurrentBody.GetCosmetic<HeadShapeCosmetic>();
            headShapeCosmetic.Apply(indexToUse);
            
            AvatarEditor.Instance.EndSession();

            BlendshapeManager[] managers = avatarToCheck.CurrentBody.GetBlendShapeManagers();
            Assert.IsTrue(managers != null && managers.Length > 0);

            foreach (BlendshapeCosmetic.PropertyModifier propertyModifier in headShapeCosmetic.Options[indexToUse].PropertyModifiers)
            {
                float actualValue = managers[0].GetBlendshape(propertyModifier.Property);
                float desiredValue = propertyModifier.Value * BlendshapeManager.BlendShapeValueModifier;
                
                if (!actualValue.Approximately(desiredValue))
                    Assert.Fail($"{propertyModifier.Property} ({actualValue}) doesn't match (should be {desiredValue})");
            }
            
            //ensure it is saved in persistent data
            Assert.AreEqual(indexToUse, headShapeCosmetic.GetSavedIndex());
            
            //ensure it is current
            Assert.AreEqual(indexToUse, headShapeCosmetic.CurrentIndex);
        }
        
        [UnityTest]
        public IEnumerator BlendShapeColorIsPersistent()
        {
            yield return new WaitUntil(() => isInitialised);
        
            yield return AvatarEditor.Instance.StartSession();
            
            Avatar avatarToCheck = AvatarEditor.Instance.CurrentSelectedAvatar;
        
            const int indexToUse = 5;
            //just use mouth as blendshape cosmetic
            MouthCosmetic mouthCosmetic = avatarToCheck.CurrentBody.GetCosmetic<MouthCosmetic>();
            mouthCosmetic.ApplyColor(indexToUse);
            
            AvatarEditor.Instance.EndSession();
        
            foreach (string colorProperty in mouthCosmetic.Colorable.ColorMaterialProperties)
            {
                foreach (Material material in avatarToCheck.CurrentBody.GetMaterialsWithProperty(colorProperty))
                {
                    Color actualColor = material.GetColor(colorProperty);
                    Color desiredColor = mouthCosmetic.Colorable.Colors[indexToUse];
                    
                    if (actualColor != desiredColor)
                        Assert.Fail($"{colorProperty} ({actualColor}) doesn't match (should be {desiredColor})");
                }
            }
            
            //ensure it is saved in persistent data
            Assert.AreEqual(indexToUse, mouthCosmetic.GetSavedColorIndex());
            
            //ensure it is current
            Assert.AreEqual(indexToUse, mouthCosmetic.CurrentColorIndex);
        }
        
    }
}
