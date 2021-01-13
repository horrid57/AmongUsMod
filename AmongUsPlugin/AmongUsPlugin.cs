using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Reactor;
using System.Linq;
using Hazel;
using Il2CppSystem.Runtime.Remoting.Messaging;
using PluginExperiments.Roles;
using UnhollowerBaseLib;
using System.Collections.Generic;
using Il2CppSystem.Runtime.Remoting;
using UnityEngine;
using UnityEngine.LowLevel;
using KeyCode = BepInEx.IL2CPP.UnityEngine.KeyCode;

// IntroCutscene.Method_0 == Crewmate
// IntroCutscene.Method_15 == Impostor

// MeetingHud.CreateButton = Method_108 or Method_7


// Jester can be given to imposter, win screen doesn't show correctly


public enum CustomRpcMessages
{
    SET_CUSTOM_ROLES = 64,
}


namespace PluginExperiments
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class AmongUsPlugin : BasePlugin
    {
        public const string Id = "AmongUs.AmongUsPlugin";

        //public const int CUSTOM_ROLE_COUNT = 1;

        public Harmony Harmony { get; } = new Harmony(Id);

        public override void Load()
        {
            Harmony.PatchAll();
        }

        public static Dictionary<byte, CustomRole[]> customRoles = new Dictionary<byte, CustomRole[]>();

        public static CustomRole GameWinner = null;


        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
        public static class RpcSetInfected
        {
            public static void Postfix(PlayerControl __instance, Il2CppStructArray<byte> JPGEIBIBJPJ)
            {
                var infected = JPGEIBIBJPJ;
                var playersToGiveRoles = new List<GameData.PlayerInfo>();
                foreach (var p in GameData.Instance.AllPlayers)
                {
                    if (!p.IsImpostor)
                    {
                        playersToGiveRoles.Add(p);
                        System.Console.WriteLine(p.PlayerName);
                    }
                }

                var rolesToAssign = CustomRole.Roles;

                var messageWriter = AmongUsClient.Instance.StartRpc(__instance.NetId,
                    (int) CustomRpcMessages.SET_CUSTOM_ROLES, SendOption.Reliable);
                messageWriter.Write((byte) rolesToAssign.Length);
                var customRoleData = new byte[rolesToAssign.Length];
                customRoles.Clear();
                foreach (var role in rolesToAssign)
                {
                    System.Console.WriteLine(role.Name);
                    var index = HashRandom.Method_1(playersToGiveRoles.Count);
                    customRoles.Add(playersToGiveRoles[index].PlayerId, new Roles.CustomRole[] {role});
                    messageWriter.Write(playersToGiveRoles[index].PlayerId);
                    messageWriter.WriteBytesAndSize(new byte[] {(byte) role.RoleId});
                    //messageWriter.Write(role.RoleId);
                    playersToGiveRoles.RemoveAt(index);
                }
                messageWriter.EndMessage();
                GameWinner = null;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        public static class PlayerHandleRpc
        {
            public static void Postfix(byte HKHMBLJFLMC, MessageReader ALMCIJKELCP)
            {
                var writer = ALMCIJKELCP;
                switch ((CustomRpcMessages) HKHMBLJFLMC)
                {
                    case CustomRpcMessages.SET_CUSTOM_ROLES:
                        customRoles.Clear();
                        var length = writer.ReadByte();
                        for (var i = 0; i < length; i++)
                        {
                            var playerId = writer.ReadByte();
                            var player = GameData.Instance.GetPlayerById(playerId).Object;
                            var roleIds = writer.ReadBytesAndSize();
                            var roles = new Roles.CustomRole[roleIds.Length];
                            for (var j = 0; j < roleIds.Length; j++)
                            {
                                roles[j] = Roles.CustomRole.Roles[j];
                            }

                            customRoles.Add(player.PlayerId, roles);
                        }
                        if (customRoles.ContainsKey(PlayerControl.LocalPlayer.PlayerId))
                        {
                            var role = customRoles[PlayerControl.LocalPlayer.PlayerId];
                            PlayerControl.LocalPlayer.nameText.Color = role[0].Colour;
                        }
                        return;
                    default:
                        return;
                }
            }
        }


        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Method_108))]
        public static class OverwriteMeetingColour
        {
            public static void Postfix(GameData.PlayerInfo PPIKPNJEAKJ,
                ref PlayerVoteArea __result)
            {
                if (PPIKPNJEAKJ.Object.AmOwner && customRoles.ContainsKey(PPIKPNJEAKJ.PlayerId))
                {
                    var role = customRoles[PlayerControl.LocalPlayer.PlayerId];
                    __result.NameText.Color = role[0].Colour;
                    
                }
            }
        }
        

        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
        public static class EndGameManagerPatch
        {
            public static void Prefix()
            {
                if (GameWinner != null)
                {
                    TempData.winners = GameWinner.GetWinners(customRoles);
                }
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.Method_0))]
        public static class IntroCutscenePatch
        {
            public static void Prefix(ref Il2CppSystem.Collections.Generic.List<PlayerControl> KADFCNPGKLO)
            {
                if (customRoles.ContainsKey(PlayerControl.LocalPlayer.PlayerId))
                {
                    KADFCNPGKLO.Clear();
                    KADFCNPGKLO.Add(PlayerControl.LocalPlayer);
                }
            }
            
            public static void Postfix(IntroCutscene __instance)
            {
                if (customRoles.ContainsKey(PlayerControl.LocalPlayer.PlayerId))
                {
                    var role = customRoles[PlayerControl.LocalPlayer.PlayerId];
                    __instance.BackgroundBar.material.SetColor("_Color", role[0].Colour);
                    __instance.Title.Text = role[0].Name;
                    __instance.Title.Color = role[0].Colour;
                    __instance.ImpostorText.Text = role[0].ImpostorText;
                }
            }
        }
    }
}

