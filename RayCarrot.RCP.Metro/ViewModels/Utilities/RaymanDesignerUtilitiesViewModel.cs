﻿using RayCarrot.CarrotFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Designer utilities
    /// </summary>
    public class RaymanDesignerUtilitiesViewModel : BaseRCPViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RaymanDesignerUtilitiesViewModel()
        {
            // Create commands
            ReplaceRayKitCommand = new AsyncRelayCommand(ReplaceRayKitAsync);
            CreateConfigCommand = new AsyncRelayCommand(CreateConfigAsync);

            // Default properties
            MapperLanguage = RaymanDesignerMapperLanguage.English;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// The selected Mapper language
        /// </summary>
        public RaymanDesignerMapperLanguage MapperLanguage { get; set; }

        #endregion

        #region Commands

        public ICommand ReplaceRayKitCommand { get; }

        public ICommand CreateConfigCommand { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Replaces the infected Rayman Designer files
        /// </summary>
        /// <returns>The task</returns>
        public async Task ReplaceRayKitAsync()
        {
            RCF.Logger.LogInformationSource($"The Rayman Designer replacement patch is downloading...");

            // Find the files to be replaced
            var files = new Tuple<string, Uri>[]
            {
                new Tuple<string, Uri>("CLIENT.EXE", new Uri(CommonUrls.RD_ClientExe_URL)),
                new Tuple<string, Uri>("RAYRUN.EXE", new Uri(CommonUrls.RD_RayrunExe_URL)), 
                new Tuple<string, Uri>("STARTUP.EXE", new Uri(CommonUrls.RD_StartupExe_URL)),
                new Tuple<string, Uri>("MAPPER.EXE", new Uri(
                    MapperLanguage == RaymanDesignerMapperLanguage.English ? CommonUrls.RD_USMapperExe_URL :
                        MapperLanguage == RaymanDesignerMapperLanguage.French ? CommonUrls.RD_FRMapperExe_URL : CommonUrls.RD_ALMapperExe_URL)), 
            };

            // Get the game info
            var gameInfo = Games.RaymanDesigner.GetInfo();

            // Find the directories to search
            var dirs = new FileSystemPath[]
            {
                gameInfo.InstallDirectory,
                gameInfo.InstallDirectory + "OSD"
            };

            // Keep track of the found files
            var foundFiles = new List<Tuple<FileSystemPath, Uri>>();

            // Search for the files
            foreach (var file in files)
            {
                // Check each directory
                foreach (var dir in dirs)
                {
                    // Get the path
                    var path = dir + file.Item1;

                    // Check if the path exists
                    if (!path.FileExists)
                        continue;

                    foundFiles.Add(new Tuple<FileSystemPath, Uri>(dir, file.Item2));
                    break;
                }
            }

            RCF.Logger.LogInformationSource($"The following Rayman Designer files were found to replace: {foundFiles.Select(x => x.Item1.Name).JoinItems(", ")}");

            await RCF.MessageUI.DisplayMessageAsync($"{foundFiles.Count}/{files.Length} files were found to replace. This might require several downloads depending on their locations.", "Information", MessageType.Information);

            try
            {
                // Get the download groups
                var groups = foundFiles.GroupBy(x => x.Item1);

                // Download each group
                foreach (var group in groups)
                    // Download the files
                    await App.DownloadAsync(group.Select(x => x.Item2).ToList(), false, group.Key);

                RCF.Logger.LogInformationSource($"The Rayman Designer files have been replaced");

                await RCF.MessageUI.DisplayMessageAsync("Replacement complete", "Operation complete", MessageType.Information);
            }
            catch (Exception ex)
            {
                ex.HandleError("Replacing R1 soundtrack");
                await RCF.MessageUI.DisplayMessageAsync("Soundtrack replacement failed.", "Error", MessageType.Error);
            }
        }

        /// <summary>
        /// Creates the Rayman Designer configuration file
        /// </summary>
        /// <returns>The task</returns>
        public async Task CreateConfigAsync()
        {
            // Get the path
            var path = Games.RaymanDesigner.GetInfo().InstallDirectory + "Ubisoft" + "ubi.ini";

            // Check if the file exists
            if (path.FileExists)
            {
                if (!await RCF.MessageUI.DisplayMessageAsync("The configuration file already exists. You can still recreate it if it is corrupt. Continue?", "Replace File", MessageType.Question, true))
                    return;
            }

            try
            {
                Directory.CreateDirectory(path.Parent);

                // Create the file
                File.WriteAllLines(path, new string[]
                {
                    "[OSD]",
                    "Directory =.\\ubisoft\\osd",
                    "Valid = TRUE",
                    String.Empty,
                    "[INSTALLED PRODUCTS]",
                    "RAYKIT",
                    String.Empty,
                    "[RAYKIT]",
                    "SrcDataPath =\\",
                    "Directory =.\\"
                });

                RCF.Logger.LogInformationSource($"The Rayman Designer config file has been recreated");

                await RCF.MessageUI.DisplaySuccessfulActionMessageAsync("The file was successfully created.", "Action complete");
            }
            catch (Exception ex)
            {
                ex.HandleError("Applying RD config patch");
                await RCF.MessageUI.DisplayMessageAsync("The patch could not be applied.", "Error", MessageType.Error);
            }
        }

        #endregion

        #region Public Enums

        /// <summary>
        /// The available Rayman Designer Mapper languages
        /// </summary>
        public enum RaymanDesignerMapperLanguage
        {
            English,
            German,
            French
        }

        #endregion
    }
}