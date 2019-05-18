namespace LiveCoverageVsPlugin.UI
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("7bb909bd-2478-4f88-b4f0-f51acf28dbca")]
    public class CoverageOverviewSettings : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoverageOverviewSettings"/> class.
        /// </summary>
        public CoverageOverviewSettings() : base(null)
        {
            this.Caption = "CoverageOverviewSettings";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new CoverageOverviewSettingsControl();
        }
    }
}
