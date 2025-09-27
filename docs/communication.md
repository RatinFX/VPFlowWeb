# Communication between the sides

## C# -> JS

> Either `PostWebMessageAsString` a message or `ExecuteScriptAsync`

### WinForms

```c#
// 1. Messages
// - POST as a message with a json payload
// - "payload": object to pass
var json = JsonConvert.SerializeObject(payload);
webView2.CoreWebView2.PostWebMessageAsString(json);

// 2. Named functions
//    {0}: function name
//    {1}: payload as json
webView2.ExecuteScriptAsync(string.Format(
    // `window.methodName(properties)`
    "window.{0}('{1}')",
    "receiveFromHost",
    json
));
```

### Web

```ts
onMounted(() => {
  // 1. Messages
  (window as any).chrome.webview.addEventListener("message", (e: any) => {
    // could do `if (typeof data === "string")`
    const data = JSON.parse(e.data);

    // handle based on the fields
    if (data?.type === "dataUpdate") {
      // ...
    }
  });

  // 2. Named functions
  // - extend `window` by interfaces for types
  (window as any).receiveFromHost = (data: any) => {
    // ...
  };
});
```

## JS -> C#

> `webview.postMessage(payload)` seems to be perfect here

### Web

```ts
const payload = {
  // ...data
};
(window as any).chrome.webview.postMessage(payload);
```

### WinForms

```c#
// ...
webView2.WebMessageReceived += WebView2_WebMessageReceived;
// ...

void WebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e) {
  string raw = e.WebMessageAsJson;
  // - make `object` generic for types
  var msg = JsonConvert.DeserializeObject<Dictionary<string, object>>(raw);
}
```
