﻿using System;

namespace Samotorcan.HtmlUi.Core.Utilities
{
    /// <summary>
    /// String utility.
    /// </summary>
    internal static class StringUtility
    {
        #region Methods
        #region Public

        #region Normalize
        /// <summary>
        /// Normalizes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="normalizeType">The normalize type.</param>
        /// <returns></returns>
        public static string Normalize(string value, NormalizeType normalizeType)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            if (normalizeType == NormalizeType.CamelCase)
                return Char.ToLowerInvariant(value[0]) + value.Substring(1);
            else if (normalizeType == NormalizeType.PascalCase)
                return Char.ToUpperInvariant(value[0]) + value.Substring(1);

            return value;
        }
        #endregion
        #region CamelCase
        /// <summary>
        /// Camel case.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string CamelCase(string value)
        {
            return StringUtility.Normalize(value, NormalizeType.CamelCase);
        }
        #endregion
        #region PascalCase
        /// <summary>
        /// Pascal case.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string PascalCase(string value)
        {
            return StringUtility.Normalize(value, NormalizeType.PascalCase);
        }
        #endregion

        #endregion
        #endregion
    }
}
