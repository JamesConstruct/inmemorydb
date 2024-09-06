using System.Text;

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

        /// <summary>
        /// Creates a new stringifier instance with the number of columns set to columnNum and the width of each column fiexed to colWidth.
        /// </summary>
        /// <param name="columnNum">Number of columns of the table.</param>
        /// <param name="colWidth">Width of a single column in characters.</param>
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
                }
                else
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
                strings.Add(item.ToString()!);
            }
            pushRowData(strings);
        }

        /// <summary>
        /// Writes a table header to the internal cache.
        /// </summary>
        /// <param name="header"></param>
        public void PushHeader(List<string> header)
        {
            pushRowData(header);
            MakeDoubleLine();
        }

        /// <summary>
        /// Writes a table row to the internal cache.
        /// </summary>
        /// <param name="data"></param>
        public void PushRow(List<object> data)
        {
            pushDynamic(data);
            MakeLine();
        }

        /// <summary>
        /// Writes a table row to the internal cache. The fields are individualy converted to strings.
        /// </summary>
        /// <param name="fields"></param>
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

        /// <summary>
        /// Makes a horizontal line.
        /// </summary>
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

        /// <summary>
        /// Makes a double horizontal line.
        /// </summary>
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

        /// <summary>
        /// Converts the current cache to string and returns it.
        /// </summary>
        /// <returns>Current string version of the written table features.</returns>
        public override string ToString()
        {
            return _sb.ToString();
        }

    }
}
