using System;
using System.IO;
using System.Reflection;

namespace VPFlowWebMain.Config
{
    public static class Parameters
    {
        public static string MainFolder(string subFolder = "") => Path.Combine("VPFlowWeb", subFolder);

        public static string InsanceName { get; } = "IVPFlowWeb";
        public static string Name { get; } = "VPFlow Web Test";
        public static string GitHubRepoName { get; } = "VPFlowWeb";

        public static Version Version { get; } = Assembly.GetExecutingAssembly().GetName().Version;
        public static string CurrentVersion { get; } = $"{Version.Major}.{Version.Minor}.{Version.Build}";
        public static string DisplayName { get; } = $"{Name} {CurrentVersion}";

        public static string LatestVersion { get; set; }
        /// <summary>
        /// Error, if this is not null
        /// </summary>
        public static string LatestVersionInfo { get; set; }

        public static string SettingsFileName { get; } = "Settings";
    }
}
