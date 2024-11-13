using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Gumball
{
    public sealed class JsonDataProvider : DataProvider
    {

        public readonly string FilePath;

        public JsonDataProvider(string identifier) : base(identifier)
        {
            FilePath = $"{Application.persistentDataPath}/{identifier}.json";
        }

        public override bool SourceExists()
        {
            return File.Exists(FilePath);
        }

        protected override void SaveToSource()
        {
            //serialise to binary file
            using FileStream fileStream = new FileStream(FilePath, FileMode.Create, FileAccess.Write);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(fileStream, currentValues);
            fileStream.Flush();
        }

        protected override void LoadFromSource()
        {
            if (!SourceExists())
            {
                GlobalLoggers.SaveDataLogger.Log($"No previously saved data for '{Identifier}'.");
                return;
            }

            //deserialise from the binary file
            using FileStream fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            try
            {
                object data = binaryFormatter.Deserialize(fileStream);
                currentValues = (Dictionary<string, object>)data;
            }
            catch (SerializationException exception)
            {
                Debug.LogWarning(exception.Message);
            }
        }

        protected override void OnRemoveFromSource()
        {
            File.Delete(FilePath);
        }
        
    }
}