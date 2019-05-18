using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using EnvDTE;
using LiveCoverageVsPlugin.Extensions;
using LiveCoverageVsPlugin.Tasks;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using TestCoverage.CoverageCalculation;
using TestCoverage.Tasks;
using TestCoverage.Tasks.Events;

namespace LiveCoverageVsPlugin
{
    /// <summary>
    /// Margin's canvas and visual definition including both size and content
    /// </summary>
    internal class LiveCoverageMargin : Canvas, IWpfTextViewMargin
    {
        /// <summary>
        /// Margin name.
        /// </summary>
        public const string MarginName = "LiveCoverageMargin";

        private readonly IWpfTextView _textView;
        private readonly IVsStatusbar _statusBar;
        private readonly Solution _solution;
        private readonly Canvas _canvas;
        private readonly ITaskCoverageManager _taskCoverageManager;

        private string _documentPath;
        private readonly VsSolutionTestCoverage _vsSolutionTestCoverage;
        private bool isDisposed = false;
        private int _currentNumberOfLines;
        private string _projectName;

        public LiveCoverageMargin(VsSolutionTestCoverage vsSolutionTestCoverage,
             ITaskCoverageManager taskCoverageManager,
             IWpfTextView textView,
             IVsStatusbar statusBar,
             Solution solution)
        {
            _vsSolutionTestCoverage = vsSolutionTestCoverage;
            _taskCoverageManager = taskCoverageManager;

            _canvas = new Canvas();
            _textView = textView;
            _statusBar = statusBar;
            _solution = solution;

            _textView.ViewportHeightChanged += (s, e) => Redraw();
            _textView.LayoutChanged += LayoutChanged;
            this.Width = 20;
            this.ClipToBounds = true;
            this.Background = new SolidColorBrush(Colors.White);
            Children.Add(_canvas);
            textView.TextBuffer.Changed += TextBuffer_Changed;

            _taskCoverageManager.CoverageTaskEvent += TaskCoverageManagerCoverageTaskEvent;
        }

        private void LayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (e.VerticalTranslation || _currentNumberOfLines != _textView.TextViewLines.Count)
            {
                Redraw();
                _currentNumberOfLines = _textView.TextViewLines.Count;
            }
        }

        private void TaskCoverageManagerCoverageTaskEvent(object sender, CoverageTaskArgsBase e)
        {
            if (e is MethodCoverageTaskStartedArgs)
            {
                var startedEvent = (MethodCoverageTaskStartedArgs)e;
                _statusBar.SetText(
                    $"Calculating coverage for the method {System.IO.Path.GetFileName(startedEvent.DocPath)}_{startedEvent.MethodName}");
            }
            else if (e is DocumentCoverageTaskStartedArgs)
            {
                var startedEvent = (DocumentCoverageTaskStartedArgs)e;
                _statusBar.SetText(
                    $"Calculating coverage for the document {System.IO.Path.GetFileName(startedEvent.DocPath)}");
            }
            else if (e is ResyncAllStarted)
            {
                _statusBar.SetText("Resyncing all...");
            }

            else if (e is MethodCoverageTaskCompletedArgs || e is DocumentCoverageTaskCompletedArgs || e is ResyncAllCompleted)
            {
                _statusBar.SetText("");
                Redraw();
            }
        }

        private void TextBuffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            bool foundMethod = _taskCoverageManager.EnqueueMethodTask(_projectName,
                    _textView.Caret.Position.BufferPosition.Position,
                    _textView.TextBuffer,
                    _documentPath);

