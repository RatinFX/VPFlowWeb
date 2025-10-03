import store from "../store";

function formatLog(data: any[]) {
  return data
    .map((d) => (typeof d === "string" ? `"${d}"` : JSON.stringify(d)))
    .join(" ");
}

export function log(...data: any[]) {
  console.log(...data);
  store.logs.value += "\r\n" + "> " + formatLog(data);
}

export function warn(...data: any[]) {
  console.warn(...data);
  store.logs.value += "\r\n" + "[!] " + formatLog(data);
}

export function error(...data: any[]) {
  console.error(...data);
  store.logs.value += "\r\n" + "[ERROR] " + formatLog(data);
}
