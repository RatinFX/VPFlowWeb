import type { MenuItem } from "./MenuItem";

export class Menu {
  label: string = "";
  action?: Function = () => {};
  items?: MenuItem[] = [];
}
