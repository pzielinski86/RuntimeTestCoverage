using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Logs
{
    public class LogsLineContainer
    {
        private readonly int _maxLines;
        private readonly LinkedList<string> _lines;

        public LogsLineContainer(int maxLines = 100)
        {
            _maxLines = maxLines;
            _lines = new LinkedList<string>();
        }

        public void Add(string line)
        {
            if (_lines.Count == _maxLines)
                _lines.RemoveFirst();

            _lines.AddLast(line);
        }

        public IEnumerable<string> Lines
        {
            get { return _lines; }
        }

        public string Text
        {
            get { return string.Join(Environment.NewLine, _lines.ToArray()); }
        }
    }
}
