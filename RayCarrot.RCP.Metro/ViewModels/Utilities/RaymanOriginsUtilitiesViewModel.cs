﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ByteSizeLib;
using Microsoft.WindowsAPICodePack.Shell;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.Extensions;
using RayCarrot.IO;
using RayCarrot.UI;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Origins utilities
    /// </summary>
    public class RaymanOriginsUtilitiesViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RaymanOriginsUtilitiesViewModel()
        {
            // Create the commands
            ReplaceVideosCommand = new AsyncRelayCommand(ReplaceVideosAsync);
            UpdateDiscVersionCommand = new AsyncRelayCommand(UpdateDiscVersionAsync);

            // Create properties
            UpdateDebugCommandsAsyncLock = new AsyncLock();
            DebugCommands = new Dictionary<string, string>();
            AvailableMaps = Files.RO_Levels.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            // Get the Rayman Origins install directory
            var instDir = Games.RaymanOrigins.GetInfo().InstallDirectory;

            // Attempt to find the Rayman Origins video directory
            var dir = GetVideosDirectory(instDir);

            // Set to music path if found
            VideoDir = dir.DirectoryExists && (dir + "intro.bik").FileExists ? dir : FileSystemPath.EmptyPath;

            // Indicate if music can be replaces
            CanVideosBeReplaced = VideoDir.DirectoryExists;

            if (CanVideosBeReplaced)
            {
                var result = GetIsOriginalVideos(VideoDir);

                if (result == null)
                    CanVideosBeReplaced = false;
                else
                    IsOriginalVideos = result.Value;
            }

            DebugCommandFilePath = GetDebugCommandFilePath(instDir);

            if (!DebugCommandFilePath.FileExists)
            {
                IsDebugModeEnabled = false;
                return;
            }

            try
            {
                // Check each line for a command
                foreach (var line in File.ReadAllLines(DebugCommandFilePath))
                {
                    // Split at the equals sign
                    var ls = line.Split('=');

                    // Make sure it was split into two strings
                    if (ls.Length != 2)
                        continue;

                    // Add the command
                    DebugCommands.Add(ls[0], ls[1]);
                }
            }
            catch (Exception ex)
            {
                ex.HandleError($"Reading RO command file");
            }

        }

        #endregion

        #region Private Constants

        private const string InvincibilityKey = "player_nodamage";

        private const string MouseHiddenKey = "nomouse";

        private const string MaxZoomKey = "camera_maxdezoom";

        private const string FramerateKey = "fps";

        private const string MapKey = "map";

        private const string LanguageKey = "language";

        #endregion

        #region Private Fields

        private bool _isDebugModeEnabled = true;

        #endregion

        #region Private Properties

        /// <summary>
        /// The async lock for <see cref="UpdateDebugCommandsAsync"/>
        /// </summary>
        private AsyncLock UpdateDebugCommandsAsyncLock { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The Rayman Origins video directory
        /// </summary>
        public FileSystemPath VideoDir { get; }

        /// <summary>
        /// Indicates if the Rayman Origins videos can be replaced
        /// </summary>
        public bool CanVideosBeReplaced { get; set; }

        /// <summary>
        /// Indicates if the current video files are the original ones
        /// </summary>
        public bool IsOriginalVideos { get; set; }

        /// <summary>
        /// The Rayman Origins debug command file path
        /// </summary>
        public FileSystemPath DebugCommandFilePath { get; }

        /// <summary>
        /// Indicates if debug mode is enabled
        /// </summary>
        public bool IsDebugModeEnabled
        {
            get => _isDebugModeEnabled;
            set
            {
                _isDebugModeEnabled = value;
                _ = UpdateDebugCommandsAsync();
            }
        }

        /// <summary>
        /// The available debug commands
        /// </summary>
        public Dictionary<string, string> DebugCommands { get; }

        /// <summary>
        /// Indicates if invincibility is enabled
        /// </summary>
        public bool? IsInvincibilityEnabled
        {
            get
            {
                var value = DebugCommands.TryGetValue(InvincibilityKey);

                if (value == null)
                    return null;

                return value == "1";
            }
            set
            {
                if (value is bool v)
                    DebugCommands.SetValue(InvincibilityKey, v ? "1" : "0");
                else
                    DebugCommands.Remove(InvincibilityKey);

                _ = UpdateDebugCommandsAsync();
            }
        }

        /// <summary>
        /// Indicates if the mouse should be hidden
        /// </summary>
        public bool? IsMouseHidden
        {
            get
            {
                var value = DebugCommands.TryGetValue(MouseHiddenKey);

                if (value == null)
                    return null;

                return value == "1";
            }
            set
            {
                if (value is bool v)
                    DebugCommands.SetValue(MouseHiddenKey, v ? "1" : "0");
                else
                    DebugCommands.Remove(MouseHiddenKey);

                _ = UpdateDebugCommandsAsync();
            }
        }

        /// <summary>
        /// Indicates if the camera should be zoomed out to max
        /// </summary>
        public bool? IsCameraMaxZoom
        {
            get
            {
                var value = DebugCommands.TryGetValue(MaxZoomKey);

                if (value == null)
                    return null;

                return value == "1";
            }
            set
            {
                if (value is bool v)
                    DebugCommands.SetValue(MaxZoomKey, v ? "1" : "0");
                else
                    DebugCommands.Remove(MaxZoomKey);

                _ = UpdateDebugCommandsAsync();
            }
        }

        /// <summary>
        /// The game framerate
        /// </summary>
        public double? SelectedFramerate
        {
            get => Double.TryParse(DebugCommands.TryGetValue(FramerateKey), out double result) ? (double?)result : null;
            set
            {
                if (value is double v && v > 0)
                    DebugCommands.SetValue(FramerateKey, v.ToString(CultureInfo.InvariantCulture));
                else
                    DebugCommands.Remove(FramerateKey);

                _ = UpdateDebugCommandsAsync();
            }
        }

        /// <summary>
        /// The available map paths to choose from
        /// </summary>
        public string[] AvailableMaps { get; }

        /// <summary>
        /// The path of the map to load
        /// </summary>
        public string MapPath
        {
            get => DebugCommands.TryGetValue(MapKey);
            set
            {
                if (value is string v && !v.IsNullOrWhiteSpace())
                    DebugCommands.SetValue(MapKey, v);
                else
                    DebugCommands.Remove(MapKey);

                _ = UpdateDebugCommandsAsync();
            }
        }

        /// <summary>
        /// The selected language
        /// </summary>
        public ROLanguages Language
        {
            get => Enum.TryParse(DebugCommands.TryGetValue(LanguageKey), out ROLanguages result) ? result : ROLanguages.English;
            set
            {
                DebugCommands.SetValue(LanguageKey, ((int)value).ToString());

                _ = UpdateDebugCommandsAsync();
            }
        }

        #endregion

        #region Commands

        public ICommand ReplaceVideosCommand { get; }

        public ICommand UpdateDiscVersionCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Replaces the current videos
        /// </summary>
        /// <returns>The task</returns>
        public async Task ReplaceVideosAsync()
        {
            try
            {
                RCFCore.Logger?.LogInformationSource($"The Rayman Origins videos are being replaced with {(IsOriginalVideos ? "HQ Videos" : "original videos")}");

                // Download the files
                var succeeded = await App.DownloadAsync(new Uri[]
                {
                    new Uri(IsOriginalVideos ? CommonUrls.RO_HQVideos_URL : CommonUrls.RO_OriginalVideos_URL), 
                }, true, VideoDir);

                if (succeeded)
                    IsOriginalVideos ^= true;
            }
            catch (Exception ex)
            {
                ex.HandleError("Replacing RO videos");
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.ROU_HQVideosFailed, MessageType.Error);
            }
        }

        /// <summary>
        /// Updates the disc version to the latest version (1.02)
        /// </summary>
        /// <returns>The task</returns>
        public async Task UpdateDiscVersionAsync()
        {
            try
            {
                RCFCore.Logger?.LogInformationSource($"The Rayman Origins disc updater is being downloaded...");

                // Download the file
                var succeeded = await App.DownloadAsync(new Uri[]
                {
                    new Uri(CommonUrls.RO_Updater_URL)
                }, true, KnownFolders.Downloads.Path);

                if (succeeded)
                    (await RCFRCP.File.LaunchFileAsync(Path.Combine(KnownFolders.Downloads.Path, "RaymanOriginspc_1.02.exe")))?.Dispose();
            }
            catch (Exception ex)
            {
                ex.HandleError("Downloading RO updater");
                await RCFUI.MessageUI.DisplayMessageAsync(Resources.ROU_UpdateFailed, MessageType.Error);
            }


        }

        /// <summary>
        /// Updates the Rayman Origins debug commands used
        /// </summary>
        /// <returns>The task</returns>
        public async Task UpdateDebugCommandsAsync()
        {
            using (await UpdateDebugCommandsAsyncLock.LockAsync())
            {
                RCFCore.Logger?.LogInformationSource($"The Rayman Origins debug commands are being updated...");

                // Make sure the install directory was found
                if (!DebugCommandFilePath.Parent.DirectoryExists)
                {
                    IsDebugModeEnabled = false;

                    RCFCore.Logger?.LogWarningSource($"The Rayman Origins debug commands could not be updated due to the install directory not being found");

                    await RCFUI.MessageUI.DisplayMessageAsync(Resources.ROU_DebugCommandsInstallationNotFound, MessageType.Error);
                    return;
                }

                try
                {
                    RCFRCP.File.DeleteFile(DebugCommandFilePath);

                    if (!IsDebugModeEnabled)
                    {
                        RCFCore.Logger?.LogInformationSource($"The Rayman Origins debug commands have been disabled");

                        return;
                    }

                    File.WriteAllLines(DebugCommandFilePath, DebugCommands.Select(x => $"{x.Key}={x.Value}"));

                    RCFCore.Logger?.LogInformationSource($"The Rayman Origins debug commands have been updated");
                }
                catch (Exception ex)
                {
                    ex.HandleError("Applying RO debug commands");
                    await RCFUI.MessageUI.DisplayMessageAsync(Resources.ROU_DebugCommandsError, MessageType.Error);
                }
            }
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Gets the videos directory
        /// </summary>
        /// <param name="installDir">The game install directory</param>
        /// <returns>The directory path</returns>
        public static FileSystemPath GetVideosDirectory(FileSystemPath installDir)
        {
            return installDir + "GameData";
        }

        /// <summary>
        /// Gets a value indicating if the original videos are available in the specified path
        /// </summary>
        /// <param name="path">The video directory</param>
        /// <returns>True if the original videos are available, false if not. Null if an error occurred while checking.</returns>
        public static bool? GetIsOriginalVideos(FileSystemPath path)
        {
            try
            {
                var file = path + "intro.bik";

                if (!path.FileExists)
                    return null;

                var size = file.GetSize();

                return size == new ByteSize(59748732);
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting RO video size");
                return null;
            }
        }

        /// <summary>
        /// Gets the debug command file path
        /// </summary>
        /// <param name="installDir">The game install directory</param>
        /// <returns>The file path</returns>
        public static FileSystemPath GetDebugCommandFilePath(FileSystemPath installDir)
        {
            return installDir + "cmdline.txt";
        }

        #endregion

        #region Public Enums

        /// <summary>
        /// The available Rayman Origins languages
        /// </summary>
        public enum ROLanguages
        {
            English,
            French,
            Japanese,
            German,
            Spanish,
            Italian,
        }

        #endregion
    }
}