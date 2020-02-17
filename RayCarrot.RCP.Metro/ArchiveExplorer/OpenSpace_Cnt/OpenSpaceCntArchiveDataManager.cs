﻿using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.Rayman;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Rayman.OpenSpace;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archive data manager for an OpenSpace .cnt file
    /// </summary>
    public class OpenSpaceCntArchiveDataManager : IArchiveDataManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="settings">The settings when serializing the data</param>
        public OpenSpaceCntArchiveDataManager(OpenSpaceSettings settings)
        {
            Settings = settings;
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The settings when serializing the data
        /// </summary>
        protected OpenSpaceSettings Settings { get; }

        /// <summary>
        /// Indicates if the files should be encrypted when imported. Defaulted to false due to encrypting the file being very slow.
        /// </summary>
        protected virtual bool EncryptFiles => false;

        #endregion

        #region Public Properties

        /// <summary>
        /// The path separator character to use. This is usually \ or /.
        /// </summary>
        public char PathSeparatorCharacter => '\\';

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the archive
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The archive data</returns>
        public ArchiveData LoadArchive(Stream archiveFileStream)
        {
            RCFCore.Logger?.LogInformationSource("The directories are being retrieved for a CNT archive");

            // Set the stream position to 0
            archiveFileStream.Position = 0;

            // Read the file data
            var data = OpenSpaceCntData.GetSerializer(Settings).Deserialize(archiveFileStream);

            RCFCore.Logger?.LogInformationSource($"Read CNT file ({data.VersionID}) with {data.Files.Length} files and {data.Directories.Length} directories");

            // Helper method for getting the directories
            IEnumerable<ArchiveDirectory> GetDirectories()
            {
                // Add the directories to the collection
                for (var i = -1; i < data.Directories.Length; i++)
                {
                    // Get the directory path
                    var dir = i == -1 ? String.Empty : data.Directories[i];

                    // Return each directory with the available files, including the root directory
                    yield return new ArchiveDirectory(dir, data.Files.Where(x => x.DirectoryIndex == i).Select(f => new OpenSpaceCntArchiveFileData(f, Settings, dir, EncryptFiles) as IArchiveFileData).ToArray());
                }
            }

            // Return the data
            return new ArchiveData(GetDirectories(), data.GetArchiveContent(archiveFileStream));
        }

        /// <summary>
        /// Updates the archive with the modified files
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <param name="outputFileStream">The file stream for the updated archive</param>
        /// <param name="files">The files of the archive. Modified files have the <see cref="IArchiveFileData.PendingImportTempPath"/> property set to an existing path.</param>
        /// <param name="generator">The file generator</param>
        public void UpdateArchive(Stream archiveFileStream, Stream outputFileStream, IEnumerable<IArchiveFileData> files, IDisposable generator)
        {
            RCFCore.Logger?.LogInformationSource($"A CNT archive is being repacked...");

            // Set the stream position to 0
            archiveFileStream.Position = 0;

            // Load the current file
            var data = OpenSpaceCntData.GetSerializer(Settings).Deserialize(archiveFileStream);

            // Order files by path
            var allFiles = files.Cast<OpenSpaceCntArchiveFileData>().ToDictionary(x => Path.Combine(x.Directory, x.FileName));

            // Create a temporary directory to load the current files into 
            using var tempDir = new TempDirectory(true);

            // Create the file generator
            using var fileGenerator = new ArchiveFileGenerator<OpenSpaceCntFileEntry>();

            // The current pointer position
            var pointer = data.GetHeaderSize(Settings);

            // Load each file data into temporary files as we're changing the archive structure by modifying the pointers
            foreach (var file in data.Files)
            {
                // Get the full path
                var fullPath = file.GetFullPath(data.Directories);

                // Get the file
                var existingFile = allFiles.TryGetValue(fullPath);

                // Check if the file is one of the modified files
                if (existingFile.PendingImportTempPath.FileExists)
                {
                    RCFCore.Logger?.LogTraceSource($"{existingFile.FileName} as been modified");

                    // Get the temporary file path without disposing it as it gets removed from the directory
                    var tempFilePath = (tempDir.TempPath + file.FileName).GetNonExistingFileName();

                    // Remove the file from the dictionary
                    allFiles.Remove(fullPath);

                    // Move the file
                    RCFRCP.File.MoveFile(existingFile.PendingImportTempPath, tempFilePath, true);

                    // Set the file size
                    file.Size = (int)tempFilePath.GetSize().Bytes;

                    // NOTE: Leaving this unknown value causes the game to crash if the texture is modified - why? Setting it to 0 always seems to work.
                    // Remove unknown value
                    file.Unknown1 = 0;

                    // Remove the encryption if set to do so
                    if (!EncryptFiles)
                    {
                        file.FileXORKey = new byte[]
                        {
                            0, 0, 0, 0
                        };

                        RCFCore.Logger?.LogTraceSource($"The encryption has been removed for {existingFile.FileName}");
                    }
                    // Otherwise encrypt the file
                    else
                    {
                        throw new NotImplementedException("Encrypting .gf files is currently not supported");
                    }

                    // Add to the generator
                    fileGenerator.Add(file, () => File.ReadAllBytes(tempFilePath));
                }
                // Use the original file without decrypting it
                else
                {
                    // Add to the generator
                    fileGenerator.Add(file, () => generator.CastTo<IArchiveFileGenerator<OpenSpaceCntFileEntry>>().GetBytes(existingFile.FileEntry)); 
                }

                // Set the pointer
                file.Pointer = pointer;

                // Increase by the file size
                pointer += file.Size;
            }

            // Serialize the data
            OpenSpaceCntData.GetSerializer(Settings).Serialize(outputFileStream, data);

            // Write the files
            data.WriteArchiveContent(outputFileStream, fileGenerator);

            RCFCore.Logger?.LogInformationSource($"The CNT archive has been repacked");
        }

        #endregion
    }
}