export class Menu {
  label: string = "";
  action?: Function = () => {};
  items?: MenuItem[] = [];
}

export class MenuItem {
  label?: string = "";
  action?: Function = undefined;
  isSeparator?: boolean = false;
  isCheckboxItem?: boolean = false;
  checkboxValue?: boolean = false;
}
