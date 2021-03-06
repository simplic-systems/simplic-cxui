﻿using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Simplic.CXUI.BuildTask
{
    /// <summary>
    /// Base class which must be implemented in all build steps
    /// </summary>
    public abstract class BuildTaskBase : ITask
    {
        /// <summary>
        /// Buildtask which will be used for creating a specific output
        /// </summary>
        public IBuildEngine BuildEngine
        {
            get;
            set;
        }

        /// <summary>
        /// CXUI Build engine
        /// </summary>
        public CXUIBuilder CXUIBuildEngine
        {
            get
            {
                return BuildEngine as CXUIBuilder;
            }
        }

        /// <summary>
        /// Build hosting object
        /// </summary>
        public ITaskHost HostObject
        {
            get;
            set;
        }

        /// <summary>
        /// Execute build step/task
        /// </summary>
        /// <returns>Returns true if building was successfull</returns>
        public virtual bool Execute()
        {
            return true;
        }

        /// <summary>
        /// Temporary output directory
        /// </summary>
        public virtual string TempOutputDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of assemblies that are required for the current build step
        /// </summary>
        public virtual IList<Assembly> Assemblies
        {
            get;
            set;
        } = new List<Assembly>();
    }
}
