﻿using Microsoft.Build.Framework;
using Microsoft.Build.Tasks.Windows;
using Microsoft.Build.Tasks.Xaml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Simplic.CXUI.BuildTask
{
    /// <summary>
    /// Compile final assembly using Roslyn
    /// </summary>
    public class BuildAssemblyTask : BuildTaskBase
    {
        #region Fields
        private IList<string> sources;
        #endregion

        #region Constructor
        /// <summary>
        /// Create assembly compiler
        /// </summary>
        public BuildAssemblyTask() : base()
        {
            sources = new List<string>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Compile assembly
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            // Create syntax trees
            IList<SyntaxTree> syntaxTrees = new List<SyntaxTree>();

            foreach (var file in sources)
            {
                var _st = CSharpSyntaxTree.ParseText(File.ReadAllText(file));
                syntaxTrees.Add(_st);
            }

            Console.WriteLine("Add sources to compile");
            foreach (var gen in CXUIBuildEngine.GeneratedFiles.Where(item => item.Extension == ".cs"))
            {
                Console.WriteLine(gen.AbsolutePath);
                var _st = CSharpSyntaxTree.ParseText(File.ReadAllText(gen.AbsolutePath));
                syntaxTrees.Add(_st);
            }

            // Set references (Assemblies)
            List<MetadataReference> references = CXUIBuildEngine.References.Select(item => MetadataReference.CreateFromFile(item.Location)).ToList<MetadataReference>();

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (!CXUIBuildEngine.References.Contains(asm) && !string.IsNullOrWhiteSpace(asm.Location))
                    {

                        references.Add(MetadataReference.CreateFromFile(asm.Location));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Skip: assembly");
                }
            }

            // Set compiler options
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName: CXUIBuildEngine.AssemblyName,
                syntaxTrees: syntaxTrees.ToArray(),
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                // We also need to find and embedd some ressource
                List<ResourceDescription> resourceDescriptions = new List<ResourceDescription>();

                string resourcePath = string.Format("{0}{1}.g.resources", TempOutputDirectory, CXUIBuildEngine.RootNamespace);
                ResourceWriter rsWriter = new ResourceWriter(resourcePath);

                foreach (var generatedFile in CXUIBuildEngine.GeneratedFiles.Where(item => item.Extension == ".baml"))
                {
                    var fileName = string.Format("{0}{1}{2}{3}", generatedFile.RelativeDirectoryPath, "\\", generatedFile.Name, generatedFile.Extension);
                    fileName = fileName.ToLower();

                    if (fileName.StartsWith("\\"))
                    {
                        fileName = fileName.Substring(1, fileName.Length - 1);
                    }

                    var data = File.OpenRead(generatedFile.AbsolutePath);
                    rsWriter.AddResource(fileName.Replace("\\", "/"), data, true);
                }

                rsWriter.Generate();
                rsWriter.Close();

                // Add ressource under the namespace AND assembly
                var resourceDescription = new ResourceDescription(
                                string.Format("{0}.g.resources", CXUIBuildEngine.RootNamespace),
                                () => File.OpenRead(resourcePath),
                                true);
                resourceDescriptions.Add(resourceDescription);

                if (CXUIBuildEngine.RootNamespace != CXUIBuildEngine.AssemblyName)
                {
                    resourceDescription = new ResourceDescription(
                                    string.Format("{0}.g.resources", CXUIBuildEngine.AssemblyName),
                                    () => File.OpenRead(resourcePath),
                                    true);
                    resourceDescriptions.Add(resourceDescription);
                }

                // Emit code and embedd ressources
                EmitResult result = compilation.Emit(ms, manifestResources: resourceDescriptions.ToArray());

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);


                    foreach (Diagnostic diagnostic in failures)
                    {
                        var error = new BuildErrorEventArgs
                            (
                                diagnostic.Descriptor.Category,
                                "",
                                "",
                                0, 0, 0, 0, diagnostic.GetMessage(), diagnostic.Id, this.ToString()
                            );

                        BuildEngine.LogErrorEvent(error);
                    }
                }
                else
                {
                    // Reset stream
                    ms.Seek(0, SeekOrigin.Begin);
                    base.CXUIBuildEngine.RawAssembly = ms.ToArray();

                    // Generate output
                    if (WriteToFileSystem)
                    {
                        string _path = Path.Combine(OutputDirectory, CXUIBuildEngine.AssemblyName + ".dll");
                        string _dir = Path.GetDirectoryName(_path);

                        if (!Directory.Exists(_dir))
                        {
                            Directory.CreateDirectory(_dir);
                        }

                        File.WriteAllBytes(_path, base.CXUIBuildEngine.RawAssembly);
                    }
                }
            }

            return true;
        }
        #endregion

        #region Public Member
        /// <summary>
        /// If set to true, the assembly will be written to the filesystem
        /// </summary>
        public bool WriteToFileSystem
        {
            get;
            set;
        }

        /// <summary>
        /// The directory for writing the assembly to
        /// </summary>
        public string OutputDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// List of sources which should be compiled
        /// </summary>
        public IList<string> Sources
        {
            get
            {
                return sources;
            }
        }
        #endregion
    }
}