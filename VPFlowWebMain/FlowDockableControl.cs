using ScriptPortal.Vegas;
using System;
using System.Collections;
using System.Drawing;

namespace VPFlowWebMain
{
    public sealed class FlowDockableControl : DockableControl
    {
        private MainForm _mainControl;

        public FlowDockableControl(CustomCommand cc) : base(Info.InsanceName)
        {
            DisplayName = Info.DisplayName;
            AutoLoadCommand = cc;
            PersistDockWindowState = true;
        }

        public override DockWindowStyle DefaultDockWindowStyle => DockWindowStyle.Floating;

        public override Size DefaultFloatingSize => new Size(225, 515);

        protected override void OnLoad(EventArgs e)
        {
            _mainControl = new MainForm(myVegas)
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
            };

            Controls.Add(_mainControl);

            // track event state changed
            // track state changed
            // force focus main track view

            base.OnLoad(e);
        }

        protected override void OnClosed(EventArgs args)
        {
            // track event state changed
            // track state changed
            // force focus main track view

            // mainControl remove event listeners

            base.OnClosed(args);
        }
    }

    /// <summary>
    /// Hidden from Toolbar and Keybindings
    /// </summary>
    public class VPFlowWebMainEditModule : VPFlowWebMainCommandModule
    {
        public VPFlowWebMainEditModule() : base(CommandCategory.Edit)
        {
            cc.CanAddToToolbar = false;
            cc.CanAddToKeybindings = false;
        }
    }

    /// <summary>
    /// Hidden from Toolbar and Keybindings
    /// </summary>
    public class VPFlowWebMainViewModule : VPFlowWebMainCommandModule
    {
        public VPFlowWebMainViewModule() : base(CommandCategory.View)
        {
            cc.CanAddToToolbar = false;
            cc.CanAddToKeybindings = false;
        }
    }

    /// <summary>
    /// The only one visible in Toolbar and Keybindings
    /// </summary>
    public class VPFlowWebMainToolsModule : VPFlowWebMainCommandModule
    {
        public VPFlowWebMainToolsModule() : base(CommandCategory.Tools) { }
    }

    /// <summary>
    /// Base CCM for VPFlowWebMain
    /// </summary>
    public class VPFlowWebMainCommandModule : ICustomCommandModule
    {
        internal readonly CustomCommand cc;
        internal Vegas myVegas;

        public VPFlowWebMainCommandModule(CommandCategory category)
        {
            cc = SetCustomCommand(category);
        }

        private CustomCommand SetCustomCommand(CommandCategory category)
        {
            return new CustomCommand(category, Info.DisplayName)
            {
                MenuItemName = Info.DisplayName,
            };
        }

        public void InitializeModule(Vegas vegas)
        {
            myVegas = vegas;
        }

        public ICollection GetCustomCommands()
        {
            cc.MenuPopup += HandleMenuPopup;
            cc.Invoked += HandleInvoked;

            return new[] { cc };
        }

        private void HandleMenuPopup(object sender, EventArgs e)
        {
            cc.Checked = myVegas.FindDockView(Info.InsanceName);
        }

        private void HandleInvoked(object sender, EventArgs e)
        {
            if (myVegas.ActivateDockView(Info.InsanceName))
                return;

            var dock = new FlowDockableControl(cc);
            myVegas.LoadDockView(dock);
        }
    }
}
