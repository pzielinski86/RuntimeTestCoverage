using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using TestCoverage.CoverageCalculation;

namespace TestCoverageVsPlugin
{
    /// <summary>
    /// A class detailing the margin's visual definition including both size and content.
    /// </summary>
    class TestCoverageVsPlugin : Canvas, IWpfTextViewMargin
    {
        public const string MarginName = "TestCoverageVsPlugin";

        private readonly IWpfTextView _textView;
        private readonly IVsStatusbar _statusBar;
        private readonly Canvas _canvas;
        private readonly DispatcherTimer _timer;

        private Task _currentTask;
        private SyntaxTree _syntaxTree;
        private readonly string _documentPath;
        private readonly VsSolutionTestCoverage _vsSolutionTestCoverage;
        private bool _isDisposed = false;
        private bool _taskQueued;

        /// <summary>
        /// Creates a <see cref="TestCoverageVsPlugin"/> for a given <see cref="IWpfTextView"/>.
        /// </summary>
        /// <param name="vsSolutionTestCoverage"></param>
        /// <param name="textView">The <see cref="IWpfTextView"/> to attach the margin to.</param>
        /// <param name="statusBar"></param>
        public TestCoverageVsPlugin(VsSolutionTestCoverage vsSolutionTestCoverage, IWpfTextView textView, IVsStatusbar statusBar)
        {
            _canvas = new Canvas();
            _textView = textView;
            _statusBar = statusBar;
            _textView.ViewportHeightChanged += TextViewViewportHeightChanged;
            _textView.LayoutChanged += TextViewLayoutChanged;
            this.Width = 20;
            this.ClipToBounds = true;
            this.Background = new SolidColorBrush(Colors.White);
            Children.Add(_canvas);
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += RecalculateTimerElapsed;
            textView.TextBuffer.Changing += TextBuffer_Changing;

            _documentPath = GetTextDocument().FilePath;
            _vsSolutionTestCoverage = vsSolutionTestCoverage;
            _syntaxTree = CSharpSyntaxTree.ParseText(_textView.TextBuffer.CurrentSnapshot.GetText());
        }

        private void RecalculateTimerElapsed(object sender, EventArgs eventArgs)
        {
            if (_currentTask != null)
                return;

            _timer.Stop();

            ITextDocument textDocument = GetTextDocument();
            if (textDocument == null)
                return;

            string documentPath = textDocument.FilePath;
            string documentContent = _textView.TextBuffer.CurrentSnapshot.GetText();

            _statusBar.SetText($"Calculating coverage for {System.IO.Path.GetFileName(documentPath)}");
            _syntaxTree = CSharpSyntaxTree.ParseText(_textView.TextBuffer.CurrentSnapshot.GetText());

            _currentTask = _vsSolutionTestCoverage.CalculateForDocumentAsync(documentPath, documentContent);
            _currentTask.ContinueWith(CalculationsCompleted, null, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void CalculationsCompleted(Task task, object o)
        {
            _taskQueued = false;
            _statusBar.SetText("");
            _currentTask = null;
            Redraw();
        }

        private void TextBuffer_Changing(object sender, TextContentChangingEventArgs e)
        {
            _taskQueued = true;
            _timer.Stop();
            _timer.Start();
        }

        private ITextDocument GetTextDocument()
        {
            ITextDocument textDocument = null;

            if (_textView.TextBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument))
                return textDocument;
            return null;
        }

        private void TextViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            Redraw();
        }

        private void TextViewViewportHeightChanged(object sender, EventArgs e)
        {
            Redraw();
        }

        private void Redraw()
        {
            _canvas.Children.Clear();

            var text = _textView.TextBuffer.CurrentSnapshot.GetText();
            List<LineCoverage> lineCoverage = _vsSolutionTestCoverage.SolutionCoverageByDocument[_documentPath];
            var coverageDotDrawer = new CoverageDotDrawer(lineCoverage, text);

            int[] positions = _textView.TextViewLines.Select(x => x.Start.Position).ToArray();

            foreach (CoverageDot dotCoverage in coverageDotDrawer.Draw(positions, _taskQueued))
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Fill = dotCoverage.Color;
                ellipse.Width = ellipse.Height = 15;

                SetTop(ellipse, _textView.TextViewLines[dotCoverage.LineNumber].TextTop - _textView.ViewportTop);
                _canvas.Children.Add(ellipse);
            }
        }

        private int GetLineNumber(int index)
        {
            int position = _textView.TextViewLines[index].Start.Position;
            return _textView.TextViewLines[index].Start.Snapshot.GetLineNumberFromPosition(position) + 1;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(MarginName);
        }

        #region IWpfTextViewMargin Members

        /// <summary>
        /// The <see FrameworkElementlement"/> that implements the visual representation
        /// of the margin.
        /// </summary>
        public System.Windows.FrameworkElement VisualElement
        {
            // Since this margin implements Canvas, this is the object which renders
            // the margin.
            get
            {
                ThrowIfDisposed();
                return this;
            }
        }

        #endregion

        #region ITextViewMargin Members

        public double MarginSize
        {
            // Since this is a horizontal margin, its width will be bound to the width of the text view.
            // Therefore, its size is its height.
            get
            {
                ThrowIfDisposed();
                return this.ActualHeight;
            }
        }

        public bool Enabled
        {
            // The margin should always be enabled
            get
            {
                ThrowIfDisposed();
                return true;
            }
        }

        /// <summary>
        /// Returns an instance of the margin if this is the margin that has been requested.
        /// </summary>
        /// <param name="marginName">The name of the margin requested</param>
        /// <returns>An instance of TestCoverageVsPlugin or null</returns>
        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return (marginName == TestCoverageVsPlugin.MarginName) ? (IWpfTextViewMargin)this : null;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }
        #endregion
    }
}
