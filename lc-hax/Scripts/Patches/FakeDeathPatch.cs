using GameNetcodeStuff;
using HarmonyLib;

[HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayerClientRpc))]
sealed class FakeDeathPatch {
    static bool Prefix(int playerId) {
        if (!Setting.EnableFakeDeath) return true;

        Setting.EnableFakeDeath = false;
        return Helper.LocalPlayer?.PlayerIndex() != playerId;
    }
}
