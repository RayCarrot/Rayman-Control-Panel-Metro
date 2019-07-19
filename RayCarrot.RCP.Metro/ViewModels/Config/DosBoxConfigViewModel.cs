﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for DosBox configuration
    /// </summary>
    public class DosBoxConfigViewModel : GameConfigViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor without default game <see cref="Games.Rayman1"/>
        /// </summary>
        public DosBoxConfigViewModel() : this(Games.Rayman1)
        { }

        /// <summary>
        /// Default constructor for a specific game
        /// </summary>
        /// <param name="game">The DosBox game</param>
        public DosBoxConfigViewModel(Games game)
        {
            Game = game;

            // Create the async lock
            AsyncLock = new AsyncLock();

            // Create the commands
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            UseRecommendedCommand = new RelayCommand(UseRecommended);

            // Set up the available resolution values
            AvailableResolutionValues = new ObservableCollection<string>();

            const double ratio = 16d / 10d;
            const int minHeight = 200;
            double maxHeight = SystemParameters.PrimaryScreenHeight;

            AvailableResolutionValues.Add($"Original");

            for (int height = minHeight; height <= maxHeight; height = height + minHeight)
                AvailableResolutionValues.Add($"{height * ratio}x{height}");

            // NOTE: Below options are not localized

            // Set available DosBox outputs
            AvailableDosBoxOutputs = new string[]
            {
                "default",
                "surface",
                "overlay",
                "opengl",
                "openglnb",
                "ddraw"
            };

            // Set available DosBox scalers
            AvailableDosBoxScalers = new string[]
            {
                "default",
                "none",
                "normal2x",
                "normal3x",
                "advmame2x",
                "advmame3x",
                "hq2x",
                "hq3x",
                "2xsai",
                "super2xsai",
                "supereagle",
                "advinterp2x",
                "advinterp3x",
                "tv2x",
                "tv3x",
                "rgb2x",
                "rgb3x",
                "scan2x",
                "scan3x"
            };

            // Set available DosBox core modes
            AvailableDosBoxCoreModes = new string[]
            {
                "default",
                "normal",
                "simple",
                "dynamic",
                "auto"
            };

            // Set available DosBox cycle modes
            AvailableDosBoxCycleModes = new string[]
            {
                "default",
                "auto",
                "max"
            };
        }

        #endregion

        #region Private Fields

        private FileSystemPath _mountPath;

        private bool? _fullscreenEnabled;

        private bool? _fullDoubleEnabled;

        private bool? _aspectCorrectionEnabled;

        private double? _memorySize;

        private double? _frameskip;

        private string _selectedOutput;

        private string _fullscreenResolution;

        private string _windowedResolution;

        private string _selectedScaler;

        private string _selectedCoreMode;

        private string _selectedCycles;

        private string _customCommands;

        private R1Languages _gameLanguage;

        #endregion

        #region Private Constants

        public const string FullScreenKey = "fullscreen";

        public const string FullDoubleKey = "fulldouble";

        public const string AspectCorrectionKey = "aspect";

        public const string MemorySizeKey = "memsize";

        public const string FrameskipKey = "frameskip";

        public const string OutputKey = "output";

        public const string FullscreenResolutionKey = "fullresolution";

        public const string WindowedResolutionKey = "windowresolution";

        public const string ScalerKey = "scaler";

        public const string CoreKey = "core";

        public const string CyclesKey = "cycles";

        #endregion

        #region Private Properties

        /// <summary>
        /// The async lock to use for saving the configuration
        /// </summary>
        private AsyncLock AsyncLock { get; }

        #endregion
    
        #region Public Properties

        /// <summary>
        /// The DosBox game
        /// </summary>
        public Games Game { get; }

        /// <summary>
        /// The available resolution values to use
        /// </summary>
        public ObservableCollection<string> AvailableResolutionValues { get; }

        /// <summary>
        /// The available DosBox outputs to use
        /// </summary>
        public string[] AvailableDosBoxOutputs { get; }

        /// <summary>
        /// The available DosBox scalers to use
        /// </summary>
        public string[] AvailableDosBoxScalers { get; }

        /// <summary>
        /// The available DosBox core modes to use
        /// </summary>
        public string[] AvailableDosBoxCoreModes { get; }

        /// <summary>
        /// The available DosBox cycle modes to use
        /// </summary>
        public string[] AvailableDosBoxCycleModes { get; }

        /// <summary>
        /// The file or directory to mount
        /// </summary>
        public FileSystemPath MountPath
        {
            get => _mountPath;
            set
            {
                _mountPath = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// Indicates if fullscreen has been enabled
        /// </summary>
        public bool? FullscreenEnabled
        {
            get => _fullscreenEnabled;
            set
            {
                _fullscreenEnabled = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// Indicates if fullscreen double buffering has been enabled
        /// </summary>
        public bool? FullDoubleEnabled
        {
            get => _fullDoubleEnabled;
            set
            {
                _fullDoubleEnabled = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// Indicates if aspect ratio correction has been enabled
        /// </summary>
        public bool? AspectCorrectionEnabled
        {
            get => _aspectCorrectionEnabled;
            set
            {
                _aspectCorrectionEnabled = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The selected memory size
        /// </summary>
        public double? MemorySize
        {
            get => _memorySize;
            set
            {
                _memorySize = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The selected frame skip value
        /// </summary>
        public double? Frameskip
        {
            get => _frameskip;
            set
            {
                _frameskip = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The selected output to use
        /// </summary>
        public string SelectedOutput
        {
            get => _selectedOutput;
            set
            {
                _selectedOutput = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The selected fullscreen resolution
        /// </summary>
        public string FullscreenResolution
        {
            get => _fullscreenResolution;
            set
            {
                _fullscreenResolution = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The selected fullscreen resolution
        /// </summary>
        public string WindowedResolution
        {
            get => _windowedResolution;
            set
            {
                _windowedResolution = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The selected scaler to use
        /// </summary>
        public string SelectedScaler
        {
            get => _selectedScaler;
            set
            {
                _selectedScaler = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The selected core mode to use
        /// </summary>
        public string SelectedCoreMode
        {
            get => _selectedCoreMode;
            set
            {
                _selectedCoreMode = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The selected cycles to use
        /// </summary>
        public string SelectedCycles
        {
            get => _selectedCycles;
            set
            {
                _selectedCycles = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The custom DosBox commands to use
        /// </summary>
        public string CustomCommands
        {
            get => _customCommands;
            set
            {
                _customCommands = value.Replace('\"', '\'');
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// The selected game language, if any
        /// </summary>
        public R1Languages GameLanguage
        {
            get => _gameLanguage;
            set
            {
                _gameLanguage = value;
                UnsavedChanges = true;
            }
        }

        /// <summary>
        /// Indicates if <see cref="GameLanguage"/> is available
        /// </summary>
        public bool IsGameLanguageAvailable { get; set; }

        /// <summary>
        /// Indicates if <see cref="R1Languages.English"/> is available
        /// </summary>
        public bool IsEnglishAvailable { get; set; }

        /// <summary>
        /// Indicates if <see cref="R1Languages.French"/> is available
        /// </summary>
        public bool IsFrenchAvailable { get; set; }

        /// <summary>
        /// Indicates if <see cref="R1Languages.German"/> is available
        /// </summary>
        public bool IsGermanAvailable { get; set; }

        #endregion

        #region Commands

        public AsyncRelayCommand SaveCommand { get; }

        public RelayCommand UseRecommendedCommand { get; }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Gets the Rayman 1 language from the specified config file
        /// </summary>
        /// <param name="configFile">The config file to get the language from</param>
        /// <returns>The language or null if none was found</returns>
        private static R1Languages? GetR1Language(FileSystemPath configFile)
        {
            try
            {
                // Open the file
                using (var stream = File.OpenRead(configFile))
                {
                    switch (stream.ReadByte())
                    {
                        case 0:
                            return R1Languages.English;

                        case 1:
                            return R1Languages.French;

                        case 2:
                            return R1Languages.German;

                        default:
                            return null;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting Rayman 1 language from config file");
                return null;
            }
        }

        /// <summary>
        /// Sets the Rayman 1 language in the specified config file
        /// </summary>
        /// <param name="configFile">The config file to set the language in</param>
        /// <param name="language">The language</param>
        /// <returns>The task</returns>
        private static async Task SetR1LanguageAsync(FileSystemPath configFile, R1Languages language)
        {
            try
            {
                using (var stream = File.OpenWrite(configFile))
                    stream.WriteByte((byte)language);
            }
            catch (Exception ex)
            {
                ex.HandleError("Setting Rayman 1 game language from config file");
                await RCF.MessageUI.DisplayMessageAsync(Resources.DosBoxConfig_SetLanguageError, Resources.DosBoxConfig_SetLanguageErrorHeader, MessageType.Error);
            }
        }

        /// <summary>
        /// Gets the current game language from the specified batch file
        /// </summary>
        /// <param name="batchFile">The batch file to get the language from</param>
        /// <returns>The language or null if none was found</returns>
        private static R1Languages? GetBatchFileanguage(FileSystemPath batchFile)
        {
            try
            {
                // Read the file into an array
                var file = File.ReadAllLines(batchFile);

                // Check each line for the launch argument
                foreach (string line in file)
                {
                    // Find the argument
                    var index = line.IndexOf("ver=", StringComparison.Ordinal);

                    if (index == -1)
                        continue;

                    string lang = line.Substring(index + 4);

                    if (lang.Equals("usa", StringComparison.InvariantCultureIgnoreCase))
                        return R1Languages.English;

                    if (lang.Equals("fr", StringComparison.InvariantCultureIgnoreCase))
                        return R1Languages.French;

                    if (lang.Equals("al", StringComparison.InvariantCultureIgnoreCase))
                        return R1Languages.German;
                    
                    return null;
                }

                return null;
            }
            catch (Exception ex)
            {
                ex.HandleError("Getting DOSBox game language from batch file");
                return null;
            }
        }

        /// <summary>
        /// Sets the current game language in the specified batch file
        /// </summary>
        /// <param name="batchFile">The batch file to set the language in</param>
        /// <param name="language">The language</param>
        /// <param name="game">The game to set the language for</param>
        /// <returns>The task</returns>
        private static async Task SetBatchFileLanguageAsync(FileSystemPath batchFile, R1Languages language, Games game)
        {
            try
            {
                string lang;

                switch (language)
                {
                    case R1Languages.English:
                        lang = "usa";
                        break;

                    case R1Languages.French:
                        lang = "fr";
                        break;

                    case R1Languages.German:
                        lang = "al";
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(language), language, null);
                }

                // Delete the existing file
                RCFRCP.File.DeleteFile(batchFile);

                // Create the .bat file
                File.WriteAllLines(batchFile, new string[]
                {
                    "@echo off",
                    $"{Path.GetFileNameWithoutExtension(new DOSBoxGameManager(game).GetGameExectuable())} ver={lang}"
                });
            }
            catch (Exception ex)
            {
                ex.HandleError("Setting DOSBox game language from batch file");
                await RCF.MessageUI.DisplayMessageAsync(Resources.DosBoxConfig_SetLanguageError, Resources.DosBoxConfig_SetLanguageErrorHeader, MessageType.Error);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads and sets up the current configuration properties
        /// </summary>
        /// <returns>The task</returns>
        public override Task SetupAsync()
        {
            RCF.Logger.LogInformationSource($"{Game.GetDisplayName()} config is being set up");

            // Get the current DosBox options for the specified game
            var options = Data.DosBoxGames[Game];

            MountPath = options.MountPath;

            // Default game language to not be available
            IsGameLanguageAvailable = false;

            var instDir = Game.GetInfo().InstallDirectory;

            // Attempt to get the game language from the .bat file or config file
            var batchFile = instDir + Game.GetLaunchName();
            var configFile = instDir + "RAYMAN.CFG";

            if (Game == Games.Rayman1 && configFile.FileExists || batchFile.FullPath.EndsWith(".bat", StringComparison.InvariantCultureIgnoreCase) && batchFile.FileExists)
            {
                // Check language availability
                if (Game != Games.Rayman1)
                {
                    var pcmapDir = instDir + "pcmap";

                    IsEnglishAvailable = (pcmapDir + "usa").DirectoryExists;
                    IsFrenchAvailable = (pcmapDir + "fr").DirectoryExists;
                    IsGermanAvailable = (pcmapDir + "al").DirectoryExists;
                }
                else
                {
                    // NOTE: Currently there is no consistent way to check the availability in Rayman 1
                    IsEnglishAvailable = true;
                    IsFrenchAvailable = true;
                    IsGermanAvailable = true;
                }

                // Make sure at least one language is available
                if (IsEnglishAvailable || IsFrenchAvailable || IsGermanAvailable)
                {
                    var lang = Game == Games.Rayman1 ? GetR1Language(configFile) : GetBatchFileanguage(batchFile);

                    if (lang != null)
                    {
                        GameLanguage = lang.Value;
                        IsGameLanguageAvailable = true;
                    }
                }
            }

            // Get the config manager
            var configManager = new DosBoxAutoConfigManager(Game.GetDosBoxConfigFile());

            // Create the file
            configManager.Create();

            // Read the content
            var configData = configManager.ReadFile();

            FullscreenEnabled = GetBool(FullScreenKey);
            FullDoubleEnabled = GetBool(FullDoubleKey);
            AspectCorrectionEnabled = GetBool(AspectCorrectionKey);
            MemorySize = GetDouble(MemorySizeKey);
            Frameskip = GetDouble(FrameskipKey);
            SelectedOutput = GetString(OutputKey, "default");
            FullscreenResolution = GetString(FullscreenResolutionKey, "Original");
            WindowedResolution = GetString(WindowedResolutionKey, "Original");
            SelectedScaler = GetString(ScalerKey, "default");
            SelectedCoreMode = GetString(CoreKey, "default");
            SelectedCycles = GetString(CyclesKey, "default");

            CustomCommands = configData.CustomLines.JoinItems(Environment.NewLine);

            UnsavedChanges = false;

            RCF.Logger.LogInformationSource($"DosBox configuration for {Game} has been loaded");

            return Task.CompletedTask;

            // Helper methods for getting properties
            bool? GetBool(string propName) => Boolean.TryParse(configData.Commands.TryGetValue(propName), out bool output) ? (bool?)output : null;
            string GetString(string propName, string defaultValue = null) => configData.Commands.TryGetValue(propName) ?? defaultValue;
            double? GetDouble(string propName) => Double.TryParse(configData.Commands.TryGetValue(propName), out double output) ? (double?)output : null;
        }

        /// <summary>
        /// Saves the changes
        /// </summary>
        /// <returns>The task</returns>
        public async Task SaveAsync()
        {
            using (await AsyncLock.LockAsync())
            {
                RCF.Logger.LogInformationSource($"DosBox configuration for {Game} is saving...");

                try
                {
                    // Get the current DosBox options for the specified game
                    var options = Data.DosBoxGames[Game];

                    options.MountPath = MountPath;

                    // If game language is available, update it
                    if (IsGameLanguageAvailable)
                    {
                        if (Game == Games.Rayman1)
                            await SetR1LanguageAsync(Game.GetInfo().InstallDirectory + "RAYMAN.CFG", GameLanguage);
                        else
                            await SetBatchFileLanguageAsync(Game.GetInfo().InstallDirectory + Game.GetLaunchName(), GameLanguage, Game);
                    }

                    // Get the config manager
                    var configManager = new DosBoxAutoConfigManager(Game.GetDosBoxConfigFile());

                    // Create config data
                    var configData = new DosBoxAutoConfigData();

                    // Add custom commands
                    configData.CustomLines.AddRange(CustomCommands.Split(new[] { Environment.NewLine }, StringSplitOptions.None));

                    // Add commands
                    SetProp(FullScreenKey, FullscreenEnabled);
                    SetProp(FullDoubleKey, FullDoubleEnabled);
                    SetProp(AspectCorrectionKey, AspectCorrectionEnabled);
                    SetProp(MemorySizeKey, MemorySize);
                    SetProp(FrameskipKey, Frameskip);
                    SetProp(OutputKey, SelectedOutput, true);
                    SetProp(FullscreenResolutionKey, FullscreenResolution);
                    SetProp(WindowedResolutionKey, WindowedResolution);
                    SetProp(ScalerKey, SelectedScaler, true);
                    SetProp(CoreKey, SelectedCoreMode, true);
                    SetProp(CyclesKey, SelectedCycles, true);

                    // Write to the config file
                    configManager.WriteFile(configData);

                    RCF.Logger.LogInformationSource($"DosBox configuration for {Game} has been saved");

                    // Helper methods for setting properties
                    void SetProp(string propName, object value, bool ignoreDefault = false)
                    {
                        if ((value != null && (!ignoreDefault || !value.ToString().Equals("default", StringComparison.CurrentCultureIgnoreCase))))
                            configData.Commands[propName] = value.ToString();
                        else if (configData.Commands.ContainsKey(propName))
                            configData.Commands.Remove(propName);
                    }
                }
                catch (Exception ex)
                {
                    ex.HandleError("Saving DosBox configuration data");
                    await RCF.MessageUI.DisplayMessageAsync(Resources.Config_DosBoxSaveError, Resources.Config_SaveErrorHeader, MessageType.Error);
                    return;
                }

                UnsavedChanges = false;

                await RCF.MessageUI.DisplaySuccessfulActionMessageAsync(Resources.Config_SaveSuccess);

                OnSave();
            }
        }

        /// <summary>
        /// Applies the recommended settings for the specified game
        /// </summary>
        public void UseRecommended()
        {
            AspectCorrectionEnabled = false;
            MemorySize = 30;
            Frameskip = 0;
            SelectedOutput = "default";
            SelectedCycles = "20000";

            RCF.Logger.LogTraceSource($"Recommended DosBox settings were applied");
        }

        #endregion
    }

    public enum R1Languages
    {
        English,
        French,
        German
    }
}