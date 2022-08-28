using System;
using UnityEngine;
using MelonLoader;
using HarmonyLib;

namespace AutoReadyUp
{
    [HarmonyPatch(typeof(LocalizationManager), "GetLocalizedValue", new Type[]
    { typeof(string), typeof(string)})]
    public static class GetLocalizedValuePatch
    {
        public static void Postfix(LocalizationManager __instance, string __result, string __0, string __1)
        {
            if (__0 == "YOUJOINEDTHELOBBY")
            {
                Conditions.isPlayerInLobby = true;
            }
        }
    }

    [HarmonyPatch(typeof(LobbyUiManager), "OnChatLeft")]
    public static class OnChatLeftPatch
    {
        public static void Postfix()
        {
            Conditions.isPlayerInLobby = false;
        }
    }

    [HarmonyPatch(typeof(LobbyCounterHelper), "setCounter", new Type[] { typeof(int) })]
    public static class SetCounterPatch
    {
        public static int playerCount;

        [HarmonyPostfix]
        public static void SetPlayerCount(LobbyCounterHelper __instance, int __0)
        {
            playerCount = __0;
        }
    }

    [HarmonyPatch(typeof(LobbyUiManager), "Update")]
    public static class LobbyUiManagerUpdatePatch
    {
        public static void Postfix(LobbyUiManager __instance)
        {
            if (Conditions.isPlayerInLobby == true)
            {
                if (Conditions.didPlayerDecided == true && Conditions.activateMod == false)
                {
                    Conditions.activateMod = true;
                    Terminal.Log("Activated mod for current lobby!");
                }
                else if (Conditions.didPlayerDecided == false && Conditions.activateMod == true)
                {
                    Conditions.activateMod = false;
                    Terminal.Log("Deactivated mod for current lobby!");
                }
            }

            if (Conditions.activateMod == true && SetCounterPatch.playerCount >= 10)
            {
                __instance.OnSetReadyButtonClicked();
                Conditions.didPlayerDecided = false;
                Conditions.activateMod = false;
                Terminal.Log("Readied up successfuly! Deactivated mod for current lobby.");
            }
        }
    }

    [HarmonyPatch(typeof(TerminalHelper), "Update")]
    public class TerminalHelperUpdatePatch
    {
        public static string message = null;
        public static TerminalHelper terminalHelper;

        public static void Postfix(TerminalHelper __instance)
        {
            terminalHelper = __instance;
            if (Conditions.isInitialLaunch == true)
            {
                Conditions.isInitialLaunch = false;
                string[] messages = {
                    "Hello there!",
                    "Currently mod is activated for any lobby you will join.",
                    "You can (de)activate mod by pressing TAB.",
                    "Have fun ;D"
                };

                foreach (string msg in messages)
                {
                    Terminal.Log(msg);
                }
            }
        }
    }

    public static class Terminal
    {
        public static void Log(string message)
        {
            TerminalHelperUpdatePatch.terminalHelper.AddASCsystemMessage
                ("<AutoReadyUpMod> " + message);
        }
    }

    public static class Conditions
    {
        public static bool isInitialLaunch = true;
        public static bool didPlayerDecided = true;
        public static bool activateMod = false;
        public static bool isPlayerInLobby = false;
    }

    public class ModClass : MelonMod
    {
        public override void OnApplicationLateStart()
        {
            MelonLogger.Msg("Mod started successfully.");
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (Conditions.isPlayerInLobby == false)
                {
                    if (Conditions.didPlayerDecided == false)
                    {
                        Conditions.didPlayerDecided = true;
                        Terminal.Log("Activated mod for next lobby!");
                    }
                    else if (Conditions.didPlayerDecided == true)
                    {
                        Conditions.didPlayerDecided = false;
                        Terminal.Log("Deactivated mod for next lobby!");
                    }
                }
            }
        }
    }
}

