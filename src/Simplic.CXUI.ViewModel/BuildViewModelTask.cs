﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using Simplic.CXUI.BuildTask;

namespace Simplic.CXUI.ViewModel
{
    /// <summary>
    /// Generates the viewmodels for the generated and compiled xaml files
    /// </summary>
    public abstract class BuildViewModelTask : BuildTaskBase
    {
        #region Fields
        private IList<MetaViewModel> viewModels;
        private MetaBaseViewModel defaultBaseViewModel;
        private IList<string> viewModelFiles;
        #endregion

        #region Constructor
        /// <summary>
        /// Create generator
        /// </summary>
        public BuildViewModelTask() : base()
        {
            // Default values
            defaultBaseViewModel = new MetaBaseViewModel();
            viewModels = new List<MetaViewModel>();
            viewModelFiles = new List<string>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Generate a metaviewmodel by passing it's code
        /// </summary>
        /// <param name="code">Code of the meta viewmodel in the specific language (json, c#, python, ...)</param>
        /// <returns>Throws an exception or returns the viewmodel meta information</returns>
        public abstract MetaViewModel GenerateMetaViewModel(string code);

        /// <summary>
        /// Generate viewmodel and save in output directory
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            viewModels.Clear();

            foreach (var file in viewModelFiles)
            {
                var model = GenerateMetaViewModel(System.IO.File.ReadAllText(file));

                model.__AbsolutePath__ = Path.GetDirectoryName(file);
                model.__RelativePath__ = Path.GetDirectoryName(model.__AbsolutePath__.Replace(CXUIBuildEngine.ProjectRoot, "") + "\\");

                viewModels.Add(model);
            }

            foreach (var model in viewModels)
            {
                string tempOutputPath = Path.Combine(TempOutputDirectory, model.__RelativePath__, model.Name + ".cs");
                Console.WriteLine("Generate: " + tempOutputPath);

                // List of values which will be replaced in the file
                var values = new Dictionary<string, string>();

                // Get default base viewmodel
                var baseViewModel = model.BaseViewModel ?? defaultBaseViewModel;

                StringBuilder fields = new StringBuilder();
                StringBuilder properties = new StringBuilder();
                StringBuilder constructor = new StringBuilder();

                values.Add("Namespace", model.Namespace);
                values.Add("ViewModelName", model.Name);
                values.Add("BaseViewModel", model.FullQualifiedBaseViewModel);

                // Generate fields and properties
                foreach (var property in model.Properties)
                {
                    // Field
                    string field = string.Format("_{0}", property.Name.Trim());
                    fields.AppendLine(TemplateHelper.GetTemplate("Simplic.CXUI.ViewModel.Templates.ViewModelField.cstemplate", new Dictionary<string, string> { { "Type", property.Type }, { "Name", field } }, typeof(BuildViewModelTask).Assembly));


                    Dictionary<string, string> propertyTemplateFields = new Dictionary<string, string>();
                    propertyTemplateFields.Add("Field", field);
                    propertyTemplateFields.Add("Name", property.Name);
                    propertyTemplateFields.Add("Type", property.Type);

                    propertyTemplateFields.Add("SetIsDirty", "");
                    propertyTemplateFields.Add("SetForceSave", "");
                    propertyTemplateFields.Add("RaisePropertyChanged", "");

                    // Is dirty and force save
                    if (!string.IsNullOrWhiteSpace(baseViewModel.NameIsDirtyProperty) && property.SetIsDirty)
                    {
                        propertyTemplateFields["SetIsDirty"] = string.Format("{0} = true;", baseViewModel.NameIsDirtyProperty);
                    }
                    if (!string.IsNullOrWhiteSpace(baseViewModel.NameForceSaveProperty) && property.SetForceSave)
                    {
                        propertyTemplateFields["SetForceSave"] = string.Format("{0} = true;", baseViewModel.NameForceSaveProperty);
                    }

                    // Raise property changed
                    if (!string.IsNullOrWhiteSpace(baseViewModel.NameForceSaveProperty) && property.SetForceSave)
                    {
                        propertyTemplateFields["RaisePropertyChanged"] = string.Format("{0}(\"{1}\");", baseViewModel.RaisePropertyChangedMethod, property.Name);
                    }

                    properties.AppendLine(TemplateHelper.GetTemplate("Simplic.CXUI.ViewModel.Templates.ViewModelProperty.cstemplate", propertyTemplateFields, typeof(BuildViewModelTask).Assembly));
                    properties.AppendLine();
                }

                // Set
                values.Add("Fields", fields.ToString().TrimEnd());
                values.Add("ConstructorCode", constructor.ToString().TrimEnd());
                values.Add("Properties", properties.ToString().TrimEnd());

                // Generate file
                string template = TemplateHelper.GetTemplate("Simplic.CXUI.ViewModel.Templates.ViewModel.cstemplate", values, typeof(BuildViewModelTask).Assembly);

                if (!Directory.Exists(Path.GetDirectoryName(tempOutputPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(tempOutputPath));
                }

                System.IO.File.WriteAllText(tempOutputPath, template, Encoding.UTF8);

                CXUIBuildEngine.GeneratedFiles.Add(new GeneratedFile(tempOutputPath));
            }

            return base.Execute();
        }
        #endregion

        #region Public Member
        /// <summary>
        /// List of all viewmodels to generate
        /// </summary>
        public IList<MetaViewModel> ViewModels
        {
            get
            {
                return viewModels;
            }

        }

        /// <summary>
        /// Get or set the default viewmodel base, which will be used if no MetaBaseViewModel is set
        /// </summary>
        public MetaBaseViewModel DefaultBaseViewModel
        {
            get
            {
                return defaultBaseViewModel;
            }

            set
            {
                defaultBaseViewModel = value;
            }
        }

        /// <summary>
        /// List of viewmodel files to compile
        /// </summary>
        public IList<string> ViewModelFiles
        {
            get
            {
                return viewModelFiles;
            }
        }
        #endregion
    }
}
