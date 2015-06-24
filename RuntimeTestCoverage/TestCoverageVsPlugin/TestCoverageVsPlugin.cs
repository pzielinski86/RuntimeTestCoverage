﻿using System;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using TestCoverage;

namespace TestCoverageVsPlugin
{
    /// <summary>
    /// A class detailing the margin's visual definition including both size and content.
    /// </summary>
    class TestCoverageVsPlugin : Canvas, IWpfTextViewMargin
    {
        public const string MarginName = "TestCoverageVsPlugin";
        private IWpfTextView _textView;
        private bool _isDisposed = false;
        private Canvas _canvas;
        private static int[] _coveredPaths;
        /// <summary>
        /// Creates a <see cref="TestCoverageVsPlugin"/> for a given <see cref="IWpfTextView"/>.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> to attach the margin to.</param>
        public TestCoverageVsPlugin(IWpfTextView textView)
        {
            _canvas = new Canvas();
            _textView = textView;
            _textView.ViewportHeightChanged += _textView_ViewportHeightChanged;
            _textView.LayoutChanged += _textView_LayoutChanged;
            this.Width = 20;
            this.ClipToBounds = true;
            this.Background = new SolidColorBrush(Colors.LightGreen);
            Children.Add(_canvas);

            if (_coveredPaths != null)
                return;

            const string solutionPath = @"C:\projects\RuntimeTestCoverage\TestSolution\TestSolution.sln";

            var rewritter = new SolutionRewritter();
            RewriteResult rewriteResult = rewritter.RewriteAllClasses(solutionPath);

            var lineCoverageCalc = new LineCoverageCalc();
            _coveredPaths=lineCoverageCalc.CalculateForAllTests(solutionPath, rewriteResult);

        }

        private void _textView_LayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            Redraw();
        }

        private void _textView_ViewportHeightChanged(object sender, EventArgs e)
        {
            Redraw();
        }

        private void Redraw()
        {
            _canvas.Children.Clear();

            string text=_textView.TextBuffer.CurrentSnapshot.GetText();

            for (int i = 0; i < _textView.TextViewLines.Count; i++)
            {
                int lineNumber = GetLineNumber(i);

                if (lineNumber > _textView.TextBuffer.CurrentSnapshot.LineCount)
                    break;

                string currentLineText =
                    _textView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber-1).GetText();

                int spanStart = text.IndexOf(currentLineText.Trim());

                Ellipse ellipse = new Ellipse();

                if (_coveredPaths.Contains(spanStart))
                    ellipse.Fill = Brushes.Green;
                else
                    ellipse.Fill = Brushes.Red;

                ellipse.Width = ellipse.Height = 15;

                SetTop(ellipse, _textView.TextViewLines[i].TextTop - _textView.ViewportTop);

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
        /// The <see cref="Sytem.Windows.FrameworkElement"/> that implements the visual representation
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
