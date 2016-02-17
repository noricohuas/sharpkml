using System;
using SharpKml.Engine;

namespace SharpKml.Base
{
    public interface IKmzReader
    {
        /// <summary>Extracts the default Kml file from the archive.</summary>
        /// <returns>
        /// A string containing the Kml content if a suitable file was found in
        /// the Kmz archive; otherwise, null.
        /// </returns>
        /// <remarks>
        /// This returns the first file in the Kmz archive table of contents
        /// which has a ".kml" extension. Note that the file found may not
        /// necessarily be in the root directory.
        /// </remarks>
        /// <exception cref="System.ObjectDisposedException">
        /// <see cref="SharpKml.Engine.KmzFile.Dispose()"/> has been called on this instance.
        /// </exception>
        string ReadKml();
    }
}