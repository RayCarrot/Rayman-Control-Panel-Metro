﻿using System;
using System.Collections.Generic;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rayman 3 Demo 3 game info
    /// </summary>
    public sealed class Demo_Rayman3_3_Info : Demo_Rayman3_BaseInfo
    {
        #region Public Override Properties

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.Demo_Rayman3_3;

        /// <summary>
        /// The game display name
        /// </summary>
        public override string DisplayName => "Rayman 3 Demo 3";

        /// <summary>
        /// The game backup name
        /// </summary>
        public override string BackupName => "Rayman 3 Demo 3";

        /// <summary>
        /// The download URLs for the game if it can be downloaded. All sources must be compressed.
        /// </summary>
        public override IList<Uri> DownloadURLs => new Uri[]
        {
            new Uri(CommonUrls.Games_R3Demo3_Url),
        };

        #endregion
    }
}