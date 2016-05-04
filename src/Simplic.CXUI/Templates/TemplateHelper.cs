﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplic.CXUI
{
    /// <summary>
    /// Helper to work with templates
    /// </summary>
    public static class TemplateHelper
    {
        /// <summary>
        /// Path to the viewmodel template
        /// </summary>
        public const string VIEWMODEL_TEMPLATE = "Simplic.CXUI.ViewModelTemplate.cstemplate";

        /// <summary>
        /// Get a filled template.
        /// </summary>
        /// <param name="name">Name of the template (use the constant strings to access them)</param>
        /// <param name="values">Values which will be replaced within the template</param>
        /// <returns>Temaplte as string</returns>
        public static string GetTemplate(string name, IDictionary<string, string> values)
        {
            var assembly = typeof(TemplateHelper).Assembly;

            using (Stream stream = assembly.GetManifestResourceStream(name))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();

                    foreach (var val in values)
                    {
                        result = result.Replace("{" + val.Key + "}", val.Value);
                    }
                }
            }

            return null;
        }
    }
}