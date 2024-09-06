using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InMemoryDB
{

    /// <summary>
    /// Given the rows, the table converted to:
    /// 
    /// |-------|-------|
    /// | head1 | head2 |
    /// |===============|
    /// |  val1 | val2  |
    /// |-------|-------|
    /// 
    /// </summary>
    internal class Stringifier
    {

        private StringBuilder _sb = new();
        private int _cols;
        private int _colWidth;

        public Stringifier(int columnNum, int colWidth = 12)
        {
            _cols = columnNum;
            _colWidth = colWidth;

            MakeLine();
        }

        private void pushRowData(List<string> data)
        {
            if (data.Count > 0)
                _sb.Append('|');

            foreach (var one in data)
            {
                _sb.Append(' ');
                var f = one.ToString();
                if (f.Length > _colWidth - 2)
                {
                    _sb.Append(f.Substring(0, _colWidth - 2 - 3));  // two spaces + elipsis
                    _sb.Append("...");
                } else
                {
                    _sb.Append(f);
                    _sb.Append(new String(' ', _colWidth - 2 - f.Length));
                }
                _sb.Append(" |");
            }
            _sb.Append('\n');
        }

        private void pushDynamic(List<object> header)
        {
            List<string> strings = new List<string>();
            foreach (var item in header)
            {
                strings.Add(item.ToString());
            }
            pushRowData(strings);
        }

        public void PushHeader(List<string> header)
        {
            pushRowData(header);
            MakeDoubleLine();
        }

        public void PushRow(List<object> data)
        {
            pushDynamic(data);
            MakeLine();
        }

        public void PushFields(List<ParentField> fields)
        {
            List<string> strings = new List<string>();
            foreach (var field in fields)
            {
                strings.Add(field.ToString());
            }
            pushRowData(strings);
            MakeLine();
        }

        public void MakeLine()
        {
            var colLine = new String('-', _colWidth);
            if (_cols > 0)
                _sb.Append('|');

            for (int i = 0; i < _cols; i++)
            {
                _sb.Append(colLine);
                _sb.Append('|');
            }

            _sb.Append('\n');
        }

        public void MakeDoubleLine()
        {
            var colLine = new String('=', _colWidth);
            if (_cols > 0)
                _sb.Append('|');

            for (int i = 0; i < _cols; i++)
            {
                _sb.Append(colLine);
                _sb.Append('|');
            }

            _sb.Append('\n');
        }

        public override string ToString()
        {
            return _sb.ToString();
        }

    }
}
