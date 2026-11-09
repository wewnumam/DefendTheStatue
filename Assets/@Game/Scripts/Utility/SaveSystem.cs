using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ProjectTA.Utility
{
    public interface ISaveSystem<T> where T : class, new()
    {
        T Load();
        void Save(T data);
        void Delete();
    }

    public class SaveSystem<T> : ISaveSystem<T> where T : class, new()
    {
        private const string JsonExtension = ".json";
        private const string BinExtension = ".bin";

        private readonly string _dataPath;
        private readonly byte[] _encryptionKey; // 32 bytes for AES-256
        private readonly byte[] _encryptionIV;  // 16 bytes for AES

        public SaveSystem(string fileName)
        {
            if (!PlayerPrefs.HasKey(TagManager.KEY_VERSION) || !PlayerPrefs.GetString(TagManager.KEY_VERSION).Equals(Application.version))
                Delete();

#if UNITY_EDITOR
            string extension = JsonExtension;
#else
            string extension = BinExtension;
#endif
            _dataPath = Path.Combine(Application.persistentDataPath, fileName + extension);

            // Initialize encryption key and IV
            _encryptionKey = Encoding.UTF8.GetBytes(TagManager.DEV_ENCRYPTIONKEY);
            _encryptionIV = Encoding.UTF8.GetBytes(TagManager.DEV_ENCRYPTIONIV);
        }

        public void Save(T data)
        {
#if UNITY_EDITOR
            SaveAsJson(data);
#else
            SaveAsBinary(data);
#endif
        }

        public T Load()
        {
            if (File.Exists(_dataPath))
            {
#if UNITY_EDITOR
                return LoadFromJson();
#else
                return LoadFromBinary();
#endif
            }
            return new T();
        }

        public void Delete()
        {
            try
            {
                if (File.Exists(_dataPath))
                {
                    File.Delete(_dataPath);
                    Debug.Log($"Save file deleted at: {_dataPath}");
                }
                else
                {
                    Debug.LogWarning($"No save file found to delete at: {_dataPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete save file: {ex.Message}");
            }
        }

        private void SaveAsJson(T data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(_dataPath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save JSON file: {ex.Message}");
            }
        }

        private T LoadFromJson()
        {
            try
            {
                string json = File.ReadAllText(_dataPath);
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load JSON file: {ex.Message}");
                return new T();
            }
        }

        private void SaveAsBinary(T data)
        {
            try
            {
                using (FileStream stream = File.Open(_dataPath, FileMode.Create))
                using (Aes aes = Aes.Create())
                {
                    aes.Key = _encryptionKey;
                    aes.IV = _encryptionIV;

                    using (CryptoStream cryptoStream = new CryptoStream(stream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        var formatter = new BinaryFormatter();
                        formatter.Serialize(cryptoStream, data);
                    }
                }
                Debug.Log($"Data successfully saved and encrypted to {_dataPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save binary file: {ex.Message}");
            }
        }

        private T LoadFromBinary()
        {
            try
            {
                using (FileStream stream = File.Open(_dataPath, FileMode.Open))
                using (Aes aes = Aes.Create())
                {
                    aes.Key = _encryptionKey;
                    aes.IV = _encryptionIV;

                    using (CryptoStream cryptoStream = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        var formatter = new BinaryFormatter();
                        return formatter.Deserialize(cryptoStream) as T;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load binary file: {ex.Message}");
                return new T();
            }
        }

    }
}
