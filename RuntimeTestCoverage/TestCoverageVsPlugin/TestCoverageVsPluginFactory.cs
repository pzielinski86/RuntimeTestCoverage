using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace TestCoverageVsPlugin
{
    #region TestCoverageVsPlugin Factory
    /// <summary>
    /// Export a <see cref="IWpfTextViewMarginProvider"/>, which returns an instance of the margin for the editor
    /// to use.
    /// </summary>
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name(TestCoverageVsPlugin.MarginName)]
    [Order(After = PredefinedMarginNames.LeftSelection)]
    [MarginContainer(PredefinedMarginNames.LeftSelection)] 
    [ContentType("text")] //Show this margin for all text-based types
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class MarginFactory : IWpfTextViewMarginProvider
    {
        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            return new TestCoverageVsPlugin(textViewHost.TextView);
        }
    }
    #endregion
}
