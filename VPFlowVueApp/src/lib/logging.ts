import store from "../store";

const MAX_LOG_LINES = 50;

function format(data: any[]) {
  return data
    .map((d) => (typeof d === "string" ? `"${d}"` : JSON.stringify(d)))
    .join(" ");
}

function trim(text: string) {
  const lines = text.split(/\r?\n/);
  if (lines.length <= MAX_LOG_LINES) {
    return text;
  }
  return lines.slice(-MAX_LOG_LINES).join("\r\n");
}

function commit(prefix: string, data: any[]) {
  const entry = prefix + format(data);
  const newText = store.logs.value + "\r\n" + entry;
  store.logs.value = trim(newText);
}

export function log(...data: any[]) {
  console.log(...data);
  commit("> ", data);
}

export function warn(...data: any[]) {
  console.warn(...data);
  commit("[!] ", data);
}

export function error(...data: any[]) {
  console.error(...data);
  commit("[ERROR] ", data);
}
