using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VPFlowWebMain.Lib;
using VPFlowWebMain.Models;

namespace VPFlowWebMain
{
    internal static class WebView2Extensions
    {
        /// <summary>
        /// Runs the JavaScript function named after the caller method,
        /// passing the serialized payload as an argument.
        /// </summary>
        public static Task<string> RunScript(
            this WebView2 webView2,
            object payload,
            [CallerMemberName] string callerName = "ERROR_NO_NAME"
        )
        {
            Logging.Log("Executing script: " + callerName);
            return webView2.ExecuteScriptAsync(string.Format(
                "window.{0}('{1}')",
                callerName,
                JsonConvert.SerializeObject(payload)
            ));
        }

        public static Task<string> receiveSettings(this WebView2 webView2, SettingsPayload payload)
        {
            return webView2.RunScript(payload);
        }

        public static Task<string> receiveItems(this WebView2 webView2, string[] payload)
        {
            return webView2.RunScript(payload);
        }
    }
}
