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
                Conditions.areStatsShown = false;
            }
        }
    }

    [HarmonyPatch(typeof(LobbyUiManager), "OnDisconnectButtonClicked")]
    public static class OnDisconnectButtonClickedPatch
    {
        public static void Postfix()
        {
            Conditions.isPlayerInLobby = false;
            Conditions.areStatsShown = false;
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
                if (Conditions.areStatsShown == false)
                {
                    if (Conditions.isModActivated == true)
                    {
                        Terminal.Log("Activated for current lobby!");
                    }
                    else if (Conditions.isModActivated == false)
                    {
                        Terminal.Log("Deactivated for current lobby!");
                    }
                    Conditions.areStatsShown = true;
                }

                if (Conditions.didPlayerDecided == true
                    && Conditions.isModActivated == false
                    && Conditions.areStatsShown == true)
                {
                    Conditions.isModActivated = true;
                    Terminal.Log("Activated for current lobby!");
                }
                else if (Conditions.didPlayerDecided == false
                    && Conditions.isModActivated == true
                    && Conditions.areStatsShown == true)
                {
                    Conditions.isModActivated = false;
                    Terminal.Log("Deactivated for current lobby!");
                }
            }

            if (Conditions.isModActivated == true && SetCounterPatch.playerCount >= 10)
            {
                __instance.OnSetReadyButtonClicked();
                Conditions.didPlayerDecided = false;
                Conditions.isModActivated = false;
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
            /* arguments
             * 1 is a type of message:
             *      -1 - system message
             *      -2 - broadcast???
             *      -3 - chat message
             *      -4 - some kind of purple text in the second argument
             *      -5 - asc
             * 2 is initial text
             * 3 is a message
             * 4 is an icon
             * 5 is event icon
             * 6 is text speed
             * 
             * the rest are unknown to me
             */
            TerminalHelperUpdatePatch.terminalHelper.addTerminalChatMessage
                (-5, "<AutoReadyUp>", message, false, false, 0.1f, 0, false, false);
        }
    }

    public static class Conditions
    {
        // theese comments are bad and only intended to remember what are variables for

        // is Untrusted just launched
        public static bool isInitialLaunch = true;

        // player decision about wether to activate mod or not to
        public static bool didPlayerDecided = true;

        // is player in lobby right now
        public static bool isPlayerInLobby = false;

        // is mod activated
        public static bool isModActivated = false;

        // are statistic in lobby alredy shown
        public static bool areStatsShown = false;
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
                if (Conditions.didPlayerDecided == false)
                {
                    Conditions.didPlayerDecided = true;

                    if (Conditions.isPlayerInLobby == false)
                    {
                        Terminal.Log("Activated for next lobby!");
                    }
                }
                else if (Conditions.didPlayerDecided == true)
                {
                    Conditions.didPlayerDecided = false;

                    if (Conditions.isPlayerInLobby == false)
                    {
                        Terminal.Log("Deactivated for next lobby!");
                    }
                }
                
            }
        }
    }
}

