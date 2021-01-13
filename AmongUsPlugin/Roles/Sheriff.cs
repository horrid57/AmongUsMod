using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Hazel;

namespace PluginExperiments.Roles
{
    public class Sheriff : CustomRole
    {
        private static Sheriff _instance;

        public Sheriff()
        {
            _instance = this;
            RoleId = 1;
            Name = "Sheriff";
            Colour = new Color(0.961f, 0.525f, 0f);
            ImpostorText = "Shoot the impostor.";
        }

        public override string ToString()
        {
            return this.Name;
        }

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
        public static class ExiledTextPatch
        {
            public static void Postfix(ExileController __instance, [HarmonyArgument(0)] GameData.PlayerInfo exiled)
            {
                AmongUsPlugin.customRoles.TryGetValue(exiled.PlayerId, out var roles);
                if (roles.Contains(_instance))
                {
                    //__instance.Field_9 = exiled.PlayerName + " was the " + Instance.Name;
                    __instance.ImpostorText.Text = "But you shot the " + _instance.Name;
                }
            }
        }

        public override Il2CppSystem.Collections.Generic.List<WinningPlayerData> GetWinners(Dictionary<byte, CustomRole[]> customRoles)
        {
            var list = new Il2CppSystem.Collections.Generic.List<WinningPlayerData>();

            foreach (var player in GameData.Instance.AllPlayers)
            {
                if (!player.IsImpostor)
                {
                    list.Add(new WinningPlayerData
                    {
                        IsYou = player.PlayerId == PlayerControl.LocalPlayer.PlayerId,
                        Name = player.PlayerName,
                        ColorId = player.ColorId,
                        SkinId = player.SkinId,
                        IsImpostor = player.IsImpostor,
                        IsDead = player.IsDead,
                        PetId = player.PetId,
                        HatId = player.HatId,
                    });
                }
            }
            return list;
        }
    }
}