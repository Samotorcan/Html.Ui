﻿using Samotorcan.HtmlUi.Core.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samotorcan.HtmlUi.Core.Utilities
{
    /// <summary>
    /// Resource utility.
    /// </summary>
    internal static class ResourceUtility
    {
        #region Methods
        #region Public

        #region ResourceExists
        /// <summary>
        /// Resource exists.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static bool ResourceExists(string name)
        {
            Argument.NullOrEmpty(name, "name");

            name = ResourceUtility.GetFullResourceName(name);
            var assembly = typeof(ResourceUtility).Assembly;

            return assembly.GetManifestResourceNames().Contains(name);
        }
        #endregion
        #region GetResourceAsString
        /// <summary>
        /// Gets the resource as string.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">name</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "It's ok.")]
        public static string GetResourceAsString(string name)
        {
            Argument.NullOrEmpty(name, "name");

            name = ResourceUtility.GetFullResourceName(name);
            var assembly = typeof(ResourceUtility).Assembly;

            using (var stream = assembly.GetManifestResourceStream(name))
            {
                Argument.InvalidArgument(stream == null, "Resource not found.", "name");

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        #endregion
        #region GetResourceAsBytes
        /// <summary>
        /// Gets the resource as bytes.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">name</exception>
        public static byte[] GetResourceAsBytes(string name)
        {
            Argument.NullOrEmpty(name, "name");

            name = ResourceUtility.GetFullResourceName(name);
            var assembly = typeof(ResourceUtility).Assembly;

            using (var stream = assembly.GetManifestResourceStream(name))
            {
                Argument.InvalidArgument(stream == null, "Resource not found.", "name");

                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
        #endregion

        #endregion
        #region Private

        #region GetFullResourceName
        /// <summary>
        /// Gets the full name of the resource.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private static string GetFullResourceName(string name)
        {
            return "Samotorcan.HtmlUi.Core.Resources." + name.Replace('/', '.').TrimStart('.');
        }
        #endregion

        #endregion
        #endregion
    }
}