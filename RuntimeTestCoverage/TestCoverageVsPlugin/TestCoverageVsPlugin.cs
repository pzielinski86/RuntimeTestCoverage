using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        private readonly DocumentTestCoverage _documentTestCoverage;
        private IWpfTextView _textView;
        private bool _isDisposed = false;
        private Canvas _canvas;
        private Timer _timer;
        private string[] _paths = new string[0];

        /// <summary>
        /// Creates a <see cref="TestCoverageVsPlugin"/> for a given <see cref="IWpfTextView"/>.
        /// </summary>
        /// <param name="documentTestCoverage"></param>
        /// <param name="textView">The <see cref="IWpfTextView"/> to attach the margin to.</param>
        public TestCoverageVsPlugin(DocumentTestCoverage documentTestCoverage, IWpfTextView textView)
        {
            _canvas = new Canvas();
            _documentTestCoverage = documentTestCoverage;
            _textView = textView;
            _textView.ViewportHeightChanged += _textView_ViewportHeightChanged;
            _textView.LayoutChanged += _textView_LayoutChanged;
            this.Width = 20;
            this.ClipToBounds = true;
            this.Background = new SolidColorBrush(Colors.LightGreen);
            Children.Add(_canvas);
            _timer = new Timer(3000);
            _timer.Elapsed += _timer_Elapsed;
            textView.TextBuffer.Changing += TextBuffer_Changing;
            ExtractTypePaths();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();            
            string documentContent = _textView.TextBuffer.CurrentSnapshot.GetText();
            int carretPos = _textView.Caret.Position.BufferPosition;
            _documentTestCoverage.CalculateForDocument(GetDocumentName(), documentContent, carretPos);
            ExtractTypePaths();
            Redraw();
        }

        private void TextBuffer_Changing(object sender, TextContentChangingEventArgs e)
        {
            _timer.Stop();
            _timer.Start();
        }

        private void ExtractTypePaths()
        {
            SyntaxTree node=CSharpSyntaxTree.ParseText(_textView.TextBuffer.CurrentSnapshot.GetText());
            SyntaxNode root = node.GetRoot();

            List<string> paths=new List<string>();

            foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {

                SyntaxNode namespaceNode = method.Parent;
                ClassDeclarationSyntax classNode = null;                

                while (namespaceNode!=null&&!(namespaceNode is NamespaceDeclarationSyntax))
                {
                   
                    if (namespaceNode is ClassDeclarationSyntax)
                        classNode = (ClassDeclarationSyntax)namespaceNode;

                    namespaceNode = namespaceNode.Parent;
                }

                string @namespace = null;


                if (namespaceNode != null)
                    @namespace = ((NamespaceDeclarationSyntax) namespaceNode).Name.ToString();

                paths.Add(string.Format("{0}.{1}.{2}.", @namespace, classNode.Identifier.Text, method.Identifier.Text));

            }

            _paths = paths.ToArray();

        }
        private string GetDocumentName()
        {
            ITextDocument textDocument = GetTextDocument(_textView.TextBuffer);

            return System.IO.Path.GetFileName(textDocument.FilePath);
        }
        private ITextDocument GetTextDocument(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetProperty<ITextDocument>(typeof(ITextDocument));
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
            var positions = _documentTestCoverage.GetPositions(_paths).ToArray();

            if (positions.Length == 0)
                return;


            _canvas.Children.Clear();

            int currentSpan = 0;

            string text = _textView.TextBuffer.CurrentSnapshot.GetText();

            for (int i = 0; i < _textView.TextViewLines.Count; i++)
            {
                int lineNumber = GetLineNumber(i);

                if (lineNumber > _textView.TextBuffer.CurrentSnapshot.LineCount)
                    break;

                string currentLineText =
                    _textView.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber - 1).GetText();

                int spanStart = text.IndexOf(currentLineText.Trim(), currentSpan);
                currentSpan = spanStart + 1;

                Ellipse ellipse = new Ellipse();

                if (positions.Contains(spanStart))
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
