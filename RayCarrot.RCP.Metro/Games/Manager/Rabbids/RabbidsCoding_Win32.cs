﻿using System.Collections.Generic;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// The Rabbids Coding (Win32) game manager
    /// </summary>
    public sealed class RabbidsCoding_Win32 : RCPWin32Game
    {
        #region Public Overrides

        /// <summary>
        /// The game
        /// </summary>
        public override Games Game => Games.RabbidsCoding;

        /// <summary>
        /// Gets the game finder item for this game
        /// </summary>
        public override GameFinderItem GameFinderItem => new GameFinderItem(null, "Rabbids Coding", new string[]
        {
            "RabbidsCoding",
            "Rabbids Coding",
        });

        /// <summary>
        /// Gets the purchase links for the game
        /// </summary>
        public override IList<GamePurchaseLink> GetGamePurchaseLinks => new GamePurchaseLink[]
        {
            new GamePurchaseLink(Resources.GameDisplay_DownloadUplay, "https://register.ubisoft.com/rabbids-coding/")
        };

        #endregion
    }
}