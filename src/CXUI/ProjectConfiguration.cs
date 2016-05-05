﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CXUI
{
    /// <summary>
    /// Project file configuration
    /// </summary>
    public class ProjectConfiguration
    {
        /// <summary>
        /// Create configuration
        /// </summary>
        public ProjectConfiguration()
        {
            Output = ".";
            Xaml = new List<string>();
            ViewModels = new List<string>();
            Ressources = new List<string>();
        }

        /// <summary>
        /// Assembly name
        /// </summary>
        public string Assembly
        {
            get;
            set;
        }

        /// <summary>
        /// Root namespace of the assembly
        /// </summary>
        public string RootNamespace
        {
            get;
            set;
        }

        /// <summary>
        /// Output path of the assembly, default .
        /// </summary>
        public string Output
        {
            get;
            set;
        }

        /// <summary>
        /// List of UI elements
        /// </summary>
        public IList<string> Xaml
        {
            get;
            set;
        }

        /// <summary>
        /// List of viewmodels
        /// </summary>
        public IList<string> ViewModels
        {
            get;
            set;
        }

        /// <summary>
        /// List of ressources
        /// </summary>
        public IList<string> Ressources
        {
            get;
            set;
        }
    }
}
