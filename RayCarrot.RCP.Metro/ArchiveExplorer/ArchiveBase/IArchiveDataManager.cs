﻿using System;
using System.Collections.Generic;
using System.IO;
using RayCarrot.IO;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Defines an archive data manager
    /// </summary>
    public interface IArchiveDataManager
    {
        /// <summary>
        /// The path separator character to use. This is usually \ or /.
        /// </summary>
        char PathSeparatorCharacter { get; }

        /// <summary>
        /// Loads the archive data
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The archive data</returns>
        ArchiveData LoadArchiveData(Stream archiveFileStream);

        /// <summary>
        /// Loads the archive
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The archive data</returns>
        object LoadArchive(Stream archiveFileStream);

        /// <summary>
        /// Gets a new archive from a collection of modified files
        /// </summary>
        /// <param name="files"></param>
        /// <returns>The archive</returns>
        object GetArchive(IEnumerable<IArchiveImportData> files);

        /// <summary>
        /// Gets a new file entry from the specified path
        /// </summary>
        /// <param name="relativePath">The relative path of the file</param>
        /// <returns>The file entry</returns>
        object GetFileEntry(FileSystemPath relativePath);

        /// <summary>
        /// Updates the archive with the modified files
        /// </summary>
        /// <param name="archive">The loaded archive data</param>
        /// <param name="outputFileStream">The file stream for the updated archive</param>
        /// <param name="files">The import data for the archive files</param>
        /// <param name="generator">The file generator</param>
        void UpdateArchive(object archive, Stream outputFileStream, IEnumerable<IArchiveImportData> files, IDisposable generator);
    }
}