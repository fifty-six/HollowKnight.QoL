using System.Reflection;
using JetBrains.Annotations;
using Modding;

namespace QoL
{
    [UsedImplicitly]
    public class NoHardSaves : Mod, ITogglableMod
    {
        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize() => RegisterCallbacks();

        void ITogglableMod.Unload() => UnregisterCallbacks();

        private static void RegisterCallbacks()
        {
            On.PlayerData.SetBenchRespawn_RespawnMarker_string_int -= PlayerData_SetBenchRespawn;
            On.PlayerData.SetBenchRespawn_RespawnMarker_string_int += PlayerData_SetBenchRespawn;

            On.PlayerData.SetBenchRespawn_string_string_bool -= PlayerData_SetBenchRespawn_1;
            On.PlayerData.SetBenchRespawn_string_string_bool += PlayerData_SetBenchRespawn_1;

            On.PlayerData.SetBenchRespawn_string_string_int_bool -= PlayerData_SetBenchRespawn_2;
            On.PlayerData.SetBenchRespawn_string_string_int_bool += PlayerData_SetBenchRespawn_2;
        }

        private static void UnregisterCallbacks()
        {
            On.PlayerData.SetBenchRespawn_RespawnMarker_string_int -= PlayerData_SetBenchRespawn;
            On.PlayerData.SetBenchRespawn_string_string_bool -= PlayerData_SetBenchRespawn_1;
            On.PlayerData.SetBenchRespawn_string_string_int_bool -= PlayerData_SetBenchRespawn_2;
        }

        private static bool IsGarbage
        {
            get
            {
                string str = GameManager.instance.GetSceneNameString();
                return str == "Deepnest_Spider_Town" || str == "GG_Workshop";
            }
        }

        private static void PlayerData_SetBenchRespawn_2
        (
            On.PlayerData.orig_SetBenchRespawn_string_string_int_bool orig,
            PlayerData                                                self,
            string                                                    spawnMarker,
            string                                                    sceneName,
            int                                                       spawnType,
            bool                                                      facingRight
        )
        {
            if (IsGarbage || !string.IsNullOrEmpty(spawnMarker) && spawnMarker.ToLower().Contains("bench"))
            {
                orig(self, spawnMarker, sceneName, spawnType, facingRight);
            }
        }

        private static void PlayerData_SetBenchRespawn_1
        (
            On.PlayerData.orig_SetBenchRespawn_string_string_bool orig,
            PlayerData                                            self,
            string                                                spawnMarker,
            string                                                sceneName,
            bool                                                  facingRight
        )
        {
            if (IsGarbage || !string.IsNullOrEmpty(spawnMarker) && spawnMarker.ToLower().Contains("bench"))
            {
                orig(self, spawnMarker, sceneName, facingRight);
            }
        }

        private static void PlayerData_SetBenchRespawn
        (
            On.PlayerData.orig_SetBenchRespawn_RespawnMarker_string_int orig,
            PlayerData                                                  self,
            RespawnMarker                                               spawnMarker,
            string                                                      sceneName,
            int                                                         spawnType
        )
        {
            if (IsGarbage || spawnMarker != null && !string.IsNullOrEmpty(spawnMarker.name) && spawnMarker.name.ToLower().Contains("bench"))
            {
                orig(self, spawnMarker, sceneName, spawnType);
            }
        }
    }
}