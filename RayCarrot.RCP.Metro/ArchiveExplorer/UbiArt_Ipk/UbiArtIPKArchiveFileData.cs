﻿using ByteSizeLib;
using RayCarrot.IO;
using RayCarrot.Rayman;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Archived file data for a UbiArt .ipk file
    /// </summary>
    public class UbiArtIPKArchiveFileData : IArchiveFileData
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileData">The file data</param>
        /// <param name="settings">The settings when serializing the data</param>
        /// <param name="baseOffset">The base offset to use when reading the files</param>
        public UbiArtIPKArchiveFileData(UbiArtIPKFile fileData, UbiArtSettings settings, uint baseOffset)
        {
            // Get the file properties
            Directory = fileData.DirectoryPath;
            FileExtension = fileData.GetFileExtensions().Last();
            FileName = fileData.FileName;
            FileData = fileData;
            BaseOffset = baseOffset;
            Settings = settings;
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// The settings when serializing the data
        /// </summary>
        protected UbiArtSettings Settings { get; }

        /// <summary>
        /// The base offset to use when reading the files
        /// </summary>
        protected uint BaseOffset { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The directory the file is located under
        /// </summary>
        public string Directory { get; }

        /// <summary>
        /// The file data
        /// </summary>
        public UbiArtIPKFile FileData { get; }

        /// <summary>
        /// The file name
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// The info about the file to display
        /// </summary>
        public virtual string FileDisplayInfo => String.Format(
            Resources.Archive_IPK_FileInfo,
            Directory,
            FileData.IsCompressed,
            new ByteSize(FileData.Size),
            new ByteSize(FileData.CompressedSize),
            FileData.Offset + BaseOffset);

        /// <summary>
        /// The name of the file format
        /// </summary>
        public string FileFormatName => FileExtension.Substring(1).ToUpper();

        /// <summary>
        /// The supported file formats to import from
        /// </summary>
        public virtual string[] SupportedImportFileExtensions
        {
            get => new string[]
            {
                FileExtension
            };
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// The supported file formats to export to
        /// </summary>
        public virtual string[] SupportedExportFileExtensions
        {
            get => new string[]
            {
                FileExtension
            };
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// The file extension
        /// </summary>
        public string FileExtension { get; set; }

        /// <summary>
        /// The path to the temporary file containing the data to be imported
        /// </summary>
        public FileSystemPath PendingImportTempPath { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the contents of the file from the stream
        /// </summary>
        /// <param name="archiveFileStream">The file stream for the archive</param>
        /// <returns>The contents of the file</returns>
        public byte[] GetFileBytes(Stream archiveFileStream)
        {
            // Get the bytes
            var bytes = FileData.GetFileBytes(archiveFileStream, BaseOffset);

            // Initialize the data
            InitializeData(bytes);

            // Return the bytes
            return bytes;
        }

        /// <summary>
        /// Initializes the data for the file
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        public virtual void InitializeData(byte[] fileBytes) { }

        /// <summary>
        /// Exports the file to the specified path
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="filePath">The path to export the file to</param>
        /// <param name="fileFormat">The file extension to use</param>
        /// <returns>The task</returns>
        public virtual Task ExportFileAsync(byte[] fileBytes, FileSystemPath filePath, string fileFormat)
        {
            // Open the file
            using var file = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

            // Write to the stream
            file.Write(fileBytes, 0, fileBytes.Length);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Imports the file from the specified path to the <see cref="PendingImportTempPath"/> path
        /// </summary>
        /// <param name="fileBytes">The file bytes</param>
        /// <param name="filePath">The path of the file to import</param>
        /// <returns>A value indicating if the file was successfully imported</returns>
        public virtual Task<bool> ImportFileAsync(byte[] fileBytes, FileSystemPath filePath)
        {
            // Get the temporary file to save to, without disposing it
            var tempFile = new TempFile(false);

            // Copy the file
            RCFRCP.File.CopyFile(filePath, tempFile.TempPath, true);

            // Set the pending path
            PendingImportTempPath = tempFile.TempPath;

            return Task.FromResult(true);
        }

        #endregion
    }
}