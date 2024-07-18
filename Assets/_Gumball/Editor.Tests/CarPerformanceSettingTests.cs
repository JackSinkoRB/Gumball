using System.Collections;
using System.Collections.Generic;
using MyBox;
using NUnit.Framework;
using UnityEngine;

namespace Gumball.Editor.Tests
{
    public class CarPerformanceSettingTests
    {

        [Test]
        public void FloatSetting()
        {
            CarPerformanceSettingFloat setting = new CarPerformanceSettingFloat(100, 200);
            setting.SetWeights(0.5f, 0.5f, 0, 0); //nos and handling have 0 effect
            
            CarPerformanceProfile profile1 = new CarPerformanceProfile(0, 0, 1, 1);
            Assert.AreEqual(100, setting.GetValue(profile1));
            
            CarPerformanceProfile profile2 = new CarPerformanceProfile(1, 0, 1, 1);
            Assert.AreEqual(150, setting.GetValue(profile2));
            
            CarPerformanceProfile profile3 = new CarPerformanceProfile(1, 1, 1, 1);
            Assert.AreEqual(200, setting.GetValue(profile3));
        }
        
        [Test]
        public void MinMaxFloatSetting()
        {
            CarPerformanceSettingMinMaxFloat setting = new CarPerformanceSettingMinMaxFloat(new MinMaxFloat(100, 200), new MinMaxFloat(200, 300));
            setting.SetWeights(0.5f, 0, 0.5f, 0); //nos and acceleration have 0 effect
            
            CarPerformanceProfile profile1 = new CarPerformanceProfile(0, 1, 0, 1);
            Assert.AreEqual(new MinMaxFloat(100, 200), setting.GetValue(profile1));
            
            CarPerformanceProfile profile2 = new CarPerformanceProfile(1, 1, 0, 1);
            Assert.AreEqual(new MinMaxFloat(150, 250), setting.GetValue(profile2));
            
            CarPerformanceProfile profile3 = new CarPerformanceProfile(1, 1, 1, 1);
            Assert.AreEqual(new MinMaxFloat(200, 300), setting.GetValue(profile3));
        }
        
        [Test]
        public void FloatScalarSetting()
        {
            CarPerformanceSettingFloatScalar setting = new CarPerformanceSettingFloatScalar(100);
            setting.SetWeights(0, 1, 0, 1); //maxspeed and handling have 0 effect
            
            CarPerformanceProfile profile1 = new CarPerformanceProfile(1, 0, 1, 0);
            Assert.AreEqual(0, setting.GetValue(profile1));
            
            CarPerformanceProfile profile2 = new CarPerformanceProfile(0, 0.2f, 0, 0);
            Assert.AreEqual(10, setting.GetValue(profile2));
            
            CarPerformanceProfile profile3 = new CarPerformanceProfile(1, 1, 1, 1);
            Assert.AreEqual(100, setting.GetValue(profile3));
        }
        
        [Test]
        public void PercentSetting()
        {
            CarPerformanceSettingPercent setting = new CarPerformanceSettingPercent(0.5f);
            setting.SetWeights(0, 1, 0, 0); //maxspeed, nos and handling have 0 effect
            
            CarPerformanceProfile profile1 = new CarPerformanceProfile(1, 0, 0, 0);
            Assert.AreEqual(0, setting.GetValue(profile1));
            
            CarPerformanceProfile profile2 = new CarPerformanceProfile(1, 0.5f, 1, 1);
            Assert.AreEqual(0.25f, setting.GetValue(profile2));
            
            CarPerformanceProfile profile3 = new CarPerformanceProfile(1, 1, 1, 1);
            Assert.AreEqual(0.5f, setting.GetValue(profile3));
        }
        
        [Test]
        public void FloatArraySetting()
        {
            CarPerformanceSettingFloatArray setting = new CarPerformanceSettingFloatArray(new float[]{0, 10, 100}, new float[]{10, 20, 300});
            setting.SetWeights(0, 1, 0, 1); //maxspeed and handling have 0 effect
            
            CarPerformanceProfile profile1 = new CarPerformanceProfile(1, 0, 1, 0);
            Assert.AreEqual(new float[]{0, 10, 100}, setting.GetValue(profile1));
            
            CarPerformanceProfile profile2 = new CarPerformanceProfile(0, 0.5f, 0, 0.5f);
            Assert.AreEqual(new float[]{5, 15, 200}, setting.GetValue(profile2));
            
            CarPerformanceProfile profile3 = new CarPerformanceProfile(1, 1, 1, 1);
            Assert.AreEqual(new float[]{10, 20, 300}, setting.GetValue(profile3));
        }
        
        [Test]
        public void AnimationCurveSetting()
        {
            AnimationCurve minCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 2), new Keyframe(1, 3));
            AnimationCurve maxCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 4), new Keyframe(1, 6));
            
            CarPerformanceSettingAnimationCurve setting = new CarPerformanceSettingAnimationCurve(minCurve, maxCurve);
            setting.SetWeights(1, 1, 0, 0); //maxspeed and handling have 0 effect
            
            CarPerformanceProfile profile1 = new CarPerformanceProfile(0, 0, 1, 1);
            Assert.AreEqual(1, setting.GetValue(profile1).keys[0].value);
            
            CarPerformanceProfile profile2 = new CarPerformanceProfile(0.5f, 0.5f, 0, 0);
            Assert.AreEqual(3, setting.GetValue(profile2).keys[1].value);

            CarPerformanceProfile profile3 = new CarPerformanceProfile(1, 1, 1, 1);
            Assert.AreEqual(6, setting.GetValue(profile3).keys[2].value);
        }
        
    }
}
