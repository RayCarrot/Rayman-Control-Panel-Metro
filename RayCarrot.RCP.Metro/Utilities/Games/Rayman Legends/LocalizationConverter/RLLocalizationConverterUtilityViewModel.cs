﻿using RayCarrot.IO;
using RayCarrot.Rayman.UbiArt;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// View model for the Rayman Legends localization converter utility
    /// </summary>
    public class RLLocalizationConverterUtilityViewModel : BaseUbiArtLocalizationConverterUtilityViewModel<SerializableDictionary<int, SerializableDictionary<int, string>>>
    {
        /// <summary>
        /// The default localization directory for the game, if available
        /// </summary>
        protected override FileSystemPath? DefaultLocalizationDirectory => Games.RaymanLegends.GetInstallDir() + "EngineData" + "Localisation";

        /// <summary>
        /// The localization file extension
        /// </summary>
        protected override FileExtension LocalizationFileExtension => new FileExtension(".loc8");

        /// <summary>
        /// Deserializes the localization file
        /// </summary>
        /// <param name="file">The localization file</param>
        /// <returns>The data</returns>
        protected override SerializableDictionary<int, SerializableDictionary<int, string>> Deserialize(FileSystemPath file)
        {
            return UbiArtLocalizationData.GetSerializer(UbiArtGameMode.RaymanLegendsPC.GetSettings()).Deserialize(file).Strings;
        }

        /// <summary>
        /// Serializes the data to the localization file
        /// </summary>
        /// <param name="file">The localization file</param>
        /// <param name="data">The data</param>
        protected override void Serialize(FileSystemPath file, SerializableDictionary<int, SerializableDictionary<int, string>> data)
        {
            // Get the serializer
            var serializer = UbiArtLocalizationData.GetSerializer(UbiArtGameMode.RaymanLegendsPC.GetSettings());

            // Read the current data to get the remaining bytes
            var currentData = serializer.Deserialize(file);

            // Replace the string data
            currentData.Strings = data;

            // Serialize the data
            serializer.Serialize(file, currentData);
        }
    }
}