﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro;
using Nito.AsyncEx;
using RayCarrot.CarrotFramework;

namespace RayCarrot.RCP.Metro
{
    /// <summary>
    /// Manages secret codes for this application
    /// </summary>
    public static class SecretCodeManager
    {
        #region Static Constructor

        static SecretCodeManager()
        {
            AsyncLock = new AsyncLock();

            Codes = new Dictionary<Key[], Func<Task>>()
            {
                {
                    // Konami code
                    new Key[]
                    {
                        Key.Up,
                        Key.Up,
                        Key.Down,
                        Key.Down,
                        Key.Left,
                        Key.Right,
                        Key.Left,
                        Key.Right,
                        Key.B,
                        Key.A
                    },
                    async () =>
                    {
                        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("Mauve"), ThemeManager.GetAppTheme("BaseDark"));

                        await RCF.MessageUI.DisplayMessageAsync(Resources.SecretCodes_Konami, Resources.SecretCodes_KonamiHeader, MessageType.Success);
                    }
                },
                {
                    // RayCarrot code
                    new Key[]
                    {
                        Key.R,
                        Key.A,
                        Key.Y,
                        Key.C,
                        Key.A,
                        Key.R,
                        Key.R,
                        Key.O,
                        Key.T,
                    },
                    async () =>
                    {
                        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("Orange"), ThemeManager.GetAppTheme("BaseDark"));

                        await RCF.MessageUI.DisplayMessageAsync(Resources.SecretCodes_RayCarrot, Resources.SecretCodes_RayCarrotHeader, MessageType.Success);
                    }
                },
                {
                    // Lime code
                    new Key[]
                    {
                        Key.S,
                        Key.O,
                        Key.U,
                        Key.R,
                    },
                    async () =>
                    {
                        ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("Lime"), ThemeManager.GetAppTheme("BaseDark"));

                        await RCF.MessageUI.DisplayMessageAsync(Resources.SecretCodes_Lime, Resources.SecretCodes_LimeHeader, MessageType.Success);
                    }
                },
                {
                    // Secret code
                    new Key[]
                    {
                        Key.S,
                        Key.E,
                        Key.C,
                        Key.R,
                        Key.E,
                        Key.T,
                    },
                    async () =>
                    {
                        await RCF.MessageUI.DisplayMessageAsync(Resources.SecretCodes_Secret, Resources.SecretCodes_SecretHeader, MessageType.Success);
                    }
                },
            };

            CurrentInput = new List<Key>(Codes.OrderBy(x => x.Key.Length).First().Key.Length);
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Sets up the manager
        /// </summary>
        public static void Setup()
        {
            Application.Current.MainWindow.PreviewKeyDown += async (s, e) => await AddKeyAsync(e.Key);
        }

        /// <summary>
        /// Add a new key to the manager
        /// </summary>
        /// <param name="key">The key to add</param>
        /// <returns>The task</returns>
        public static async Task AddKeyAsync(Key key)
        {
            // Lock the method
            using (await AsyncLock.LockAsync())
            {
                // Check if it's the following key in any of the available codes
                if (Codes.All(x => x.Key[CurrentInput.Count] != key))
                {
                    CurrentInput.Clear();
                    RCF.Logger.LogDebugSource("The secret code inputs were reset due to an invalid key being pressed");
                    return;
                }

                // Add the key to the list
                CurrentInput.Add(key);

                // Attempt to get a completed code
                var task = Codes.FindItem(x => x.Key.SequenceEqual(CurrentInput)).Value;

                if (task == null)
                    return;

                CurrentInput.Clear();
                RCF.Logger.LogDebugSource("The secret code inputs were reset due to a valid code having been entered");

                // Run the task
                await task();
            }
        }

        #endregion

        #region Private Static Properties

        /// <summary>
        /// The async lock for adding a new key
        /// </summary>
        private static AsyncLock AsyncLock { get; }

        /// <summary>
        /// The currently saved input
        /// </summary>
        private static List<Key> CurrentInput { get; }

        /// <summary>
        /// The available codes and their tasks
        /// </summary>
        private static Dictionary<Key[], Func<Task>> Codes { get; }

        #endregion
    }
}