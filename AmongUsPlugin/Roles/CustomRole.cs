using System.Collections.Generic;
using PluginExperiments.Roles;

namespace PluginExperiments.Roles
{
    public abstract class CustomRole
    {
        public byte RoleId;
        public string Name;
        public UnityEngine.Color Colour;
        public string ImpostorText;

        public static readonly CustomRole[] Roles = {
            //new Sheriff(),
            new Jester(),
        };

        public abstract Il2CppSystem.Collections.Generic.List<WinningPlayerData> GetWinners(Dictionary<byte, CustomRole[]> customRoles);
    }
}