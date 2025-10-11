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


        // Postfix patch for RestBenchHelper.SetOnBench to give player full silk when resting on a bench
        [HarmonyPatch(typeof(RestBenchHelper), "SetOnBench")]
        [HarmonyPostfix]
        public static void SetOnBenchPostfix(bool onBench)
        {
            if (onBench)
            {
                var playerData = GameManager.instance.playerData;
                HeroController controll = HeroController.instance;
                int missingSilk = playerData.CurrentSilkMax - playerData.silk;
                if (missingSilk > 0)
                {
                    controll.AddSilk(missingSilk, false);

                }
            }
        }

        // Postfix patch for CurrencyObjectBase.MagnetToolIsEquipped to make rosaries and shards attracted without magnet tool
        [HarmonyPatch(typeof(CurrencyObjectBase), "MagnetToolIsEquipped")]
        [HarmonyPostfix]
        public static void MagnetToolIsEquipped_Postfix(CurrencyObjectBase __instance, ref bool __result)
        {
            if (IsRosaryOrShard(__instance))
            {
                __result = true;
            }
        }

        // Check type name to identify rosaries (Inner name: geo) or shards
        private static bool IsRosaryOrShard(object o)
        {
            if (o == null) return false;
            string name = o.GetType().Name;
            return name.Contains("Rosary") || name.Contains("Shard") || name.Contains("Geo");
        }

        // Postfix patch para CurrencyObjectBase.OnEnable: asigna el visual effect magnético a rosarios y shards
        [HarmonyPatch(typeof(CurrencyObjectBase), "OnEnable")]
        [HarmonyPostfix]
        public static void CurrencyObjectBase_OnEnable_Postfix(CurrencyObjectBase __instance)
        {
            if (__instance == null) return;
            string typeName = __instance.GetType().Name;
            // Solo para shards y rosarios
            if (!(typeName.Contains("Rosary") || typeName.Contains("Shard"))) return;

            var magnetEffectField = typeof(CurrencyObjectBase).GetField("magnetEffect", BindingFlags.Instance | BindingFlags.NonPublic);
            var hasMagnetEffectField = typeof(CurrencyObjectBase).GetField("hasMagnetEffect", BindingFlags.Instance | BindingFlags.NonPublic);
            var effect = magnetEffectField?.GetValue(__instance) as GameObject;
            if (effect != null) return; // Ya tiene el efecto

            // Busca el template de rosary_magnet_effect solo una vez
            if (_magnetTemplate == null)
            {
                foreach (Transform t in Resources.FindObjectsOfTypeAll<Transform>())
                {
                    if (t != null && t.name == "rosary_magnet_effect")
                    {
                        _magnetTemplate = t.gameObject;
                        break;
                    }
                }
                if (_magnetTemplate == null) return; // Si no existe, no hagas nada
            }

            // Instancia visual magnet effect para el objeto
            GameObject visual = Object.Instantiate(_magnetTemplate, ((Component)__instance).transform);
            visual.name = "rosary_magnet_effect (shard/rosary)";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = _magnetTemplate.transform.localScale;
            visual.SetActive(false);

            magnetEffectField?.SetValue(__instance, visual);
            hasMagnetEffectField?.SetValue(__instance, true);
        }

        // Private static para guardar el template visual
        private static GameObject _magnetTemplate;

    }
}

