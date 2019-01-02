using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using ModCommon.Util;
using Modding;
using MonoMod.RuntimeDetour.HookGen;
using UnityEngine;

namespace QoL
{
    [UsedImplicitly]
    public class UnencryptedSaves : Mod
    {
        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        
        private static readonly MethodInfo GET_SAVE_FILE_NAME = typeof(ModHooks).GetMethod("GetSaveFileName", BindingFlags.Instance | BindingFlags.NonPublic);

        public override void Initialize()
        {
            ModHooks.Instance.SavegameLoadHook += OnSaveLoad;
            ModHooks.Instance.SavegameSaveHook += OnSaveSave;
            IL.DesktopPlatform.WriteSaveSlot += RemoveStupidSave;
        }

        private void RemoveStupidSave(HookIL il)
        {
            HookILCursor c = il.At(0);
            while (c.TryFindNext(out HookILCursor[] cursors,
                      /* 0 */    x => x.MatchLdstr("_1.4.3.2.dat"),
                      /* 1 */    x => x.MatchCall(typeof(string), nameof(String.Concat)),
                      /* 2 */    x => true, // stloc.s V_5 (5)
                      /* 3 */    x => true, // ldloc.s V_5 (5)
                      /* 4 */    x => x.MatchLdarg(2),
                      /* 5 */    x => x.MatchCall(typeof(File), nameof(File.WriteAllBytes)),
                      /* 6 */    x => true // leave.s 51 (008C) ldloc.1
            ))
            {
                for (int i = cursors.Length - 1; i >= 0; i--)
                {
                    cursors[i].Remove();
                }
            }
        }

        private static void OnSaveSave(int id)
        {
            id = GetRealID(id);

            Modding.Logger.Log("Saving save slot: " + id);
            string path = GetSavePath(id, "json");
            var sg = new SaveGameData(GameManager.instance.playerData, GameManager.instance.sceneData);
            string text = JsonUtility.ToJson(sg, true);

            File.WriteAllText(path, text);

            File.SetLastWriteTime(path, new DateTime(1999, 6, 11));
        }

        private static void OnSaveLoad(int saveSlot)
        {
            saveSlot = GetRealID(saveSlot);

            Modding.Logger.Log("Loading save slot: " + saveSlot);
            GameManager gm = GameManager.instance;

            void DoLoad(string text)
            {
                try
                {
                    var saveGameData = JsonUtility.FromJson<SaveGameData>(text);

                    gm.playerData = PlayerData.instance = saveGameData.playerData;
                    gm.sceneData = SceneData.instance = saveGameData.sceneData;
                    gm.profileID = saveSlot;
                    gm.inputHandler.RefreshPlayerData();
                }
                catch (ArgumentException)
                {
                    // It's fine to just stop here as this is *after* the game loads the dat anyways
                }
            }

            string jsonPath = GetSavePath(saveSlot, "json");

            if (!File.Exists(jsonPath)) return;

            DateTime jsonWrite = File.GetLastWriteTimeUtc(jsonPath);

            if (jsonWrite.Year != 1999)
                LoadJson(jsonPath, DoLoad);
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

            CoreLoop.InvokeNext(() => { callback?.Invoke(res); });
        }

        private static string GetSavePath(int saveSlot, string ending)
        {
            return Path.Combine(Platform.Current.GetAttr<string>("saveDirPath"), $"user{saveSlot}.{ending}");
        }

        private static int GetRealID(int id)
        {
            string s = (string) GET_SAVE_FILE_NAME.Invoke(ModHooks.Instance, new object[] {id});

            return s == null
                ? id
                : int.Parse(new string(s.SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit).ToArray()));
        }
    }
}