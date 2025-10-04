using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace VPFlowWebMain
{
    internal static class WebView2Extensions
    {
        public static Task<string> RunScript(this WebView2 webView2, object payload, [CallerMemberName] string callerName = "ERROR_NO_NAME")
        {
            Logging.Log("Executing script: " + callerName);
            return webView2.ExecuteScriptAsync(string.Format(
                "window.{0}('{1}')",
                callerName,
                JsonConvert.SerializeObject(payload)
            ));
        }

        public static Task<string> ReceiveFromHost(this WebView2 webView2, object payload)
        {
            return webView2.RunScript(payload);
        }
    }
}
