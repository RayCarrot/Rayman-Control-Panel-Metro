﻿using RayCarrot.Binary;
using RayCarrot.Common;
using RayCarrot.IO;
using RayCarrot.Logging;
using RayCarrot.Rayman;
using RayCarrot.Rayman.Ray1;
using RayCarrot.WPF;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RayCarrot.RCP.Metro
{
    public abstract class Ray_1_KIT_EDU_BaseConfigViewModel : GameOptions_ConfigPageViewModel
    {
        #region Constructor

        /// <summary>
        /// Default constructor for a specific game
        /// </summary>
        /// <param name="game">The DosBox game</param>
        /// <param name="ray1Game">The Rayman 1 game</param>
        /// <param name="langMode">The language mode to use</param>
        protected Ray_1_KIT_EDU_BaseConfigViewModel(Games game, Ray1Game ray1Game, LanguageMode langMode)
        {
            Game = game;
            Ray1Game = ray1Game;
            LangMode = langMode;
            IsVoicesVolumeAvailable = Ray1Game != Ray1Game.Rayman1;

            FrameRateOptions_Values = new Rayman1Freq[]
            {
                Rayman1Freq.Freq_50,
                Rayman1Freq.Freq_60,
                Rayman1Freq.Freq_70,
                Rayman1Freq.Freq_80,
                Rayman1Freq.Freq_100,
                Rayman1Freq.Freq_Max
            };
            FrameRateOptions_Names = new string[]
            {
                "50 hz",
                "60 hz",
                "70 hz",
                "80 hz",
                "100 hz",
                "Max",
            };

            KeyItems = new ObservableCollection<ButtonMappingKeyItemViewModel>()
            {
                new ButtonMappingKeyItemViewModel(new LocalizedString(() => Resources.Config_Action_Left), Key.NumPad4, this),
                new ButtonMappingKeyItemViewModel(new LocalizedString(() => Resources.Config_Action_Up), Key.NumPad8, this),
                new ButtonMappingKeyItemViewModel(new LocalizedString(() => Resources.Config_Action_Right), Key.NumPad6, this),
                new ButtonMappingKeyItemViewModel(new LocalizedString(() => Resources.Config_Action_Down), Key.NumPad2, this),
                new ButtonMappingKeyItemViewModel(new LocalizedString(() => Resources.Config_Action_Jump), Key.LeftCtrl, this),
                new ButtonMappingKeyItemViewModel(new LocalizedString(() => Resources.Config_Action_Fist), Key.LeftAlt, this),
                new ButtonMappingKeyItemViewModel(new LocalizedString(() => Resources.Config_Action_Action), Key.X, this),
            };
        }

        #endregion

        #region Private Fields

        private R1Languages _gameLanguage;
        private bool _isMusicEnabled;
        private bool _isStero;
        private int _soundVolume;
        private int _voicesVolume;
        private bool _showBackground;
        private bool _showParallaxBackground;
        private bool _showHud;
        private int _selectedFrameRateOption;
        private int _zoneOfPlay;
        private int _gamePadJump;
        private int _gamePadFist;
        private int _gamePadAction;
        private int _xPadMax;
        private int _xPadMin;
        private int _yPadMax;
        private int _yPadMin;
        private int _xPadCenter;
        private int _yPadCenter;
        private int _port;
        private int _irq;
        private int _dma;
        private int _param;
        private int _deviceId;
        private int _numCard;

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if the option to use recommended options in the page is available
        /// </summary>
        public override bool CanUseRecommended => true;

        /// <summary>
        /// The DosBox game
        /// </summary>
        public Games Game { get; }

        /// <summary>
        /// The Rayman 1 game
        /// </summary>
        public Ray1Game Ray1Game { get; }

        /// <summary>
        /// The language mode to use
        /// </summary>
        public LanguageMode LangMode { get; }

        /// <summary>
        /// The game configuration
        /// </summary>
        public Rayman1PCConfigData Config { get; set; }

        /// <summary>
        /// The file path for the config file
        /// </summary>
        public FileSystemPath ConfigFilePath { get; set; }

        // Language

        /// <summary>
        /// Indicates if changing the game language is available
        /// </summary>
        public bool IsGameLanguageAvailable { get; set; }

        /// <summary>
        /// Indicates if <see cref="R1Languages.English"/> is available
        /// </summary>
        public bool IsEnglishAvailable { get; set; } = true;

        /// <summary>
        /// Indicates if <see cref="R1Languages.French"/> is available
        /// </summary>
        public bool IsFrenchAvailable { get; set; } = true;

        /// <summary>
        /// Indicates if <see cref="R1Languages.German"/> is available
        /// </summary>
        public bool IsGermanAvailable { get; set; } = true;

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

        // Sound

        public bool IsMusicEnabled
        {
            get => _isMusicEnabled;
            set
            {
                _isMusicEnabled = value;
                UnsavedChanges = true;
            }
        }

        public bool IsStero
        {
            get => _isStero;
            set
            {
                _isStero = value;
                UnsavedChanges = true;
            }
        }

        public int SoundVolume
        {
            get => _soundVolume;
            set
            {
                _soundVolume = value;
                UnsavedChanges = true;
            }
        }

        public bool IsVoicesVolumeAvailable { get; }

        public int VoicesVolume
        {
            get => _voicesVolume;
            set
            {
                _voicesVolume = value;
                UnsavedChanges = true;
            }
        }

        // Graphics

        public bool ShowBackground
        {
            get => _showBackground;
            set
            {
                _showBackground = value;
                UnsavedChanges = true;
            }
        }

        public bool ShowParallaxBackground
        {
            get => _showParallaxBackground;
            set
            {
                _showParallaxBackground = value;
                UnsavedChanges = true;
            }
        }

        public bool ShowHUD
        {
            get => _showHud;
            set
            {
                _showHud = value;
                UnsavedChanges = true;
            }
        }

        public Rayman1Freq[] FrameRateOptions_Values { get; }
        public string[] FrameRateOptions_Names { get; }

        public int SelectedFrameRateOption
        {
            get => _selectedFrameRateOption;
            set
            {
                _selectedFrameRateOption = value;
                UnsavedChanges = true;
            }
        }

        public Rayman1Freq FrameRate
        {
            get => FrameRateOptions_Values[SelectedFrameRateOption];
            set => SelectedFrameRateOption = FrameRateOptions_Values.FindItemIndex(x => x == value);
        }

        public int ZoneOfPlay
        {
            get => _zoneOfPlay;
            set
            {
                _zoneOfPlay = value;
                UnsavedChanges = true;
            }
        }

        // Controls

        /// <summary>
        /// The key items
        /// </summary>
        public ObservableCollection<ButtonMappingKeyItemViewModel> KeyItems { get; }

        public int GamePad_Jump
        {
            get => _gamePadJump;
            set
            {
                _gamePadJump = value;
                UnsavedChanges = true;
            }
        }

        public int GamePad_Fist
        {
            get => _gamePadFist;
            set
            {
                _gamePadFist = value;
                UnsavedChanges = true;
            }
        }

        public int GamePad_Action
        {
            get => _gamePadAction;
            set
            {
                _gamePadAction = value;
                UnsavedChanges = true;
            }
        }

        public int XPadMax
        {
            get => _xPadMax;
            set
            {
                _xPadMax = value;
                UnsavedChanges = true;
            }
        }

        public int XPadMin
        {
            get => _xPadMin;
            set
            {
                _xPadMin = value;
                UnsavedChanges = true;
            }
        }

        public int YPadMax
        {
            get => _yPadMax;
            set
            {
                _yPadMax = value;
                UnsavedChanges = true;
            }
        }

        public int YPadMin
        {
            get => _yPadMin;
            set
            {
                _yPadMin = value;
                UnsavedChanges = true;
            }
        }

        public int XPadCenter
        {
            get => _xPadCenter;
            set
            {
                _xPadCenter = value;
                UnsavedChanges = true;
            }
        }

        public int YPadCenter
        {
            get => _yPadCenter;
            set
            {
                _yPadCenter = value;
                UnsavedChanges = true;
            }
        }

        // Device

        public int Port
        {
            get => _port;
            set
            {
                _port = value;
                UnsavedChanges = true;
            }
        }

        public int IRQ
        {
            get => _irq;
            set
            {
                _irq = value;
                UnsavedChanges = true;
            }
        }

        public int DMA
        {
            get => _dma;
            set
            {
                _dma = value;
                UnsavedChanges = true;
            }
        }

        public int Param
        {
            get => _param;
            set
            {
                _param = value;
                UnsavedChanges = true;
            }
        }

        public int DeviceID
        {
            get => _deviceId;
            set
            {
                _deviceId = value;
                UnsavedChanges = true;
            }
        }

        public int NumCard
        {
            get => _numCard;
            set
            {
                _numCard = value;
                UnsavedChanges = true;
            }
        }

        #endregion

        #region Protected Methods

        protected override object GetPageUI() => new Ray_1_KIT_EDU_Config()
        {
            DataContext = this
        };

        /// <summary>
        /// Loads and sets up the current configuration properties
        /// </summary>
        /// <returns>The task</returns>
        protected override Task LoadAsync()
        {
            RL.Logger?.LogInformationSource($"{Game} config is being set up");

            ConfigFilePath = GetConfigPath();

            if (ConfigFilePath.FileExists)
            {
                // If a config file exists we read it
                Config = BinarySerializableHelpers.ReadFromFile<Rayman1PCConfigData>(ConfigFilePath, Ray1Settings.GetDefaultSettings(Ray1Game, Platform.PC), App.GetBinarySerializerLogger(ConfigFilePath.Name));

                // Default all languages to be available. Sadly there is no way to determine which languages a specific release can use as most releases have all languages in the files, but have it hard-coded in the exe to only pick a specific one.
                IsEnglishAvailable = true;
                IsFrenchAvailable = true;
                IsGermanAvailable = true;
            }
            else
            {
                // If no config file exists we create the config manually
                Config = CreateDefaultConfig();
            }

            // Default game language to not be available
            IsGameLanguageAvailable = false;

            if (LangMode == LanguageMode.Config)
            {
                // Get the language from the config file
                GameLanguage = Config.Language;
                IsGameLanguageAvailable = true;
            }
            else if (LangMode == LanguageMode.Argument)
            {
                // Get the game install directory
                var installDir = Game.GetInstallDir();

                // Attempt to get the game language from the .bat file
                var batchFile = installDir + Game.GetGameInfo().DefaultFileName;

                if (batchFile.FullPath.EndsWith(".bat", StringComparison.InvariantCultureIgnoreCase) && batchFile.FileExists)
                {
                    // Check language availability
                    var pcmapDir = installDir + "pcmap";

                    // IDEA: Read from VERSION file instead
                    IsEnglishAvailable = (pcmapDir + "usa").DirectoryExists;
                    IsFrenchAvailable = (pcmapDir + "fr").DirectoryExists;
                    IsGermanAvailable = (pcmapDir + "al").DirectoryExists;

                    // Make sure at least one language is available
                    if (IsEnglishAvailable || IsFrenchAvailable || IsGermanAvailable)
                    {
                        var lang = GetBatchFileanguage(batchFile);

                        if (lang != null)
                        {
                            GameLanguage = lang.Value;
                            IsGameLanguageAvailable = true;
                        }
                    }
                }
            }

            // Read button mapping
            for (int i = 0; i < KeyItems.Count; i++)
            {
                var item = KeyItems[i];
                var key = Config.Tab_Key[i];

                item.SetInitialNewKey(DirectXKeyHelpers.GetKey(key));
            }

            // Read config values
            IsMusicEnabled = Config.MusicCdActive != 0;
            IsStero = Config.IsStero != 0;
            SoundVolume = Config.VolumeSound;
            VoicesVolume = Config.EDU_VoiceSound;

            ShowBackground = Config.BackgroundOptionOn;
            ShowParallaxBackground = Config.ScrollDiffOn;
            ShowHUD = Config.FixOn;
            FrameRate = Config.Frequence;
            ZoneOfPlay = Config.SizeScreen;

            GamePad_Jump = Config.KeyJump;
            GamePad_Fist = Config.KeyWeapon;
            GamePad_Action = Config.KeyAction;
            XPadMax = Config.XPadMax;
            XPadMin = Config.XPadMin;
            YPadMax = Config.YPadMax;
            YPadMin = Config.YPadMin;
            XPadCenter = Config.XPadCentre;
            YPadCenter = Config.YPadCentre;

            Port = (int)Config.Port;
            IRQ = (int)Config.Irq;
            DMA = (int)Config.Dma;
            Param = (int)Config.Param;
            DeviceID = (int)Config.DeviceID;
            NumCard = Config.NumCard;

            UnsavedChanges = false;

            // Verify values. If these values are incorrect they will cause the game to run without sound effects with the default DOSBox configuration.
            if (Port != 544 || IRQ != 5 || DMA != 5 || DeviceID != 57368 || NumCard != 3)
            {
                Port = 544;
                IRQ = 5;
                DMA = 5;
                DeviceID = 57368;
                NumCard = 3;
                UnsavedChanges = true;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Saves the changes
        /// </summary>
        /// <returns>The task</returns>
        protected override async Task<bool> SaveAsync()
        {
            RL.Logger?.LogInformationSource($"{Game} config is saving...");

            try
            {
                // If game language is available, update it
                if (IsGameLanguageAvailable)
                {
                    if (LangMode == LanguageMode.Config)
                        Config.Language = GameLanguage;
                    else if (LangMode == LanguageMode.Argument)
                        await SetBatchFileLanguageAsync(Game.GetInstallDir() + Game.GetGameInfo().DefaultFileName, GameLanguage, Game);
                }

                // Set button mapping
                for (int i = 0; i < KeyItems.Count; i++)
                {
                    var item = KeyItems[i];

                    Config.Tab_Key[i] = (byte)DirectXKeyHelpers.GetKeyCode(item.NewKey);
                }

                // Set config values
                Config.MusicCdActive = (ushort)(IsMusicEnabled ? 1 : 0);
                Config.IsStero = (ushort)(IsStero ? 1 : 0);
                Config.VolumeSound = (ushort)SoundVolume;
                Config.EDU_VoiceSound = (ushort)VoicesVolume;

                Config.BackgroundOptionOn = ShowBackground;
                Config.ScrollDiffOn = ShowParallaxBackground;
                Config.FixOn = ShowHUD;
                Config.Frequence = FrameRate;
                Config.SizeScreen = (byte)ZoneOfPlay;

                Config.KeyJump = (ushort)GamePad_Jump;
                Config.KeyWeapon = (ushort)GamePad_Fist;
                Config.KeyAction = (ushort)GamePad_Action;
                Config.XPadMax = (short)XPadMax;
                Config.XPadMin = (short)XPadMin;
                Config.YPadMax = (short)YPadMax;
                Config.YPadMin = (short)YPadMin;
                Config.XPadCentre = (short)XPadCenter;
                Config.YPadCentre = (short)YPadCenter;

                Config.Port = (uint)Port;
                Config.Irq = (uint)IRQ;
                Config.Dma = (uint)DMA;
                Config.Param = (uint)Param;
                Config.DeviceID = (uint)DeviceID;
                Config.NumCard = (byte)NumCard;

                // Save the config file
                BinarySerializableHelpers.WriteToFile(Config, ConfigFilePath, Ray1Settings.GetDefaultSettings(Ray1Game, Platform.PC), App.GetBinarySerializerLogger(ConfigFilePath.Name));

                return true;
            }
            catch (Exception ex)
            {
                ex.HandleError($"Saving {Game} configuration data");

                await Services.MessageUI.DisplayExceptionMessageAsync(ex, String.Format(Resources.Config_SaveError, Game.GetGameInfo().DisplayName), Resources.Config_SaveErrorHeader);
                return false;
            }
        }

        protected override void UseRecommended()
        {
            IsMusicEnabled = true;
            IsStero = true;
            ShowBackground = true;
            ShowParallaxBackground = true;
            FrameRate = Rayman1Freq.Freq_60;
            ZoneOfPlay = 0;
            Port = 544;
            IRQ = 5;
            DMA = 5;
            DeviceID = 57368;
            NumCard = 3;
        }

        #endregion

        #region Public Methods

        public abstract FileSystemPath GetConfigPath();

        public override void Dispose()
        {
            // Dispose base
            base.Dispose();

            KeyItems?.DisposeAll();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Creates a new instance of <see cref="Rayman1PCConfigData"/> with default values for the specific game
        /// </summary>
        /// <returns>The config instance</returns>
        protected Rayman1PCConfigData CreateDefaultConfig()
        {
            return new Rayman1PCConfigData
            {
                Language = R1Languages.English,
                Port = 544,
                Irq = 5,
                Dma = 5,
                Param = 0,
                DeviceID = 57368,
                NumCard = 3,
                KeyJump = 1,
                KeyWeapon = 0,
                Options_jeu_10 = 3,
                KeyAction = 2,
                MusicCdActive = 1,
                VolumeSound = 18,
                IsStero = 1,
                EDU_VoiceSound = 18,
                Mode_Pad = false,
                Port_Pad = 0,
                XPadMax = 1610,
                XPadMin = 35,
                YPadMax = 1610,
                YPadMin = 35,
                XPadCentre = 830,
                YPadCentre = 830,
                NotBut = new byte[]
                {
                    0x00, 0x00, 0x00, 0x00
                },
                Tab_Key = new byte[]
                {
                    0x4B, 0x48, 0x4D, 0x50, 0x1D, 0x38, 0x2D
                },
                GameModeVideo = 0,
                P486 = 0,
                SizeScreen = 0,
                Frequence = 0,
                FixOn = true,
                BackgroundOptionOn = true,
                ScrollDiffOn = true,
                RefRam2VramNormalFix = new ushort[8],
                RefRam2VramNormal = new ushort[8],
                RefTransFondNormal = new ushort[8],
                RefSpriteNormal = new ushort[2],
                RefRam2VramX = new ushort[2],
                RefVram2VramX = new ushort[2],
                RefSpriteX = new ushort[2]
            };
        }

        /// <summary>
        /// Gets the current game language from the specified batch file
        /// </summary>
        /// <param name="batchFile">The batch file to get the language from</param>
        /// <returns>The language or null if none was found</returns>
        protected R1Languages? GetBatchFileanguage(FileSystemPath batchFile)
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
                ex.HandleError($"Getting {Game} language from batch file");
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
        protected async Task SetBatchFileLanguageAsync(FileSystemPath batchFile, R1Languages language, Games game)
        {
            try
            {
                var lang = language switch
                {
                    R1Languages.English => "usa",
                    R1Languages.French => "fr",
                    R1Languages.German => "al",
                    _ => throw new ArgumentOutOfRangeException(nameof(language), language, null)
                };

                // Delete the existing file
                RCPServices.File.DeleteFile(batchFile);

                // Create the .bat file
                File.WriteAllLines(batchFile, new string[]
                {
                    "@echo off",
                    $"{Path.GetFileNameWithoutExtension(game.GetManager<RCPDOSBoxGame>().ExecutableName)} ver={lang}"
                });
            }
            catch (Exception ex)
            {
                ex.HandleError($"Setting {Game} language from batch file");
                await Services.MessageUI.DisplayExceptionMessageAsync(ex, Resources.DosBoxConfig_SetLanguageError, Resources.DosBoxConfig_SetLanguageErrorHeader);
            }
        }

        #endregion

        #region Enums

        /// <summary>
        /// The available ways for Rayman 1 games to store the language setting
        /// </summary>
        public enum LanguageMode
        {
            /// <summary>
            /// The language is stored in the config file
            /// </summary>
            Config,

            /// <summary>
            /// The language is set from a launch argument
            /// </summary>
            Argument,

            /// <summary>
            /// The game does not allow custom languages
            /// </summary>
            None
        }

        #endregion
    }
}