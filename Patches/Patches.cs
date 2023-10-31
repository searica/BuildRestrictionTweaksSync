using HarmonyLib;
using BuildRestrictionTweaksSync.Configs;
using static Player;
using UnityEngine;

namespace BuildRestrictionTweaksSync.Patches
{
    internal class Patches
    {
        private static GameObject _gameObject;
        private static CraftingStation _craftingStation;

        private static CraftingStation GetCraftingStation()
        {
            if (_gameObject == null)
            {
                _gameObject = new GameObject();
                Object.DontDestroyOnLoad(_gameObject);
            }
            if (_craftingStation == null)
            {
                _craftingStation = _gameObject.AddComponent<CraftingStation>();
            }
            return _craftingStation;
        }

        [HarmonyPatch(typeof(Location))]
        private static class Location_IsInsideNoBuildLocation_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(Location.IsInsideNoBuildLocation))]
            private static void Postfix(ref bool __result)
            {
                if (Config.IgnoreBuildZone.Value || Config.DisableAllRestrictions.Value)
                {
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(CraftingStation))]
        private static class CraftingStation_HaveBuildStationInRange_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(CraftingStation.HaveBuildStationInRange))]
            private static void Postfix(ref CraftingStation __result, string name)
            {
                //IL_0053: Unknown result type (might be due to invalid IL or missing references)
                //IL_005d: Expected O, but got Unknown
                if ((Config.IgnoreMissingStation.Value || Config.DisableAllRestrictions.Value)
                    && __result == null)
                {
                    __result = GetCraftingStation();
                }
            }
        }

        [HarmonyPatch(typeof(Player))]
        private static class Player_UpdatePlacementGhost_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch(nameof(Player.UpdatePlacementGhost))]
            private static void Postfix(Player __instance)
            {
                if (__instance.m_placementGhost != null)
                {
                    PlacementStatus placementStatus = __instance.m_placementStatus;

                    if (placementStatus == PlacementStatus.Valid
                        || placementStatus == PlacementStatus.PrivateZone)
                    {
                        return;
                    }

                    if (Config.DisableAllRestrictions.Value
                        || placementStatus == PlacementStatus.Invalid && Config.IgnoreInvalid.Value
                        || placementStatus == PlacementStatus.BlockedbyPlayer && Config.IgnoreBlockedbyPlayer.Value
                        || placementStatus == PlacementStatus.NoBuildZone && Config.IgnoreBuildZone.Value
                        || placementStatus == PlacementStatus.MoreSpace && Config.IgnoreSpaceRestrictions.Value
                        || placementStatus == PlacementStatus.NoTeleportArea && Config.IgnoreTeleportAreaRestrictions.Value
                        || placementStatus == PlacementStatus.ExtensionMissingStation && Config.IgnoreMissingStationExtension.Value
                        || placementStatus == PlacementStatus.WrongBiome && Config.IgnoreBiomeRestrictions.Value
                        || placementStatus == PlacementStatus.NeedCultivated && Config.IgnoreCultivationRestrictions.Value
                        || placementStatus == PlacementStatus.NeedDirt && Config.IgnoreDirtRestrictions.Value
                        || placementStatus == PlacementStatus.NotInDungeon && Config.IgnoreDungeonRestrictions.Value)
                    {
                        __instance.m_placementStatus = PlacementStatus.Valid;
                        __instance.SetPlacementGhostValid(true);
                    }
                }
            }
        }
    }
}