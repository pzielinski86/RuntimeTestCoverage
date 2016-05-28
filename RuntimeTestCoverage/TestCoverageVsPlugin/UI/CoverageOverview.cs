//------------------------------------------------------------------------------
// <copyright file="CoverageOverview.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace TestCoverageVsPlugin.UI
{
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.Runtime.InteropServices;

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
    [Guid("a8a06750-76ab-40a4-aeae-df816a31862a")]
    public class CoverageOverview : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoverageOverview"/> class.
        /// </summary>
        public CoverageOverview() : base(null)
        {
            this.Caption = "Coverage Overview";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new CoverageOverviewControl();
        }
    }
}
