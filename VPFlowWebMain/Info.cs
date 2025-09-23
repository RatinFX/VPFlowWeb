using System;
using System.Reflection;

namespace VPFlowWebMain
{
    public static class Info
    {
        public static string InsanceName { get; } = "IVPFlowWebMain";
        public static string Name { get; } = "VPFlow Web Test";
        public static Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;
        public static string CurrentVersion { get; } = $"{Version.Major}.{Version.Minor}.{Version.Build}";
        public static string DisplayName { get; } = $"{Name} {CurrentVersion}";
    }
}
