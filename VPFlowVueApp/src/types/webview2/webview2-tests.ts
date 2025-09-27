/**
 * Test code adapted from https://github.com/MicrosoftEdge/WebView2Samples
 */

let button!: HTMLButtonElement;
let span!: HTMLSpanElement;
let input!: HTMLInputElement;

let chrome = window.chrome;

button.addEventListener("click", async () => {
    const propertyValue = await chrome.webview.hostObjects.sample.property;
    span.textContent = propertyValue;
});

button.addEventListener("click", () => {
    const propertyValue = chrome.webview.hostObjects.sync.sample.property;
    span.textContent = propertyValue;
});

button.addEventListener("click", async () => {
    const propertyValue = input.value;
    // The following line will work but it will return immediately before the property value has actually been set.
    // If you need to set the property and wait for the property to change value, use the setHostProperty function.
    chrome.webview.hostObjects.sample.property = propertyValue;
    span.textContent = "Set";
});

button.addEventListener("click", async () => {
    const propertyValue = input.value;
    // If you care about waiting until the property has actually changed value use the setHostProperty function.
    await chrome.webview.hostObjects.sample.setHostProperty("property", propertyValue);
    span.textContent = "Set";
});

button.addEventListener("click", () => {
    const propertyValue = input.value;
    chrome.webview.hostObjects.sync.sample.property = propertyValue;
    span.textContent = "Set";
});

button.addEventListener("click", async () => {
    const index = parseInt(input.value);
    const resultValue = await chrome.webview.hostObjects.sample.IndexedProperty[index];
    span.textContent = resultValue;
});
button.addEventListener("click", async () => {
    const index = parseInt(input.value);
    const value = input.value;;
    chrome.webview.hostObjects.sample.IndexedProperty[index] = value;
    span.textContent = "Set";
});
button.addEventListener("click", async () => {
    const paramValue1 = input.value;
    const paramValue2 = parseInt(input.value);
    const resultValue = await chrome.webview.hostObjects.sample.MethodWithParametersAndReturnValue(paramValue1, paramValue2);
    span.textContent = resultValue;
});

button.addEventListener("click", () => {
    const paramValue1 = input.value;
    const paramValue2 = parseInt(input.value);
    const resultValue = chrome.webview.hostObjects.sync.sample.MethodWithParametersAndReturnValue(paramValue1, paramValue2);
    span.textContent = resultValue;
});

let callbackCount = 0;
button.addEventListener("click", async () => {
    chrome.webview.hostObjects.sample.CallCallbackAsynchronously(() => {
        span.textContent = "Native object called the callback " + (++callbackCount) + " time(s).";
    });
});

// Date property
button.addEventListener("click", () => {
    chrome.webview.hostObjects.options.shouldSerializeDates = true;
    chrome.webview.hostObjects.sync.sample.dateProperty = new Date();
    span.textContent = "sample.dateProperty: " + chrome.webview.hostObjects.sync.sample.dateProperty;
});
button.addEventListener("click", () => {
    chrome.webview.hostObjects.sync.sample.createNativeDate();
    span.textContent = "sample.dateProperty: " + chrome.webview.hostObjects.sync.sample.dateProperty;
});

//! [HostObjectUsage]

 //! [chromeWebView]
function GetCookies() {
    window.chrome.webview.postMessage(`GetCookies ${input.value}`);
}
function AddOrUpdateCookie() {
    window.chrome.webview.postMessage(`AddOrUpdateCookie`);
}
function DeleteAllCookies() {
    window.chrome.webview.postMessage(`DeleteAllCookies`);
}

let p!: HTMLParagraphElement;

 window.chrome.webview.addEventListener('message', arg => {
    if ("SetColor" in arg.data) {
        p.style.color = arg.data.SetColor;
    }
    if ("WindowBounds" in arg.data) {
        input.value = arg.data.WindowBounds;
    }
});

function SetTitleText() {
    window.chrome.webview.postMessage(`SetTitleText ${input.value}`);
}
function GetWindowBounds() {
    window.chrome.webview.postMessage("GetWindowBounds");
}
//! [chromeWebView]
