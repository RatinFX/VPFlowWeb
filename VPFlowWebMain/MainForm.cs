using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using ScriptPortal.Vegas;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VPFlowWebMain
{
    public partial class MainForm : UserControl
    {
        private readonly Vegas _vegas;
        private WebView2 webVPFlow;

        public MainForm(Vegas myVegas)
        {
            _vegas = myVegas;
            InitializeComponent();

            webVPFlow = new WebView2
            {
                Dock = DockStyle.Fill,
            };

            Controls.Add(webVPFlow);

            // Fire-and-forget initialization (it's safe for UI init). Use ConfigureAwait(false) when appropriate.
            _ = InitializeWebViewAsync();
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
                    webVPFlow.CoreWebView2.Navigate("http://localhost:5173/");
                    return;
                }

                // No runtime found — fallback
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
            }
            catch (DllNotFoundException dllEx)
            {
                MessageBox.Show("WebView2 native loader missing or architecture mismatch. Ensure x64 fixed runtime matches Vegas x64.\nDetails: " + dllEx.Message, "WebView2", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("WebView2 initialization failed: " + ex.Message, "WebView2", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
