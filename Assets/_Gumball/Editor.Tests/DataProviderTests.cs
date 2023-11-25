using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Gumball.Editor.Tests
{
    public class DataProviderTests
    {
        
        private static readonly DataProvider[] providers = { new JsonDataProvider("JsonDataProviderTests") };
        
        [Serializable]
        private class SerializableClass
        {
            public int exampleInt;
            public string exampleString;
            public float exampleFloat;

            public SerializableClass(int exampleInt, string exampleString, float exampleFloat)
            {
                this.exampleInt = exampleInt;
                this.exampleString = exampleString;
                this.exampleFloat = exampleFloat;
            }
        }
        
        [SetUp]
        public void Setup()
        {
            foreach (DataProvider provider in providers)
            {
                provider.RemoveFromSource();
            }
        }
        
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            foreach (DataProvider provider in providers)
            {
                provider.RemoveFromSource();
            }
        }
        
        [TestCaseSource(nameof(providers))]
        public void LoadFromSourceSync(DataProvider provider)
        {
            Assert.IsTrue(!provider.IsLoaded);
            provider.LoadFromSourceSync();
            Assert.IsTrue(provider.IsLoaded);
        }
        
        [TestCaseSource(nameof(providers))]
        public void SetAndGetMemoryString(DataProvider provider)
        {
            const string key = "TestKey";
            const string value = "TestValue";
            provider.Set(key, value);
            Assert.IsTrue(provider.HasKey(key));
            Assert.AreEqual(provider.Get<string>(key), value);
        }
        
        [TestCaseSource(nameof(providers))]
        public void SetAndGetAfterSavingString(DataProvider provider)
        {
            const string key = "TestKey";
            const string value = "TestValue";
            provider.Set(key, value);
            DataProvider.SaveAllSync();
            Assert.IsTrue(provider.HasKey(key));
            Assert.AreEqual(provider.Get<string>(key), value);
        }
        
        [TestCaseSource(nameof(providers))]
        public void SetAndGetMemoryCustomClass(DataProvider provider)
        {
            const string key = "TestKey";
            const int exampleInt = 2;
            const string exampleString = "ExampleValue";
            const float exampleFloat = 3.2f;
            SerializableClass value = new SerializableClass(exampleInt, exampleString, exampleFloat);
            
            provider.Set(key, value);
            Assert.IsTrue(provider.HasKey(key));
            Assert.AreEqual(provider.Get<SerializableClass>(key).exampleInt, exampleInt);
            Assert.AreEqual(provider.Get<SerializableClass>(key).exampleString, exampleString);
            Assert.AreEqual(provider.Get<SerializableClass>(key).exampleFloat, exampleFloat);
        }
        
        [TestCaseSource(nameof(providers))]
        public void SetAndGetAfterSavingCustomClass(DataProvider provider)
        {
            const string key = "TestKey";
            const int exampleInt = 2;
            const string exampleString = "ExampleValue";
            const float exampleFloat = 3.2f;
            SerializableClass value = new SerializableClass(exampleInt, exampleString, exampleFloat);
            
            provider.Set(key, value);
            DataProvider.SaveAllSync();
            Assert.IsTrue(provider.HasKey(key));
            Assert.AreEqual(provider.Get<SerializableClass>(key).exampleInt, exampleInt);
            Assert.AreEqual(provider.Get<SerializableClass>(key).exampleString, exampleString);
            Assert.AreEqual(provider.Get<SerializableClass>(key).exampleFloat, exampleFloat);
        }
        
        [TestCaseSource(nameof(providers))]
        public void SetValueNullRemovesKeyMemory(DataProvider provider)
        {
            const string key = "TestKey";
            const string value = "TestValue";

            provider.Set(key, value);
            Assert.IsTrue(provider.HasKey(key));
            
            provider.Set(key, null);
            Assert.IsTrue(!provider.HasKey(key));
        }
        
        [TestCaseSource(nameof(providers))]
        public void SetValueNullRemovesKeySaving(DataProvider provider)
        {
            const string key = "TestKey";
            const string value = "TestValue";

            provider.Set(key, value);
            DataProvider.SaveAllSync();
            Assert.IsTrue(provider.HasKey(key));
            
            provider.Set(key, null);
            Assert.IsTrue(!provider.HasKey(key));
        }
        
        [TestCaseSource(nameof(providers))]
        public void RemoveKeyMemory(DataProvider provider)
        {
            const string key = "TestKey";
            const string value = "TestValue";

            provider.Set(key, value);
            Assert.IsTrue(provider.HasKey(key));
            
            provider.RemoveKey(key);
            Assert.IsTrue(!provider.HasKey(key));
        }
        
        [TestCaseSource(nameof(providers))]
        public void RemoveKeySaving(DataProvider provider)
        {
            const string key = "TestKey";
            const string value = "TestValue";

            provider.Set(key, value);
            DataProvider.SaveAllSync();
            Assert.IsTrue(provider.HasKey(key));
            
            provider.RemoveKey(key);
            Assert.IsTrue(!provider.HasKey(key));
        }
        
        [TestCaseSource(nameof(providers))]
        public void RemoveFromSourceNonSaving(DataProvider provider)
        {
            const string key = "TestKey";
            const string value = "TestValue";

            provider.Set(key, value);
            Assert.IsTrue(provider.HasKey(key));
            
            provider.RemoveFromSource();
            Assert.IsTrue(!provider.HasKey(key));
        }
        
        [TestCaseSource(nameof(providers))]
        public void RemoveFromSourceSaving(DataProvider provider)
        {
            const string key = "TestKey";
            const string value = "TestValue";

            provider.Set(key, value);
            DataProvider.SaveAllSync();
            Assert.IsTrue(provider.HasKey(key));
            
            provider.RemoveFromSource();
            Assert.IsTrue(!provider.HasKey(key));
        }

        [TestCaseSource(nameof(providers))]
        public void SetDirty(DataProvider provider)
        {
            const string key = "TestKey";
            const string value = "TestValue";
            
            Assert.IsTrue(!provider.IsDirty);
            provider.LoadFromSourceSync();
            Assert.IsTrue(!provider.IsDirty);
            provider.Set(key, value);
            Assert.IsTrue(provider.IsDirty);
        }
        
        [TestCaseSource(nameof(providers))]
        public void SetNotDirty(DataProvider provider)
        {
            const string key = "TestKey";
            const string value = "TestValue";
            
            Assert.IsTrue(!provider.IsDirty);
            provider.Set(key, value);
            Assert.IsTrue(provider.IsDirty);
            DataProvider.SaveAllSync();
            Assert.IsTrue(!provider.IsDirty);
            provider.Set(key, value); //if setting the same, don't set dirty
            Assert.IsTrue(!provider.IsDirty);
        }
        
        [TestCaseSource(nameof(providers))]
        public void SetDirtyIfNull(DataProvider provider)
        {
            const string key = "TestKey";
            const string value = "TestValue";
            
            Assert.IsTrue(!provider.IsDirty);
            provider.Set(key, value);
            Assert.IsTrue(provider.IsDirty);
            DataProvider.SaveAllSync();
            Assert.IsTrue(!provider.IsDirty);
            provider.Set(key, null);
            Assert.IsTrue(provider.IsDirty);
        }
        
        [TestCaseSource(nameof(providers))]
        public void SetNotDirtyIfAlreadyNull(DataProvider provider)
        {
            const string key = "TestKey";

            Assert.IsTrue(!provider.IsDirty);
            provider.Set(key, null);
            Assert.IsTrue(!provider.IsDirty);
        }

    }
}
