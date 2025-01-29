using HarmonyLib;
using static Player;
using UnityEngine;

namespace BuildRestrictionTweaksSync
{
    internal static class Patches
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
                if (RestrictionTweaks.IgnoreBuildZone.Value || RestrictionTweaks.DisableAllRestrictions.Value)
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
                if ((RestrictionTweaks.IgnoreMissingStation.Value || RestrictionTweaks.DisableAllRestrictions.Value)
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

                    if (RestrictionTweaks.DisableAllRestrictions.Value
                        || placementStatus == PlacementStatus.Invalid && RestrictionTweaks.IgnoreInvalid.Value
                        || placementStatus == PlacementStatus.BlockedbyPlayer && RestrictionTweaks.IgnoreBlockedbyPlayer.Value
                        || placementStatus == PlacementStatus.NoBuildZone && RestrictionTweaks.IgnoreBuildZone.Value
                        || placementStatus == PlacementStatus.MoreSpace && RestrictionTweaks.IgnoreSpaceRestrictions.Value
                        || placementStatus == PlacementStatus.NoTeleportArea && RestrictionTweaks.IgnoreTeleportAreaRestrictions.Value
                        || placementStatus == PlacementStatus.ExtensionMissingStation && RestrictionTweaks.IgnoreMissingStationExtension.Value
                        || placementStatus == PlacementStatus.WrongBiome && RestrictionTweaks.IgnoreBiomeRestrictions.Value
                        || placementStatus == PlacementStatus.NeedCultivated && RestrictionTweaks.IgnoreCultivationRestrictions.Value
                        || placementStatus == PlacementStatus.NeedDirt && RestrictionTweaks.IgnoreDirtRestrictions.Value
                        || placementStatus == PlacementStatus.NotInDungeon && RestrictionTweaks.IgnoreDungeonRestrictions.Value)
                    {
                        __instance.m_placementStatus = PlacementStatus.Valid;
                        __instance.SetPlacementGhostValid(true);
                    }
                }
            }
        }
    }
}