            if (!foundMethod && e.Changes.Any(x => x.AnyCodeChanges()))
            {
                _taskCoverageManager.EnqueueDocumentTask(_projectName, _textView.TextBuffer, _documentPath);
            }
        }

        private bool InitProperties()
        {
            if (_documentPath != null)
                return true;
            var textDocument = GetTextDocument();

            if (textDocument == null)
                return false;

            var docPath = textDocument.FilePath;
            var projectItem = _solution.FindProjectItem(docPath);

            if (projectItem != null)
            {
                _projectName = projectItem.ContainingProject.Name;
                _documentPath = docPath;

                return true;
            }

            return false;
        }

        private ITextDocument GetTextDocument()
        {
            ITextDocument textDocument = null;

            if (_textView.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument))
                return textDocument;
            return null;
        }

        private void Redraw()
        {
            if (!InitProperties())
                return;

            _canvas.Children.Clear();

            var text = _textView.TextBuffer.CurrentSnapshot.GetText();

            List<LineCoverage> lineCoverage;
            if (!_vsSolutionTestCoverage.SolutionCoverageByDocument.ContainsKey(_documentPath))
                lineCoverage = new List<LineCoverage>();
            else
                lineCoverage = _vsSolutionTestCoverage.SolutionCoverageByDocument[_documentPath];

            var coverageDotDrawer = new CoverageDotDrawer(lineCoverage, text, System.IO.Path.GetFileNameWithoutExtension(_documentPath));

            int[] positions = _textView.TextViewLines.Select(x => x.Start.Position).ToArray();

            foreach (CoverageDot dotCoverage in coverageDotDrawer.Draw(positions, _taskCoverageManager.AreJobsPending, _projectName))
            {
                Ellipse ellipse = new Ellipse
                {
                    Fill = dotCoverage.Color,
                    ToolTip = dotCoverage.Tooltip,
                    Cursor = System.Windows.Input.Cursors.Arrow
                };
                ellipse.Width = ellipse.Height = 15;

                SetTop(ellipse, _textView.TextViewLines[dotCoverage.LineNumber].TextTop - _textView.ViewportTop);
                _canvas.Children.Add(ellipse);
            }
        }

        #region IWpfTextViewMargin

        /// <summary>
        /// Gets the <see cref="Sytem.Windows.FrameworkElement"/> that implements the visual representation of the margin.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The margin is disposed.</exception>
        public FrameworkElement VisualElement
        {
            // Since this margin implements Canvas, this is the object which renders
            // the margin.
            get
            {
                this.ThrowIfDisposed();
                return this;
            }
        }

        #endregion

        #region ITextViewMargin

        /// <summary>
        /// Gets the size of the margin.
        /// </summary>
        /// <remarks>
        /// For a horizontal margin this is the height of the margin,
        /// since the width will be determined by the <see cref="ITextView"/>.
        /// For a vertical margin this is the width of the margin,
        /// since the height will be determined by the <see cref="ITextView"/>.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The margin is disposed.</exception>
        public double MarginSize
        {
            get
            {
                this.ThrowIfDisposed();

                // Since this is a horizontal margin, its width will be bound to the width of the text view.
                // Therefore, its size is its height.
                return this.ActualHeight;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the margin is enabled.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The margin is disposed.</exception>
        public bool Enabled
        {
            get
            {
                this.ThrowIfDisposed();

                // The margin should always be enabled
                return true;
            }
        }

        /// <summary>
        /// Gets the <see cref="ITextViewMargin"/> with the given <paramref name="marginName"/> or null if no match is found
        /// </summary>
        /// <param name="marginName">The name of the <see cref="ITextViewMargin"/></param>
        /// <returns>The <see cref="ITextViewMargin"/> named <paramref name="marginName"/>, or null if no match is found.</returns>
        /// <remarks>
        /// A margin returns itself if it is passed its own name. If the name does not match and it is a container margin, it
        /// forwards the call to its children. Margin name comparisons are case-insensitive.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="marginName"/> is null.</exception>
        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return string.Equals(marginName, LiveCoverageMargin.MarginName, StringComparison.OrdinalIgnoreCase) ? this : null;
        }

        /// <summary>
        /// Disposes an instance of <see cref="LiveCoverageMargin"/> class.
        /// </summary>
        public void Dispose()
        {
            if (!this.isDisposed)
            {
                GC.SuppressFinalize(this);
                this.isDisposed = true;
            }
        }

        #endregion

        /// <summary>
        /// Checks and throws <see cref="ObjectDisposedException"/> if the object is disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(MarginName);
            }
        }
    }
}
