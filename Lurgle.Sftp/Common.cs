﻿using System;
using System.Reflection;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Lurgle.Sftp
{
    /// <summary>
    ///     Common functions
    /// </summary>
    public static class Common
    {
        static Common()
        {
            var isSuccess = true;
            try
            {
                AppName = Assembly.GetEntryAssembly()?.GetName().Name;
                AppVersion = Assembly.GetEntryAssembly()?.GetName().Version.ToString();
            }
            catch
            {
                isSuccess = false;
            }

            if (isSuccess) return;
            try
            {
                AppName = Assembly.GetExecutingAssembly().GetName().Name;
                AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            catch
            {
                //We surrender ...
                AppName = string.Empty;
                AppVersion = string.Empty;
            }
        }

        /// <summary>
        ///     App name
        /// </summary>
        public static string AppName { get; set; }

        /// <summary>
        ///     App version
        /// </summary>
        public static string AppVersion { get; }

        /// <summary>
        ///     Convert the supplied <see cref="object" /> to a <see cref="bool" />
        ///     <para />
        ///     This will filter out nulls that could otherwise cause exceptions
        /// </summary>
        /// <param name="sourceObject">An object that can be converted to a bool</param>
        /// <returns></returns>
        public static bool GetBool(object sourceObject)
        {
            var sourceString = string.Empty;

            if (!Convert.IsDBNull(sourceObject)) sourceString = (string) sourceObject;

            return bool.TryParse(sourceString, out var destBool) && destBool;
        }

        /// <summary>
        ///     Convert the supplied <see cref="object" /> to an <see cref="int" />
        ///     <para />
        ///     This will filter out nulls that could otherwise cause exceptions
        /// </summary>
        /// <param name="sourceObject">An object that can be converted to an int</param>
        /// <returns></returns>
        public static int GetInt(object sourceObject)
        {
            var sourceString = string.Empty;

            if (!Convert.IsDBNull(sourceObject)) sourceString = (string) sourceObject;

            if (int.TryParse(sourceString, out var destInt)) return destInt;

            return -1;
        }
    }
}