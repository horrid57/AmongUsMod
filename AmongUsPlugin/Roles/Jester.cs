using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Hazel;

namespace PluginExperiments.Roles
{
    public class Jester : CustomRole
    {
        private static Jester _instance;
        
        public Jester()
        {
            _instance = this;
            RoleId = 0;
            Name = "Jester";
            Colour = new Color(0.961f, 0.525f, 0.961f);
            ImpostorText = "Get voted out to win";
        }

        public override string ToString()
        {
            return this.Name;
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
        public static class ExiledPlayerPatch
        {
            public static void Prefix(PlayerControl __instance)
            {
                AmongUsPlugin.customRoles.TryGetValue(__instance.PlayerId, out var roles);
                if (roles != null && roles.Contains(_instance))
                {
                    AmongUsPlugin.GameWinner = _instance;
                    ShipStatus.RpcEndGame(GameOverReason.ImpostorByVote, false);
                }
            }
        }

        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
        public static class ShowNormalMapPatch
        {
            public static void Prefix(MapBehaviour __instance)
            {
                __instance.ColorControl.SetColor(Palette.Purple);
            }
        }
        
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
        public static class ExiledTextPatch
        {
            public static void Postfix(ExileController __instance, [HarmonyArgument(0)] GameData.PlayerInfo exiled)
            {
                AmongUsPlugin.customRoles.TryGetValue(exiled.PlayerId, out var roles);
                if (roles != null && roles.Contains(_instance))
                {
                    //__instance.Field_9 = exiled.PlayerName + " was the " + Instance.Name;
                    __instance.ImpostorText.Text = "But they were the " + _instance.Name;
                }
            }
        }
        
        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
        public static class JesterLossPatch
        {
            public static void Prefix()
            {
                if (AmongUsPlugin.GameWinner != _instance)
                {
                    foreach (var playerId in AmongUsPlugin.customRoles.Keys)
                    {
                        if (AmongUsPlugin.customRoles[playerId].Contains(_instance))
                        {
                            for (int i = 0; i < TempData.winners.Count; i++)
                            {
                                if (TempData.winners[i].Name == GameData.Instance.GetPlayerById(playerId).PlayerName)
                                {
                                    TempData.winners.RemoveAt(i);
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetTasks))]
        public static class AddFakeTaskText
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (__instance.AmOwner)
                {
                    AmongUsPlugin.customRoles.TryGetValue(__instance.PlayerId, out var roles);
                    if (roles != null && roles.Contains(_instance))
                    {
                        ImportantTextTask importantTextTask =
                            new GameObject("_Player").AddComponent<ImportantTextTask>();
                        importantTextTask.transform.SetParent(PlayerControl.LocalPlayer.transform, false);
                        importantTextTask.Text = "[F586F5FF]Get voted out to win\r\n[FFFFFFFF]Fake Tasks:";
                        __instance.myTasks.Insert(0, importantTextTask);

                        GameData.Instance.GetPlayerById(PlayerControl.LocalPlayer.PlayerId).Tasks.Clear();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.ValidConsole))]
        public static class DisableTaskPatch
        {
            public static void Postfix(ref bool __result)
            {
                AmongUsPlugin.customRoles.TryGetValue(PlayerControl.LocalPlayer.PlayerId, out var roles);
                if (roles != null && roles.Contains(_instance))
                {
                    __result = false;
                }
            }
        }

        public override Il2CppSystem.Collections.Generic.List<WinningPlayerData> GetWinners(Dictionary<byte, CustomRole[]> customRoles)
        {
            var list = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();
            foreach (var playerId in customRoles.Keys)
            {
                if (AmongUsPlugin.customRoles[playerId].Contains(_instance))
                {
                    var jesterPlayer = GameData.Instance.GetPlayerById(playerId);
                    list.Add(new WinningPlayerData
                    {
                        IsYou = playerId == PlayerControl.LocalPlayer.PlayerId,
                        Name = jesterPlayer.PlayerName,
                        ColorId = jesterPlayer.ColorId,
                        SkinId = jesterPlayer.SkinId,
                        IsImpostor = jesterPlayer.IsImpostor,
                        IsDead = jesterPlayer.IsDead,
                        PetId = jesterPlayer.PetId,
                        HatId = jesterPlayer.HatId,
                    });
                }
            }

            return list;
        }
    }
}