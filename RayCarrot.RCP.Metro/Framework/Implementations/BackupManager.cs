﻿using RayCarrot.CarrotFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace RayCarrot.RCP.Metro
{   
    /// <summary>
    /// The backup manager
    /// </summary>
    public class BackupManager
    {
        #region Static Constructor

        static BackupManager()
        {
            AsyncLock = new AsyncLock();
        }

        #endregion

        #region Private Static Properties

        /// <summary>
        /// The async lock for backup and restore operations
        /// </summary>
        private static AsyncLock AsyncLock { get; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Performs a backup on the game
        /// </summary>
        /// <param name="game">The game to perform the backup on</param>
        /// <param name="backupInfo">The backup info</param>
        /// <returns>True if the backup was successful</returns>
        private static async Task<bool> PerformBackupAsync(Games game, IEnumerable<BackupDir> backupInfo)
        {
            // Get the destination directory
            FileSystemPath destinationDir = game.GetBackupDir();

            // Get the temp path
            var tempPath = CommonPaths.TempPath + game.GetBackupName();

            // Delete temp backup
            RCFRCP.File.DeleteDirectory(tempPath);

            // Create temp path
            Directory.CreateDirectory(CommonPaths.TempPath);

            // Check if a backup already exists
            if (destinationDir.DirectoryExists)
                // Create a new temp backup
                Directory.Move(destinationDir, tempPath);

            try
            {
                // Enumerate the backup information
                foreach (var item in backupInfo)
                {
                    // Check if the entire directory should be copied
                    if (item.IsEntireDir())
                    {
                        // Copy the directory   
                        RCFRCP.File.CopyDirectory(item.DirPath, destinationDir + item.ID, true, true);
                    }
                    else
                    {
                        // Get the files
                        var files = Directory.GetFiles(item.DirPath, item.ExtensionFilter ?? "*", item.SearchOption);

                        // Backup each file
                        foreach (FileSystemPath file in files)
                        {
                            // Get the destination file
                            var destFile = destinationDir + item.ID + file.GetRelativePath(item.DirPath);

                            // Check if the directory does not exist
                            if (!destFile.Parent.DirectoryExists)
                                // Create the directory
                                Directory.CreateDirectory(destFile.Parent);

                            // Copy the file
                            File.Copy(file, destFile);
                        }
                    }
                }

                // Check if any files were backed up
                if (!destinationDir.DirectoryExists)
                {
                    await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_MissingFilesError, game.GetDisplayName()), Resources.Backup_FailedHeader, MessageType.Error);

                    // Check if a temp backup exists
                    if (tempPath.DirectoryExists)
                        // Restore temp backup
                        RCFRCP.File.MoveDirectory(tempPath, destinationDir, true);

                    return false;
                }

                // Delete temp backup
                RCFRCP.File.DeleteDirectory(tempPath);

                RCF.Logger.LogInformationSource($"Backup complete");

                return true;
            }
            catch
            {
                // Check if a temp backup exists
                if (tempPath.DirectoryExists)
                    // Restore temp backup
                    RCFRCP.File.MoveDirectory(tempPath, destinationDir, true);

                RCF.Logger.LogInformationSource($"Backup failed - clean up succeeded");

                throw;
            }
        }

        /// <summary>
        /// Performs a compressed backup on the game
        /// </summary>
        /// <param name="game">The game to perform the backup on</param>
        /// <param name="backupInfo">The backup info</param>
        /// <returns>True if the backup was successful</returns>
        private static bool PerformCompressedBackup(Games game, IEnumerable<BackupDir> backupInfo)
        {
            // Get the destination file
            FileSystemPath destinationFile = game.GetCompressedBackupFile();

            // Get the temp file path
            var tempPath = CommonPaths.TempPath + (game.GetBackupName() + CommonPaths.BackupCompressionExtension);

            // Delete temp backup
            RCFRCP.File.DeleteFile(tempPath);

            // Check if a backup already exists
            if (destinationFile.FileExists)
                // Create a new temp backup
                File.Move(destinationFile, tempPath);

            try
            {
                // Create the compressed file
                using (var fileStream = File.OpenWrite(destinationFile))
                {
                    using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Create))
                    {
                        // Enumerate the backup information
                        foreach (var item in backupInfo)
                        {
                            // Get the files
                            var files = Directory.GetFiles(item.DirPath, item.ExtensionFilter ?? "*", item.SearchOption);

                            // Backup each file
                            foreach (FileSystemPath file in files)
                            {
                                // Get the destination file
                                var destFile = item.ID + file.GetRelativePath(item.DirPath);

                                // Copy the file
                                zip.CreateEntryFromFile(file, destFile, CompressionLevel.Optimal);
                            }
                        }
                    }
                }

                // Delete temp backup
                RCFRCP.File.DeleteFile(tempPath);

                RCF.Logger.LogInformationSource($"Backup complete");

                return true;
            }
            catch
            {
                // Check if a temp backup exists
                if (tempPath.FileExists)
                    // Restore temp backup
                    RCFRCP.File.MoveFile(tempPath, destinationFile, true);

                RCF.Logger.LogInformationSource($"Backup failed - clean up succeeded");

                throw;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Performs a backup on the game
        /// </summary>
        /// <param name="game">The game to perform the backup on</param>
        /// <returns>True if the backup was successful</returns>
        public async Task<bool> BackupAsync(Games game)
        {
            using (await AsyncLock.LockAsync())
            {
                RCF.Logger.LogInformationSource($"A backup has been requested for {game}");

                try
                {
                    // Get the backup information
                    var backupInfo = game.GetBackupInfo();

                    // Check if the directories to back up exist
                    if (!backupInfo.Select(x => x.DirPath).DirectoriesExist())
                    {
                        RCF.Logger.LogInformationSource($"Backup failed - the input directories could not be found");

                        await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_MissingDirectoriesError, game.GetDisplayName()), Resources.Backup_FailedHeader, MessageType.Error);
                        return false;
                    }

                    // Check if the backup should be compressed
                    bool compress = RCFRCP.Data.CompressBackups;

                    // Perform the backup and keep track if it succeeded
                    bool success = compress ? PerformCompressedBackup(game, backupInfo) : await PerformBackupAsync(game, backupInfo);

                    // Get the backup locations
                    var compressedLocation = game.GetCompressedBackupFile();
                    var normalLocation = game.GetBackupDir();

                    // Check if the non-relevant one exists
                    try
                    {
                        if (compress && normalLocation.DirectoryExists)
                        {
                            // Delete the directory
                            RCFRCP.File.DeleteDirectory(normalLocation);

                            RCF.Logger.LogInformationSource("Non-compressed backup was deleted due to a compressed backup having been performed");
                        }
                        else if (!compress && compressedLocation.FileExists)
                        {
                            // Delete the file
                            RCFRCP.File.DeleteFile(compressedLocation);

                            RCF.Logger.LogInformationSource("Compressed backup was deleted due to a non-compressed backup having been performed");
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.HandleError("Deleting leftover backups from previous compression setting");
                    }

                    return success;
                }
                catch (Exception ex)
                {   
                    ex.HandleCritical("Backing up game", game);
                    await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.Backup_Failed, game.GetDisplayName()), Resources.Backup_FailedHeader, MessageType.Error);

                    return false;
                }
            }
        }

        /// <summary>
        /// Restores a backup on the game
        /// </summary>
        /// <param name="game">The game to restore the backup on</param>
        /// <returns>True if the backup was successful</returns>
        public async Task<bool> RestoreAsync(Games game)
        {
            using (await AsyncLock.LockAsync())
            {
                RCF.Logger.LogInformationSource($"A backup restore has been requested for {game}");

                try
                {
                    // Get the backup directory
                    var existingBackup = game.GetExistingBackup();

                    // Make sure a backup exists
                    if (!existingBackup?.Exists ?? true)
                    {
                        RCF.Logger.LogInformationSource($"Restore failed - the input location could not be found");

                        await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.Restore_MissingBackup, game.GetDisplayName()), Resources.Restore_FailedHeader, MessageType.Error);
                        return false;
                    }

                    var backupLocation = existingBackup.Value;

                    // Get the backup information
                    var backupInfo = game.GetBackupInfo();

                    // Get the temp path
                    var tempPath = CommonPaths.TempPath + game.GetBackupName();

                    // Delete the temp backup
                    RCFRCP.File.DeleteDirectory(tempPath);

                    // Create the temp path
                    Directory.CreateDirectory(tempPath);

                    // Get temp archive path
                    var archiveTempPath = CommonPaths.TempPath + (game.GetBackupName() + "_archive");

                    try
                    {
                        // If the backup is an archive, extract it
                        if (backupLocation.FileExists)
                        {
                            // Delete the temp archive
                            RCFRCP.File.DeleteDirectory(archiveTempPath);

                            // Create the temp archive path
                            Directory.CreateDirectory(archiveTempPath);

                            using (var file = File.OpenRead(backupLocation))
                                using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
                                    zip.ExtractToDirectory(archiveTempPath);
                        }

                        // Enumerate the backup information
                        foreach (var item in backupInfo)
                        {
                            // Move existing files if the directory exists to temp
                            if (item.DirPath.DirectoryExists)
                            {
                                // Check if the entire directory should be moved
                                if (item.IsEntireDir())
                                {
                                    // Get the destination directory
                                    var destDir = tempPath + item.ID + item.DirPath.Name;

                                    // Move the directory
                                    RCFRCP.File.MoveDirectory(item.DirPath, destDir, true);
                                }
                                else
                                {
                                    // Move each file
                                    foreach (FileSystemPath file in Directory.GetFiles(item.DirPath, item.ExtensionFilter, item.SearchOption))
                                    {
                                        // Get the destination file
                                        var destFile = tempPath + item.ID + file.GetRelativePath(item.DirPath);

                                        // Move the file
                                        RCFRCP.File.MoveFile(file, destFile, true);
                                    }
                                }
                            }

                            // Get the combined directory path
                            var dirPath = (backupLocation.DirectoryExists ? backupLocation : archiveTempPath) + item.ID;

                            // Restore the backup
                            if (dirPath.DirectoryExists)
                                RCFRCP.File.CopyDirectory(dirPath, item.DirPath, false, true);
                        }
                    }
                    catch
                    {
                        // Restore temp backup
                        foreach (var item in backupInfo)
                        {
                            // Get the combined directory path
                            var dirPath = tempPath + item.ID;

                            // Make sure there is a directory to restore
                            if (!dirPath.DirectoryExists)
                                continue;

                            // Check if the entire directory should be moved
                            if (item.IsEntireDir())
                            {
                                // Get the temp directory
                                var tempDir = dirPath + item.DirPath.Name;

                                // Get the destination directory
                                var destDir = item.DirPath;

                                // Move the directory
                                RCFRCP.File.MoveDirectory(tempDir, destDir, true);
                            }
                            else
                            {
                                // Restore each directory
                                foreach (FileSystemPath dir in Directory.GetDirectories(dirPath, "*", SearchOption.AllDirectories))
                                {
                                    // Get the destination directory
                                    var destDir = item.DirPath.Parent + dir.GetRelativePath(item.DirPath);

                                    // Create the directory
                                    Directory.CreateDirectory(destDir);
                                }

                                // Restore each file
                                foreach (FileSystemPath file in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
                                {
                                    // Get the destination file
                                    var destFile = item.DirPath.Parent + file.GetRelativePath(item.DirPath);

                                    // Move the file
                                    RCFRCP.File.MoveFile(file, destFile, true);
                                }
                            }
                        }

                        RCF.Logger.LogInformationSource($"Restore failed - clean up succeeded");

                        throw;
                    }
                    finally
                    {
                        // Delete temp backup
                        RCFRCP.File.DeleteDirectory(tempPath);

                        // Delete the temp archive
                        RCFRCP.File.DeleteDirectory(archiveTempPath);
                    }

                    RCF.Logger.LogInformationSource($"Restore complete");

                    return true;
                }
                catch (Exception ex)
                {
                    ex.HandleCritical("Restoring game", game);
                    await RCF.MessageUI.DisplayMessageAsync(String.Format(Resources.Restore_Failed, game.GetDisplayName()), Resources.Restore_FailedHeader, MessageType.Error);

                    return false;
                }
            }
        }

        #endregion
    }
}