using System.Diagnostics;

namespace QDP
{
    /// <summary>
    /// Represents a position and range in the source code e.g. of a token or a parsed rule
    /// </summary>
    public class Position
    {
        public int Index { get; private set; }
        public int Row { get; private set; }
        public int Column { get; private set; }
        public int Length { get; private set; }

        internal Position(int index, int row, int column, int length)
        {
            Debug.Assert(index >= 0, "Negative Index.");
            Debug.Assert(row >= 0, "Negative Row.");
            Debug.Assert(column >= 0, "Negative Column.");
            Debug.Assert(length >= 0, "Negative length.");

            Index = index;
            Row = row;
            Column = column;
            Length = length;
        }

        public override string ToString()
        {
            return $"(Col {Column}, Row {Row} (Index {Index}, Length {Length}))";
        }
    }
}
