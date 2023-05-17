﻿namespace UglyToad.PdfPig
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// This class will be used to signify a range. a(min) &lt;= a* &lt;= a(max)
    /// </summary>
    public struct PdfRange
    {
        private readonly IReadOnlyList<double> rangeArray;
        private readonly int startingIndex;

        /// <summary>
        /// Constructor with an initial range of 0..1.
        /// </summary>
        public PdfRange()
        {
            rangeArray = new double[] { 0.0, 1.0 };
            startingIndex = 0;
        }

        /// <summary>
        /// Constructor assumes a starting index of 0.
        /// </summary>
        /// <param name="range">The array that describes the range.</param>
        public PdfRange(IEnumerable<decimal> range)
            : this(range.Select(v => (double)v), 0)
        {
        }

        /// <summary>
        /// Constructor with an index into an array. Because some arrays specify
        /// multiple ranges ie [0, 1, 0, 2, 2, 3]. It is convenient for this
        /// class to take an index into an array. So if you want this range to
        /// represent 0, 2 in the above example then you would say <c>new PDRange(array, 1)</c>.
        /// </summary>
        /// <param name="range">The array that describes the index</param>
        /// <param name="index">The range index into the array for the start of the range.</param>
        public PdfRange(IEnumerable<decimal> range, int index)
            : this(range.Select(v => (double)v), index)
        {
        }

        /// <summary>
        /// Constructor assumes a starting index of 0.
        /// </summary>
        /// <param name="range">The array that describes the range.</param>
        public PdfRange(IEnumerable<double> range)
            : this(range, 0)
        {
        }

        /// <summary>
        /// Constructor with an index into an array. Because some arrays specify
        /// multiple ranges ie [0, 1, 0, 2, 2, 3]. It is convenient for this
        /// class to take an index into an array. So if you want this range to
        /// represent 0, 2 in the above example then you would say <c>new PDRange(array, 1)</c>.
        /// </summary>
        /// <param name="range">The array that describes the index</param>
        /// <param name="index">The range index into the array for the start of the range.</param>
        public PdfRange(IEnumerable<double> range, int index)
        {
            rangeArray = range.Select(v => (double)v).ToArray();
            startingIndex = index;
        }

        /// <summary>
        /// The minimum value of the range.
        /// </summary>
        public double Min
        {
            get
            {
                return rangeArray[startingIndex * 2];
            }
        }

        /// <summary>
        /// The maximum value of the range.
        /// </summary>
        public double Max
        {
            get
            {
                return rangeArray[startingIndex * 2 + 1];
            }
        }
    }
}
