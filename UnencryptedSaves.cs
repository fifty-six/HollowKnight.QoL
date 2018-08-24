using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using Modding;
using UnityEngine;

namespace QoL
{
    [UsedImplicitly]
    public class UnencryptedSaves : Mod
    {
        public override void Initialize()
        {
            ModHooks.Instance.SavegameLoadHook += OnSaveLoad;
            ModHooks.Instance.SavegameSaveHook += OnSaveSave;
        }

        private static void OnSaveSave(int id)
        {
            string path = GetSavePath(id, "json");
            SaveGameData sg = new SaveGameData(GameManager.instance.playerData, GameManager.instance.sceneData);
            string text = JsonUtility.ToJson(sg, true);

            File.WriteAllText(path, text);

            File.SetLastWriteTime(path, new DateTime(1999, 6, 11));
        }

        private static void OnSaveLoad(int saveSlot)
        {
            GameManager gm = GameManager.instance;
            
            void DoLoad(string text)
            {
                SaveGameData saveGameData = JsonUtility.FromJson<SaveGameData>(text);

                gm.playerData = PlayerData.instance = saveGameData.playerData;
                gm.sceneData = SceneData.instance = saveGameData.sceneData;
                gm.profileID = saveSlot;
                gm.inputHandler.RefreshPlayerData();
            }

            string jsonPath = GetSavePath(saveSlot, "json");

            if (File.Exists(jsonPath))
            {
                DateTime jsonWrite = File.GetLastWriteTimeUtc(jsonPath);
                DateTime datWrite = File.GetLastWriteTimeUtc(GetSavePath(saveSlot, "dat"));

                if (jsonWrite > datWrite && jsonWrite.Year != 1999)
                    LoadJson(jsonPath, DoLoad);
                else
                    LoadDat(gm, saveSlot, DoLoad);
            }
            else
            {
                LoadDat(gm, saveSlot, DoLoad);
            }
        }

        private static void LoadJson(string jsonPath, Action<string> callback)
        {
            string res = null;
            
            try
            {
                res = Encoding.UTF8.GetString(File.ReadAllBytes(jsonPath));
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            
            CoreLoop.InvokeNext(() =>
            {
                callback?.Invoke(res);
            });
        }
        
        private static void LoadDat(GameManager gm, int saveSlot, Action<string> callback)
        {
            if (gm.gameConfig.useSaveEncryption && !Platform.Current.IsFileSystemProtected)
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                Platform.Current.ReadSaveSlot(saveSlot, bytes =>
                {
                    MemoryStream serializationStream = new MemoryStream(bytes);
                    callback(Encryption.Decrypt((string) binaryFormatter.Deserialize(serializationStream)));
                });
            }

            Platform.Current.ReadSaveSlot(saveSlot, bytes => { callback(Encoding.UTF8.GetString(bytes)); });
        }


        private static string GetSavePath(int saveSlot, string ending)
        {
            return Path.Combine(Application.persistentDataPath, $"user{saveSlot}.{ending}");
        }
    }
}