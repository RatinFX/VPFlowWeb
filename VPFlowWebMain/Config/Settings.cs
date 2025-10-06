using RatinFX.VP.Helpers;

namespace VPFlowWebMain.Config
{
    internal class Settings
    {
        public string Theme { get; set; } = "dark";
        public bool DisplayLogs { get; set; } = true;
        public bool CheckForUpdatesOnStart { get; set; } = true;
        public bool IgnoreLongSectionWarning { get; set; } = false;
        public bool OnlyCreateNecessaryKeyframes { get; set; } = true;

        public long LastChecked { get; set; } = -1;
        public string LatestVersion { get; set; } = "0.0.0";

        public Settings() { }

        public Settings(bool init)
        {
            if (!init) return;

            var config = FileHandler.LoadVPConfig(this, Parameters.MainFolder(Parameters.SettingsFileName));
            if (config != null)
            {
                Theme = config.Theme;
                DisplayLogs = config.DisplayLogs;
                CheckForUpdatesOnStart = config.CheckForUpdatesOnStart;
                IgnoreLongSectionWarning = config.IgnoreLongSectionWarning;
                OnlyCreateNecessaryKeyframes = config.OnlyCreateNecessaryKeyframes;

                LastChecked = config.LastChecked;
                LatestVersion = config.LatestVersion;
            }

            Save();
        }

        public void Save()
        {
            FileHandler.SaveVPConfig(this, Parameters.MainFolder(Parameters.SettingsFileName));
        }
    }
}
