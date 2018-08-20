using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
            
            DateTime dt = File.GetLastWriteTime(path);
            File.SetLastWriteTime(path, dt + new TimeSpan(TimeSpan.TicksPerMinute));

        }

        private static void OnSaveLoad(int saveSlot)
        {
            GameManager gm = GameManager.instance;

            string text;
            string jsonPath = GetSavePath(saveSlot, "json");

            if (File.Exists(jsonPath))
            {
                DateTime jsonWrite = File.GetLastWriteTimeUtc(jsonPath);
                DateTime datWrite = File.GetLastWriteTimeUtc(GetSavePath(saveSlot, "dat"));
                
                text = jsonWrite > datWrite
                    ? Encoding.UTF8.GetString(File.ReadAllBytes(jsonPath))
                    : LoadEncrypted(gm, saveSlot);
            }
            else
            {
                text = LoadEncrypted(gm, saveSlot);
            }

            SaveGameData saveGameData = JsonUtility.FromJson<SaveGameData>(text);

            gm.playerData = PlayerData.instance = saveGameData.playerData;
            gm.sceneData = SceneData.instance = saveGameData.sceneData;
            gm.profileID = saveSlot;
            gm.inputHandler.RefreshPlayerData();
        }

        private static string LoadEncrypted(GameManager gm, int saveSlot)
        {
            // ReSharper disable once InvertIf
            if (gm.gameConfig.useSaveEncryption && !Platform.Current.IsFileSystemProtected)
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                MemoryStream serializationStream = new MemoryStream(Platform.Current.ReadSaveSlot(saveSlot));
                return Encryption.Decrypt((string) binaryFormatter.Deserialize(serializationStream));
            }

            return Encoding.UTF8.GetString(Platform.Current.ReadSaveSlot(saveSlot));
        }


        private static string GetSavePath(int saveSlot, string ending)
        {
            return Path.Combine(Application.persistentDataPath, $"user{saveSlot}.{ending}");
        }
    }
}