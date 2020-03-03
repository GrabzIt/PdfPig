﻿namespace UglyToad.PdfPig.Parser.Parts
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Core;
    using Util.JetBrains.Annotations;

    /// <summary>
    /// Brute force search for all objects in the document.
    /// </summary>
    internal static class BruteForceSearcher
    {
        private const int MinimumSearchOffset = 6;

        /// <summary>
        /// Find the offset of every object contained in the document by searching the entire document contents.
        /// </summary>
        /// <param name="bytes">The bytes of the document.</param>
        /// <returns>The object keys and offsets for the objects in this document.</returns>
        [NotNull]
        public static IReadOnlyDictionary<IndirectReference, long> GetObjectLocations(IInputBytes bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            var loopProtection = 0;

            var lastEndOfFile = GetLastEndOfFileMarker(bytes);

            var results = new Dictionary<IndirectReference, long>();

            var originPosition = bytes.CurrentOffset;

            var currentOffset = (long)MinimumSearchOffset;

            var currentlyInObject = false;

            do
            {
                if (loopProtection > 1_000_000)
                {
                    throw new PdfDocumentFormatException("Failed to brute-force search the file due to an infinite loop.");
                }

                loopProtection++;

                if (currentlyInObject)
                {
                    if (bytes.CurrentByte == 'e')
                    {
                        var next = bytes.Peek();

                        if (next.HasValue && next == 'n')
                        {
                            if (ReadHelper.IsString(bytes, "endobj"))
                            {
                                currentlyInObject = false;
                                loopProtection = 0;

                                for (int i = 0; i < "endobj".Length; i++)
                                {
                                    bytes.MoveNext();
                                    currentOffset++;
                                }
                            }
                            else
                            {
                                bytes.MoveNext();
                                currentOffset++;
                            }
                        }
                        else
                        {
                            bytes.MoveNext();
                            currentOffset++;
                        }
                    }
                    else
                    {
                        bytes.MoveNext();
                        currentOffset++;
                        loopProtection = 0;
                    }

                    continue;
                }

                bytes.Seek(currentOffset);

                if (!ReadHelper.IsString(bytes, " obj"))
                {
                    currentOffset++;
                    continue;
                }

                // Current byte is ' '[obj]
                var offset = currentOffset - 1;

                bytes.Seek(offset);

                var generationBytes = new StringBuilder();
                while (ReadHelper.IsDigit(bytes.CurrentByte) && offset >= MinimumSearchOffset)
                {
                    generationBytes.Insert(0, (char)bytes.CurrentByte);
                    offset--;
                    bytes.Seek(offset);
                }

                // We should now be at the space between object and generation number.
                if (!ReadHelper.IsSpace(bytes.CurrentByte))
                {
                    continue;
                }

                bytes.Seek(--offset);

                var objectNumberBytes = new StringBuilder();
                while (ReadHelper.IsDigit(bytes.CurrentByte) && offset >= MinimumSearchOffset)
                {
                    objectNumberBytes.Insert(0, (char)bytes.CurrentByte);
                    offset--;
                    bytes.Seek(offset);
                }

                var obj = long.Parse(objectNumberBytes.ToString(), CultureInfo.InvariantCulture);
                var generation = int.Parse(generationBytes.ToString(), CultureInfo.InvariantCulture);

                results[new IndirectReference(obj, generation)] = bytes.CurrentOffset;

                currentlyInObject = true;

                currentOffset++;

                bytes.Seek(currentOffset);
                loopProtection = 0;
            } while (currentOffset < lastEndOfFile && !bytes.IsAtEnd());
            
            // reestablish origin position
            bytes.Seek(originPosition);
            
            return results;
        }

        private static long GetLastEndOfFileMarker(IInputBytes bytes)
        {
            var originalOffset = bytes.CurrentOffset;

            const string searchTerm = "%%EOF";

            var minimumEndOffset = bytes.Length - searchTerm.Length;

            bytes.Seek(minimumEndOffset);

            while (bytes.CurrentOffset > 0)
            {
                if (ReadHelper.IsString(bytes, searchTerm))
                {
                    var position = bytes.CurrentOffset;

                    bytes.Seek(originalOffset);

                    return position;
                }

                bytes.Seek(minimumEndOffset--);
            }

            bytes.Seek(originalOffset);
            return long.MaxValue;
        }
    }
}
