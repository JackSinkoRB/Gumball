using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Gumball
{
    public sealed class JsonDataProvider : DataProvider
    {

        private readonly string filePath;

        public JsonDataProvider(string identifier) : base(identifier)
        {
            filePath = $"{Application.persistentDataPath}/{identifier}.json";
        }

        public override bool SourceExists()
        {
            return File.Exists(filePath);
        }

        protected override void SaveToSource()
        {
            //serialise to binary file
            using FileStream fileStream = File.Create(filePath);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(fileStream, currentValues);
        }

        protected override void LoadFromSource()
        {
            if (!SourceExists())
            {
                GlobalLoggers.SaveDataLogger.Log($"No previously saved data for '{identifier}'.");
                return;
            }

            //deserialise from the binary file
            using FileStream fileStream = File.Open(filePath, FileMode.Open);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            object data = binaryFormatter.Deserialize(fileStream);
            currentValues = (Dictionary<string, object>)data;
        }

        protected override void OnRemoveFromSource()
        {
            File.Delete(filePath);
        }
        
    }
}