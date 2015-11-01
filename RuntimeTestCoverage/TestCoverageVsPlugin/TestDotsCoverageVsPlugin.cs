using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Solution = EnvDTE.Solution;

namespace TestCoverageVsPlugin
{
    /// <summary>
    /// A class detailing the margin's visual definition including both size and content.
    /// </summary>
    class TestDotsCoverageVsPlugin : Canvas, IWpfTextViewMargin
    {
        public const string MarginName = "TestDotsCoverageVsPlugin";

        private readonly IWpfTextView _textView;
        private readonly IVsStatusbar _statusBar;
        private readonly Solution _solution;
        private readonly Canvas _canvas;
        private readonly ITaskCoverageManager _taskCoverageManager;

        private Task _currentTask;
        private string _documentPath;
        private readonly VsSolutionTestCoverage _vsSolutionTestCoverage;
        private bool _isDisposed = false;
        private string _projectName;

        /// <summary>
        /// Creates a <see cref="TestDotsCoverageVsPlugin"/> for a given <see cref="IWpfTextView"/>.
        /// </summary>
        /// <param name="vsSolutionTestCoverage"></param>
        /// <param name="textView">The <see cref="IWpfTextView"/> to attach the margin to.</param>
        /// <param name="statusBar"></param>
        public TestDotsCoverageVsPlugin(VsSolutionTestCoverage vsSolutionTestCoverage, IWpfTextView textView, IVsStatusbar statusBar, Solution solution)
        {
            _canvas = new Canvas();
            _textView = textView;
            _statusBar = statusBar;
            _solution = solution;
            _textView.ViewportHeightChanged += TextViewViewportHeightChanged;
            _textView.LayoutChanged += TextViewLayoutChanged;
            this.Width = 20;
            this.ClipToBounds = true;
            this.Background = new SolidColorBrush(Colors.White);
            Children.Add(_canvas);
            textView.TextBuffer.Changed += TextBuffer_Changed;

            _vsSolutionTestCoverage = vsSolutionTestCoverage;
            _vsSolutionTestCoverage.Init();
            _taskCoverageManager = new TaskCoverageManager(new VsDispatchTimer(), _vsSolutionTestCoverage);
            _taskCoverageManager.DocumentCoverageTaskCompleted += DocumentCoverageTaskCompleted;
            _taskCoverageManager.DocumentCoverageTaskStarted += DocumentCoverageTaskStarted;

            InitProperties();
        }

        private void DocumentCoverageTaskStarted(object sender, DocumentCoverageTaskCompletedArgs e)
        {
            _statusBar.SetText($"Calculating coverage for {System.IO.Path.GetFileName(e.DocPath)}");
            CSharpSyntaxTree.ParseText(_textView.TextBuffer.CurrentSnapshot.GetText());
        }

        private void DocumentCoverageTaskCompleted(object sender, DocumentCoverageTaskCompletedArgs e)
        {
            _statusBar.SetText("");
            Redraw();
        }

        private void TextBuffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            string documentContent = e.After.GetText();
            _taskCoverageManager.EnqueueDocumentTask(_projectName, _documentPath, documentContent);

        }

        private void InitProperties()
        {
            _documentPath = GetTextDocument().FilePath;
            var projectItem = _solution.FindProjectItem(_documentPath);
            _projectName = projectItem.ContainingProject.Name;
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

            List<LineCoverage> lineCoverage;
            if (!_vsSolutionTestCoverage.SolutionCoverageByDocument.ContainsKey(_documentPath))
                lineCoverage = new List<LineCoverage>();
            else
                lineCoverage = _vsSolutionTestCoverage.SolutionCoverageByDocument[_documentPath];

            var coverageDotDrawer = new CoverageDotDrawer(lineCoverage, text);

            int[] positions = _textView.TextViewLines.Select(x => x.Start.Position).ToArray();

            foreach (CoverageDot dotCoverage in coverageDotDrawer.Draw(positions, _taskCoverageManager.AreJobsPending))
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Fill = dotCoverage.Color;
                ellipse.Width = ellipse.Height = 15;

                SetTop(ellipse, _textView.TextViewLines[dotCoverage.LineNumber].TextTop - _textView.ViewportTop);
                _canvas.Children.Add(ellipse);
            }
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
        /// <returns>An instance of TestDotsCoverageVsPlugin or null</returns>
        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return (marginName == TestDotsCoverageVsPlugin.MarginName) ? (IWpfTextViewMargin)this : null;
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
