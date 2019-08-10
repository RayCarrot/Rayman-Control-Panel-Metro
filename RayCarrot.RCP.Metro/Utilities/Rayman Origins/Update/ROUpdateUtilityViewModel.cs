﻿using RayCarrot.CarrotFramework.Abstractions;
using RayCarrot.UI;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Shell;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Origins update utility
    /// </summary>
    public class ROUpdateUtilityViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ROUpdateUtilityViewModel()
        {
            // Create the commands
            UpdateDiscVersionCommand = new AsyncRelayCommand(UpdateDiscVersionAsync);
        }

        #endregion

        #region Commands

        public ICommand UpdateDiscVersionCommand { get; }

        #endregion

        #region Public Methods

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

        #endregion
    }
}