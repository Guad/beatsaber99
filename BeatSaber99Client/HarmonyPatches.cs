using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace BeatSaber99Client
{
    public static class HarmonyPatcher
    {
        public static Harmony instance;

        public static void Patch()
        {
            if (instance == null)
                instance = new Harmony("com.guad.testmod");

            Plugin.log.Info("Patching with harmony...");

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.IsClass && x.Namespace == nameof(BeatSaber99Client) + ".OverriddenClasses"))
            {
                List<MethodInfo> harmonyMethods = instance.CreateClassProcessor(type).Patch();
                if (harmonyMethods != null && harmonyMethods.Count > 0)
                {
                    foreach (var method in harmonyMethods)
                        Plugin.log.Info($"Patched {method.DeclaringType}.{method.Name}!");
                }
            }

            Plugin.log.Info("Applied Harmony patches!");

        }
    }
}

namespace BeatSaber99Client.OverriddenClasses
{
    [HarmonyPatch(typeof(PauseController))]
    [HarmonyPatch("Pause")]
    public class GameplayManagerPausePatch
    {
        public static bool Prefix(StandardLevelGameplayManager __instance, PauseMenuManager ____pauseMenuManager)
        {
            try
            {
                if (Client.Status == ClientStatus.Playing)
                {
                    ____pauseMenuManager.ShowMenu();
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                Plugin.log.Error("Exception in Harmony patch StandardLevelGameplayManager.Pause: " + e);
                return true;
            }
        }
    }
}