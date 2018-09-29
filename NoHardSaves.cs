using Modding;

namespace NoHardSaves
{
    public class NoHardSaves : Mod, ITogglableMod
    {
        public override void Initialize()
        {
            RegisterCallbacks();
        }

        void ITogglableMod.Unload()
        {
            UnregisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            On.PlayerData.SetBenchRespawn -= PlayerData_SetBenchRespawn;
            On.PlayerData.SetBenchRespawn += PlayerData_SetBenchRespawn;

            On.PlayerData.SetBenchRespawn_1 -= PlayerData_SetBenchRespawn_1;
            On.PlayerData.SetBenchRespawn_1 += PlayerData_SetBenchRespawn_1;

            On.PlayerData.SetBenchRespawn_2 -= PlayerData_SetBenchRespawn_2;
            On.PlayerData.SetBenchRespawn_2 += PlayerData_SetBenchRespawn_2;
        }

        private void UnregisterCallbacks()
        {
            On.PlayerData.SetBenchRespawn -= PlayerData_SetBenchRespawn;
            On.PlayerData.SetBenchRespawn_1 -= PlayerData_SetBenchRespawn_1;
            On.PlayerData.SetBenchRespawn_2 -= PlayerData_SetBenchRespawn_2;
        }

        private void PlayerData_SetBenchRespawn_2(On.PlayerData.orig_SetBenchRespawn_2 orig, PlayerData self, string spawnMarker, string sceneName, int spawnType, bool facingRight)
        {
            if (!string.IsNullOrEmpty(spawnMarker) && spawnMarker.ToLower().Contains("bench"))
            {
                orig(self, spawnMarker, sceneName, spawnType, facingRight);
            }
        }

        private void PlayerData_SetBenchRespawn_1(On.PlayerData.orig_SetBenchRespawn_1 orig, PlayerData self, string spawnMarker, string sceneName, bool facingRight)
        {
            if (!string.IsNullOrEmpty(spawnMarker) && spawnMarker.ToLower().Contains("bench"))
            {
                orig(self, spawnMarker, sceneName, facingRight);
            }
        }

        private void PlayerData_SetBenchRespawn(On.PlayerData.orig_SetBenchRespawn orig, PlayerData self, RespawnMarker spawnMarker, string sceneName, int spawnType)
        {
            if (spawnMarker != null && !string.IsNullOrEmpty(spawnMarker.name) && spawnMarker.name.ToLower().Contains("bench"))
            {
                orig(self, spawnMarker, sceneName, spawnType);
            }
        }
    }
}
