using VPFlowWebMain.Config;

namespace VPFlowWebMain.Controllers
{
    internal class SettingsController
    {
        private static SettingsController _instance;
        internal static SettingsController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SettingsController();
                }
                return _instance;
            }
        }

        private static Settings Settings { get; } = new Settings(true);

        ///
        public string Theme()
        {
            return Settings?.Theme ?? "dark";
        }

        public void SetTheme(string theme)
        {
            Settings.Theme = theme;
            Settings.Save();
        }

        ///
        public bool DisplayLogs()
        {
            return Settings?.DisplayLogs ?? true;
        }

        public void SetDisplayLogs(bool @checked)
        {
            Settings.DisplayLogs = @checked;
            Settings.Save();
        }

        ///
        public bool CheckForUpdatesOnStart()
        {
            return Settings?.CheckForUpdatesOnStart ?? false;
        }

        public void SetCheckForUpdatesOnStart(bool @checked)
        {
            Settings.CheckForUpdatesOnStart = @checked;
            Settings.Save();
        }

        ///
        public bool IgnoreLongSectionWarning()
        {
            return Settings?.IgnoreLongSectionWarning ?? false;
        }

        public void SetIgnoreLongSectionWarning(bool @checked)
        {
            Settings.IgnoreLongSectionWarning = @checked;
            Settings.Save();
        }

        ///
        public bool OnlyCreateNecessaryKeyframes()
        {
            return Settings?.OnlyCreateNecessaryKeyframes ?? true;
        }

        public void SetOnlyCreateNecessaryKeyframes(bool @checked)
        {
            Settings.OnlyCreateNecessaryKeyframes = @checked;
            Settings.Save();
        }

        ///
        public bool ShouldCheckForUpdate()
        {
            return (RatinFX.VP.Helpers.Helper.ShouldCheckForUpdate(Settings.LastChecked)
                && !UsingLatestVersion())
                || Settings.LatestVersion == "0.0.0"
                || string.IsNullOrEmpty(Settings.LatestVersion);
        }

        public void SetLastChecked(string latest)
        {
            Settings.LastChecked = RatinFX.VP.Helpers.Helper.GetCurrentUnixTime();
            Settings.LatestVersion = latest;
            Settings.Save();
        }

        public bool UsingLatestVersion()
        {
            return Parameters.CurrentVersion == Settings.LatestVersion;
        }
    }
}
