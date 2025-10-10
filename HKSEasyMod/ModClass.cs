using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace HKSTestMod
{
    // Defines the mod with unique ID, name and version for BepInEx to recognize and load
    [BepInPlugin("com.marcmeru.easymod", "HKSEasyMod", "1.0.0")]
    public class HKTestMod : BaseUnityPlugin
    {
        // Multiplier used to double player damage (can be adjusted)
        private static float damageMultiplier = 2f;

        // Called when the plugin is loaded. Sets up Harmony patching.
        private void Awake()
        {
            // Create a Harmony instance with unique ID
            var harmony = new Harmony("com.marcmeru.easymod");

            // Apply all harmony patches defined in this assembly/class
            harmony.PatchAll(typeof(HKTestMod));

            // Log an informational message indicating the mod has loaded
            Logger.LogInfo("Mod initialized");
        }

        // Prefix patch for PlayerData.AddGeo method to double amount of Geo collected
        [HarmonyPatch(typeof(PlayerData), "AddGeo")]
        [HarmonyPrefix]
        private static void DoubleGeoPrefix(ref int amount)
        {
            amount *= 2; // Double Geo amount
        }

        // Prefix patch for PlayerData.AddShards to double the number of Shards collected
        [HarmonyPatch(typeof(PlayerData), "AddShards")]
        [HarmonyPrefix]
        private static void DoubleShardsPrefix(ref int amount)
        {
            amount *= 2; // Double Shards
        }

        // Prefix patch for PlayerData.AddSilk to double the amount of Silk collected
        [HarmonyPatch(typeof(PlayerData), "AddSilk")]
        [HarmonyPrefix]
        private static void DoubleSilkPrefix(ref int amount)
        {
            amount *= 2; // Double Silk
        }

        // Prefix patch for PlayerData.TakeHealth to limit damage taken by the player to a maximum of 1 point
        [HarmonyPatch(typeof(PlayerData), "TakeHealth")]
        [HarmonyPrefix]
        private static void LimitDamagePrefix(ref int amount)
        {
            if (amount > 0)
            {
                amount = 1; // Cap damage to 1
            }
        }

        // Postfix patch for GameMap.PositionCompassAndCorpse to always show compass icon on the map
        [HarmonyPatch(typeof(GameMap), "PositionCompassAndCorpse")]
        [HarmonyPostfix]
        private static void AlwaysShowCompassPostfix(object __instance)
        {
            // Access private field compassIcon via reflection
            var compassIconField = __instance.GetType().GetField("compassIcon", BindingFlags.NonPublic | BindingFlags.Instance);
            var compassIcon = compassIconField?.GetValue(__instance) as UnityEngine.GameObject;
            if (compassIcon != null)
            {
                compassIcon.SetActive(true); // Enable compass icon
            }

            // Access private field displayingCompass via reflection and set true to make compass visible
            var displayingCompassField = __instance.GetType().GetField("displayingCompass", BindingFlags.NonPublic | BindingFlags.Instance);
            if (displayingCompassField != null)
            {
                displayingCompassField.SetValue(__instance, true);
            }
        }

        // Prefix patch for HealthManager.TakeDamage to double the damage dealt by the player
        [HarmonyPatch(typeof(HealthManager), "TakeDamage")]
        [HarmonyPrefix]
        private static void DoubleDamagePrefix(ref HitInstance hitInstance)
        {
            hitInstance.DamageDealt = (int)(hitInstance.DamageDealt * damageMultiplier);
        }


    }
}

