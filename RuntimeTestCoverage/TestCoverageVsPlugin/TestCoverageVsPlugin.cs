using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using EnvDTE;

namespace TestCoverageVsPlugin
{
    /// <summary>
    /// A class detailing the margin's visual definition including both size and content.
    /// </summary>
    class TestCoverageVsPlugin : Canvas, IWpfTextViewMargin
    {
        public const string MarginName = "TestCoverageVsPlugin";

        private readonly string _documentPath;
        private SyntaxTree _syntaxTree;
        private readonly SolutionTestCoverage _solutionTestCoverage;
        private readonly IWpfTextView _textView;
        private bool _isDisposed = false;
        private readonly Canvas _canvas;
        private readonly DispatcherTimer _timer;

        /// <summary>
        /// Creates a <see cref="TestCoverageVsPlugin"/> for a given <see cref="IWpfTextView"/>.
        /// </summary>
        /// <param name="solutionTestCoverage"></param>
        /// <param name="textView">The <see cref="IWpfTextView"/> to attach the margin to.</param>
        public TestCoverageVsPlugin(SolutionTestCoverage solutionTestCoverage, IWpfTextView textView)
        {
            _canvas = new Canvas();
            _solutionTestCoverage = solutionTestCoverage;
            _textView = textView;
            _textView.ViewportHeightChanged += TextViewViewportHeightChanged;
            _textView.LayoutChanged += TextViewLayoutChanged;
            this.Width = 20;
            this.ClipToBounds = true;
            this.Background = new SolidColorBrush(Colors.White);
            Children.Add(_canvas);
            _timer=new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(2);
            _timer.Tick += RecalculateTimerElapsed;
            textView.TextBuffer.Changing += TextBuffer_Changing;
            _documentPath = GetTextDocument().FilePath;
            _syntaxTree = CSharpSyntaxTree.ParseText(_textView.TextBuffer.CurrentSnapshot.GetText());
        }

        private void RecalculateTimerElapsed(object sender, EventArgs eventArgs)
        {
            _timer.Stop();
            _syntaxTree = CSharpSyntaxTree.ParseText(_textView.TextBuffer.CurrentSnapshot.GetText());

            string documentContent = _textView.TextBuffer.CurrentSnapshot.GetText();
            int carretPos = _textView.Caret.Position.BufferPosition;
            _solutionTestCoverage.CalculateForSelectedItem(GetTextDocument().FilePath, documentContent, carretPos);

            Redraw();
        }

        private void TextBuffer_Changing(object sender, TextContentChangingEventArgs e)
        {
            _timer.Stop();
            _timer.Start();
        }
        private ITextDocument GetTextDocument()
        {
            return _textView.TextBuffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument));
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
            int currentMethodIndex = 0;
            int currentSpan = 0;
            MethodDeclarationSyntax[] allMethods = _syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().ToArray();
            if (allMethods.Length == 0)
                return;

            int[] coveragePositions = new int[0];

            if (_solutionTestCoverage.SolutionCoverage.ContainsKey(_documentPath))
                coveragePositions = _solutionTestCoverage.SolutionCoverage[_documentPath].Select(x => x.Span).ToArray();

            _canvas.Children.Clear();

            var text = _textView.TextBuffer.CurrentSnapshot.GetText();

            for (int i = 0; i < _textView.TextViewLines.Count; i++)
            {
                int lineNumber = GetLineNumber(i);

                if (lineNumber > _textView.TextBuffer.CurrentSnapshot.LineCount)
                    break;

                string currentLineText = _textView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber - 1).GetText();
                currentSpan = text.IndexOf(currentLineText.TrimStart(), currentSpan + 1);

                if (currentSpan >= allMethods[currentMethodIndex].Span.End)
                {
                    currentMethodIndex++;

                    if (currentMethodIndex >= allMethods.Length)
                        break;
                }

                if (currentSpan < allMethods[currentMethodIndex].Span.Start)
                    continue;

                AddDotCoverage(coveragePositions, currentSpan, allMethods[currentMethodIndex], _textView.TextViewLines[i]);
            }
        }

        private void AddDotCoverage(int[] coveragePositions, int currentSpan, MethodDeclarationSyntax method, IWpfTextViewLine wpfTextViewLine)
        {
            Ellipse ellipse = new Ellipse();

            if (coveragePositions.Contains(currentSpan - method.Span.Start))
                ellipse.Fill = Brushes.Green;
            else
                ellipse.Fill = Brushes.Red;

            ellipse.Width = ellipse.Height = 15;

            SetTop(ellipse, wpfTextViewLine.TextTop - _textView.ViewportTop);

            _canvas.Children.Add(ellipse);
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
