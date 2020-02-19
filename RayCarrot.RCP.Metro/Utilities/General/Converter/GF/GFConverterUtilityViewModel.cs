﻿using RayCarrot.IO;
using RayCarrot.Rayman.OpenSpace;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Utility view model for converting .gf files
    /// </summary>
    public class GFConverterUtilityViewModel : BaseConverterUtilityViewModel<OpenSpaceGameMode>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public GFConverterUtilityViewModel()
        {
            GameModeSelection = new EnumSelectionViewModel<OpenSpaceGameMode>(OpenSpaceGameMode.Rayman2PC, new OpenSpaceGameMode[]
            {
                OpenSpaceGameMode.Rayman2PC,
                OpenSpaceGameMode.Rayman2IOS,
                OpenSpaceGameMode.Rayman2PCDemo1,
                OpenSpaceGameMode.Rayman2PCDemo2,
                OpenSpaceGameMode.RaymanMPC,
                OpenSpaceGameMode.RaymanArenaPC,
                OpenSpaceGameMode.Rayman3PC,
                OpenSpaceGameMode.TonicTroublePC,
                OpenSpaceGameMode.TonicTroubleSEPC,
                OpenSpaceGameMode.DonaldDuckPC,
                OpenSpaceGameMode.PlaymobilHypePC,
                OpenSpaceGameMode.PlaymobilLauraPC,
                OpenSpaceGameMode.PlaymobilAlexPC,
                OpenSpaceGameMode.DinosaurPC,
                OpenSpaceGameMode.LargoWinchPC
            });
        }

        #endregion

        #region Public Override Properties

        /// <summary>
        /// The game mode selection
        /// </summary>
        public override EnumSelectionViewModel<OpenSpaceGameMode> GameModeSelection { get; }

        #endregion

        #region Public Override Methods

        /// <summary>
        /// Converts from the format
        /// </summary>
        /// <returns>The task</returns>
        public override async Task ConvertFromAsync()
        {
            await ConvertFromAsync(OpenSpaceGFFile.GetSerializer(GameModeSelection.SelectedValue.GetSettings()), (data, filePath, configPath) =>
            {
                // Get a bitmap from the image data
                using var bmp = data.GetBitmap();

                // Save the image
                bmp.Save(filePath, ImageHelpers.GetImageFormat(filePath.FileExtension));

                // Create the config file
                var config = new GFConfigData(data.Channels, data.Format, data.MipmapCount, data.RepeatByte, true);

                // Save the config file
                SerializeJSON(config, configPath);
            }, new FileFilterItem("*.gf", "GF").ToString(), ImageHelpers.GetSupportedBitmapExtensions(), null);
        }

        /// <summary>
        /// Converts to the format
        /// </summary>
        /// <returns>The task</returns>
        public override async Task ConvertToAsync()
        {
            var settings = GameModeSelection.SelectedValue.GetSettings();

            await ConvertToAsync(OpenSpaceGFFile.GetSerializer(settings), (filePath, configPath) =>
            {
                // Read the config file
                var config = DeserializeJSON<GFConfigData>(configPath);

                // Create the GF data
                var data = new OpenSpaceGFFile()
                {
                    Channels = config.Channels,
                    Format = config.Format,
                    MipmapCount = config.MipmapCount,
                };

                // Read the image
                using var bmp = new Bitmap(filePath);

                // Load it into the GF data
                data.ImportFromBitmap(bmp);

                // Set repeat byte if not auto generated
                if (!config.AutoGenerateRepeatByte)
                    data.RepeatByte = config.RepeatByte;

                // Return the data
                return data;
            }, new FileFilterItemCollection(ImageHelpers.GetSupportedBitmapExtensions().Select(x => new FileFilterItem($"*{x}",
                x.Substring(1).ToUpper()))).ToString(), new FileExtension(".gf"), true);
        }

        #endregion

        #region Classes

        /// <summary>
        /// Configuration data for a GF file
        /// </summary>
        protected class GFConfigData
        {
            public GFConfigData(byte channels, uint format, byte mipmapCount, byte repeatByte, bool autoGenerateRepeatByte)
            {
                Channels = channels;
                Format = format;
                MipmapCount = mipmapCount;
                RepeatByte = repeatByte;
                AutoGenerateRepeatByte = autoGenerateRepeatByte;
            }

            public byte Channels { get; }

            public uint Format { get; }

            public byte MipmapCount { get; }

            public byte RepeatByte { get; }

            public bool AutoGenerateRepeatByte { get; }
        }

        #endregion
    }
}