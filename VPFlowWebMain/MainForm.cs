using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using ScriptPortal.Vegas;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using VPFlowWebMain.Config;
using VPFlowWebMain.Controllers;
using VPFlowWebMain.Lib;
using VPFlowWebMain.Models;

namespace VPFlowWebMain
{
    public partial class MainForm : UserControl
    {
        public static MainForm Instance { get; set; }

        private WebView2 webVPFlow;
        private readonly Vegas _vegas;

        public TextBox LogArea => tbxLog;

        private bool _blockSettingsUpdate = true;

        public MainForm(Vegas myVegas)
        {
            try
            {
                Instance = this;
                _vegas = myVegas;

                InitializeComponent();

                Logging.Log("Loading WebView2...");
                webVPFlow = new WebView2
                {
                    Dock = DockStyle.Fill,
                };

                gbxWebView.Controls.Add(webVPFlow);
            }
            catch (Exception ex)
            {
                Logging.Error($"Something went wrong during init: {ex.Message}");
            }
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await InitializeWebViewAsync();
        }

        private async Task InitializeWebViewAsync()
        {
            try
            {
                var asmFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                    ?? AppDomain.CurrentDomain.BaseDirectory;

                var fixedFolder = Path.Combine(asmFolder, "WebView2FixedRuntime");

                // Always use a writable user-data folder under LocalAppData (unique per plugin)
                var userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "VPFlowWebMain",
                    "WebView2UserData"
                );

                Directory.CreateDirectory(userDataFolder);

                string available = null;
                try
                {
                    // Query installed runtime (returns null/throws if not found)
                    available = CoreWebView2Environment.GetAvailableBrowserVersionString();
                }
                catch { /* ignore: not installed */ }

                CoreWebView2Environment env = null;

                if (!string.IsNullOrEmpty(available))
                {
                    // Use installed runtime, but still create an environment that uses LocalAppData for user-data
                    env = await CoreWebView2Environment.CreateAsync(
                        browserExecutableFolder: null,
                        userDataFolder: userDataFolder
                    );
                }
                else if (Directory.Exists(fixedFolder))
                {
                    // Use fixed runtime bundled next to the assembly
                    env = await CoreWebView2Environment.CreateAsync(
                        browserExecutableFolder: fixedFolder,
                        userDataFolder: userDataFolder
                    );
                }

                if (env != null)
                {
                    // EnsureCoreWebView2Async must be called on UI thread
                    await webVPFlow.EnsureCoreWebView2Async(env);

                    // handle messages
                    webVPFlow.WebMessageReceived += WebVPFlow_WebMessageReceived;
                    webVPFlow.NavigationCompleted += WebVPFlow_NavigationCompleted;

                    webVPFlow.CoreWebView2.Navigate("http://localhost:5173/");
                    return;
                }

                // No runtime found
                MessageBox.Show(
                    "WebView2 runtime not found",
                    "WebView2 missing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                webVPFlow?.Dispose();
                webVPFlow = null;
            }
            catch (UnauthorizedAccessException uaEx)
            {
                MessageBox.Show("WebView2 initialization failed: access denied.\nEnsure the user-data folder is writable.\nDetails: " + uaEx.Message, "WebView2", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logging.Error("WebView2 initialization failed: access denied. Ensure the user-data folder is writable. Details: " + uaEx.Message);
            }
            catch (DllNotFoundException dllEx)
            {
                MessageBox.Show("WebView2 native loader missing or architecture mismatch. Ensure x64 fixed runtime matches Vegas x64.\nDetails: " + dllEx.Message, "WebView2", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logging.Error("WebView2 native loader missing or architecture mismatch. Ensure x64 fixed runtime matches Vegas x64. Details: " + dllEx.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("WebView2 initialization failed: " + ex.Message, "WebView2", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logging.Error("WebView2 initialization failed: " + ex.Message);
            }
        }

        private void WebVPFlow_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var (messageType, payload) = Messaging.Process(e.WebMessageAsJson);

                if (messageType is null || payload is null)
                {
                    return;
                }

                if (messageType is MessageType.Apply)
                {
                    HandleApply(payload);
                    return;
                }

                if (messageType is MessageType.Settings)
                {
                    HandleSettingsChanged(payload);
                    return;
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Web message handling error: " + ex.Message);
            }
        }

        private void WebVPFlow_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            this.Invoke((Action)(async () =>
            {
                // Set Language
                //Logging.Log("Changing language...");
                //SetDisplayedTexts();

                // Events
                //Logging.Log("Setting Event handlers...");
                //AddEventListeners();

                // Settings
                Logging.Log("Loading Settings...");
                await ReloadSettings();
                _blockSettingsUpdate = false;
            }));
        }

        internal void HandleApply(object payload)
        {
            var webMessage = Messaging.CreateWebMessage<ApplyPayload>(MessageType.Apply, payload);
        }

        internal void HandleSettingsChanged(object payload)
        {
            var webMessage = Messaging.CreateWebMessage<SettingsPayload>(MessageType.Settings, payload);

            Run(() =>
            {
                SettingsController.Instance.SetTheme(webMessage.Payload.theme);
                SettingsController.Instance.SetDisplayLogs(webMessage.Payload.displayLogs);
                SettingsController.Instance.SetCheckForUpdatesOnStart(webMessage.Payload.checkForUpdatesOnStart);
                SettingsController.Instance.SetIgnoreLongSectionWarning(webMessage.Payload.ignoreLongSectionWarning);
                SettingsController.Instance.SetOnlyCreateNecessaryKeyframes(webMessage.Payload.onlyCreateNecessaryKeyframes);
                SettingsController.Instance.SetSelectedMode(webMessage.Payload.selectedMode);
            });
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await webVPFlow.receiveItems(new[] { "A", "B", "C" });
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await LoadSettings();
        }

        /// <summary>
        /// Sets the state of buttons and checkboxes
        /// </summary>
        private async Task LoadSettings()
        {
            var payload = new SettingsPayload
            {
                theme = SettingsController.Instance.Theme(),
                displayLogs = SettingsController.Instance.DisplayLogs(),
                ignoreLongSectionWarning = SettingsController.Instance.IgnoreLongSectionWarning(),
                onlyCreateNecessaryKeyframes = SettingsController.Instance.OnlyCreateNecessaryKeyframes(),
                checkForUpdatesOnStart = SettingsController.Instance.CheckForUpdatesOnStart(),
                selectedMode = SettingsController.Instance.SelectedMode(),
            };

            await webVPFlow.receiveSettings(payload);
        }

        private async Task ReloadSettings()
        {
            await LoadSettings();

            // change tab to tab from settings
            // ChangeCurveTab(SettingsController.Instance.CurrentCurveTab());

            CheckForUpdates();
        }

        private void CheckForUpdates(bool forceUpdate = false)
        {
            // Update check enabled
            if (forceUpdate ||
                (SettingsController.Instance.CheckForUpdatesOnStart()
            //&& SettingsController.Instance.ShouldCheckForUpdate()
            ))
            {
                Logging.Log("Checking for update");

                // TODO: enable update checks
                //RatinFX.VP.Helpers.Helper.CheckForUpdate(
                //    Parameters.GitHubRepoName,
                //    Parameters.CurrentVersion,
                //    latest => Parameters.LatestVersion = latest,
                //    info => Parameters.LatestVersionInfo = info
                //);

                //SettingsController.Instance.SetLastChecked(Parameters.LatestVersion);

                SettingsController.Instance.SetLastChecked(Parameters.CurrentVersion);
            }

            // Update available, display text
            if (!SettingsController.Instance.UsingLatestVersion() && !string.IsNullOrEmpty(Parameters.LatestVersion))
            {
                Logging.Log($"Update available: {Parameters.CurrentVersion} -> {Parameters.LatestVersion}");
                // New version available: Parameters.CurrentVersion -> Parameters.LatestVersion
            }

            // Error
            if (!string.IsNullOrEmpty(Parameters.LatestVersionInfo))
            {
                Logging.Error(Parameters.LatestVersionInfo);
            }
        }

        /// <summary>
        /// Checks for `_blockSettingsUpdate` before any action,
        /// mainly for <see cref="LoadSettings"/>
        /// </summary>
        private void Run(Action action)
        {
            if (_blockSettingsUpdate)
                return;

            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Logging.Error($"Something went wrong: {ex.Message}");
            }
        }
    }
}
