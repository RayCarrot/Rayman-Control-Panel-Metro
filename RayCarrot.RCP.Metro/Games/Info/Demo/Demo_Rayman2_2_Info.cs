﻿using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 2 Demo 2 game info
    /// </summary>
    public sealed class Demo_Rayman2_2_Info : Demo_Rayman2_BaseInfo
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Demo_Rayman2_2;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman 2 Demo 2";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rayman 2 Demo 2";

        /// <summary>
        /// The download URLs for the game if it can be downloaded. All sources must be compressed.
        /// </summary>
        public override IList<Uri> DownloadURLs => new Uri[]
        {
            new Uri(CommonUrls.Games_R2Demo2_Url),
        };

        #endregion
    }
}