﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dynamo.Core;
using Dynamo.Engine;
using Dynamo.Exceptions;
using Dynamo.Extensions;
using Dynamo.Interfaces;
using Dynamo.Logging;
using Dynamo.Utilities;
using DynamoPackages.Properties;
using DynamoUtilities;

namespace Dynamo.PackageManager
{
    public struct LoadPackageParams
    {
        public IPreferences Preferences { get; set; }
        public IPathManager PathManager { get; set; }
    }

    public enum AssemblyLoadingState
    {
        Success,
        NotManagedAssembly,
        AlreadyLoaded
    }

    public class PackageLoader : LogSourceBase
    {
        internal event Action<Assembly> RequestLoadNodeLibrary;
        internal event Action<IEnumerable<Assembly>> PackagesLoaded;
        internal event Func<string, IEnumerable<CustomNodeInfo>> RequestLoadCustomNodeDirectory;
        internal event Func<string, IExtension> RequestLoadExtension;
        internal event Action<IExtension> RequestAddExtension;

        /// <summary>
        /// This event is raised when a package is first added to the list of packages this package loader is loading.
        /// This event occurs before the package is fully loaded. 
        /// </summary>
        public event Action<Package> PackageAdded;

        /// <summary>
        /// This event is raised when a package is fully loaded. It will be true that when this event is raised
        /// Packge.Loaded will be true for the package.
        /// </summary>
        // TODO 3.0 Fix spelling
        public event Action<Package> PackgeLoaded;

        /// <summary>
        /// This event is raised when the package is removed from the list of packages loaded by this packageLoader.
        /// </summary>
        public event Action<Package> PackageRemoved;

        private readonly List<IExtension> requestedExtensions = new List<IExtension>();
        /// <summary>
        /// Collection of ViewExtensions the ViewExtensionSource requested be loaded.
        /// </summary>
        public IEnumerable<IExtension> RequestedExtensions
        {
            get
            {
                return requestedExtensions;
            }
        }

        private readonly List<Package> localPackages = new List<Package>();
        public IEnumerable<Package> LocalPackages { get { return localPackages; } }

        private readonly List<string> packagesDirectories = new List<string>();
        public string DefaultPackagesDirectory
        {
            get { return packagesDirectories[0]; }
        }

        public PackageLoader(string overridePackageDirectory)
            : this(new[] { overridePackageDirectory })
        {
        }

        public PackageLoader(IEnumerable<string> packagesDirectories)
        {
            if (packagesDirectories == null)
                throw new ArgumentNullException("packagesDirectories");

            this.packagesDirectories.AddRange(packagesDirectories);
            var error = PathHelper.CreateFolderIfNotExist(DefaultPackagesDirectory);

            if (error != null)
                Log(error);
        }

        private void OnPackageAdded(Package pkg)
        {
            if (PackageAdded != null)
            {
                PackageAdded(pkg);
            }
        }

        private void OnPackageRemoved(Package pkg)
        {
            if (PackageRemoved != null)
            {
                PackageRemoved(pkg);
            }
        }

        internal void Add(Package pkg)
        {
            if (!this.localPackages.Contains(pkg))
            {
                this.localPackages.Add(pkg);
                pkg.MessageLogged += OnPackageMessageLogged;
                OnPackageAdded(pkg);
            }
        }

        internal void Remove(Package pkg)
        {
            if (this.localPackages.Contains(pkg))
            {
                this.localPackages.Remove(pkg);
                pkg.MessageLogged -= OnPackageMessageLogged;
                OnPackageRemoved(pkg);
            }
        }

        private void OnPackageMessageLogged(ILogMessage obj)
        {
            Log(obj);
        }

        private void OnRequestLoadNodeLibrary(Assembly assem)
        {
            if (RequestLoadNodeLibrary != null)
            {
                RequestLoadNodeLibrary(assem);
            }
        }

        private void OnPackagesLoaded(IEnumerable<Assembly> assemblies)
        {
            PackagesLoaded?.Invoke(assemblies);
        }

        private IEnumerable<CustomNodeInfo> OnRequestLoadCustomNodeDirectory(string directory)
        {
            if (RequestLoadCustomNodeDirectory != null)
            {
                return RequestLoadCustomNodeDirectory(directory);
            }

            return new List<CustomNodeInfo>();
        }

        /// <summary>
        /// Try loading package into Library (including all node libraries and custom nodes)
        /// and add to LocalPackages.
        /// </summary>
        /// <param name="package"></param>
        internal void TryLoadPackageIntoLibrary(Package package)
        {
            this.Add(package);

            // Prevent duplicate loads
            if (package.Loaded) return;

            try
            {
                // load node libraries
                foreach (var assem in package.EnumerateAssembliesInBinDirectory())
                {
                    if (assem.IsNodeLibrary)
                    {
                        try
                        {
                            OnRequestLoadNodeLibrary(assem.Assembly);
                        }
                        catch (LibraryLoadFailedException ex)
                        {
                            Log(ex.GetType() + ": " + ex.Message);
                        }
                    }
                }

                // load custom nodes
                var customNodes = OnRequestLoadCustomNodeDirectory(package.CustomNodeDirectory);
                package.LoadedCustomNodes.AddRange(customNodes);

                package.EnumerateAdditionalFiles();
                // If the additional files contained an extension manifest, then request it be loaded.
                var extensionManifests = package.AdditionalFiles.Where(
                    file => file.Model.Name.Contains("ExtensionDefinition.xml") && !(file.Model.Name.Contains("ViewExtensionDefinition.xml"))).ToList();
                foreach (var extPath in extensionManifests)
                {
                    var extension = RequestLoadExtension?.Invoke(extPath.Model.FullName);
                    if (extension != null)
                    {
                        RequestAddExtension?.Invoke(extension);
                    }
                    this.requestedExtensions.Add(extension);
                }

                package.Loaded = true;
                this.PackgeLoaded?.Invoke(package);
            }
            catch (Exception e)
            {
                Log("Exception when attempting to load package " + package.Name + " from " + package.RootDirectory);
                Log(e.GetType() + ": " + e.Message);
            }
        }

        /// <summary>
        ///     Load the package into Dynamo (including all node libraries and custom nodes)
        ///     and add to LocalPackages.
        /// </summary>
        // TODO: Remove in 3.0 (Refer to PR #9736).
        [Obsolete("This API will be deprecated in 3.0. Use LoadPackages(IEnumerable<Package> packages) instead.")]
        public void Load(Package package)
        {
            TryLoadPackageIntoLibrary(package);

            var assemblies =
                LocalPackages.SelectMany(x => x.EnumerateAssembliesInBinDirectory().Where(y => y.IsNodeLibrary));
            OnPackagesLoaded(assemblies.Select(x => x.Assembly));
        }

        /// <summary>
        ///     Scan the PackagesDirectory for packages and attempt to load all of them.  Beware! Fails silently for duplicates.
        /// </summary>
        public void LoadAll(LoadPackageParams loadPackageParams)
        {
            ScanAllPackageDirectories(loadPackageParams.Preferences);

            var pathManager = loadPackageParams.PathManager;
            if (pathManager != null)
            {
                foreach (var pkg in LocalPackages)
                {
                    if (Directory.Exists(pkg.BinaryDirectory))
                    {
                        pathManager.AddResolutionPath(pkg.BinaryDirectory);
                    }

                }
            }

            if (LocalPackages.Any())
            {
                LoadPackages(LocalPackages);
            }
        }

        /// <summary>
        /// Loads and imports all packages. 
        /// </summary>
        /// <param name="packages"></param>
        public void LoadPackages(IEnumerable<Package> packages)
        {
            foreach (var pkg in packages)
            {
                TryLoadPackageIntoLibrary(pkg);
            }

            var assemblies =
                LocalPackages.SelectMany(x => x.EnumerateAssembliesInBinDirectory().Where(y => y.IsNodeLibrary));
            OnPackagesLoaded(assemblies.Select(x => x.Assembly));
        }

        public void LoadCustomNodesAndPackages(LoadPackageParams loadPackageParams, CustomNodeManager customNodeManager)
        {
            foreach (var path in loadPackageParams.Preferences.CustomPackageFolders)
            {
                customNodeManager.AddUninitializedCustomNodesInPath(path, false, false);
                if (!this.packagesDirectories.Contains(path))
                {
                    this.packagesDirectories.Add(path);
                }
            }
            LoadAll(loadPackageParams);
        }

        private void ScanAllPackageDirectories(IPreferences preferences)
        {
            foreach (var packagesDirectory in packagesDirectories)
            {
                ScanPackageDirectories(packagesDirectory, preferences);
            }
        }

        private void ScanPackageDirectories(string root, IPreferences preferences)
        {
            try
            {
                if (!Directory.Exists(root))
                {
                    string extension = null;
                    if (root != null)
                    {
                        extension = Path.GetExtension(root);
                    }

                    // If the path has a .dll or .ds extension it is a locally imported library
                    // so do not output an error about the directory
                    if (extension == ".dll" || extension == ".ds")
                    {
                        return;
                    }

                    this.Log(string.Format(Resources.InvalidPackageFolderWarning, root));
                    return;
                }

                foreach (var dir in
                    Directory.EnumerateDirectories(root, "*", SearchOption.TopDirectoryOnly))
                {
                    var pkg = ScanPackageDirectory(dir);
                    if (pkg != null && preferences.PackageDirectoriesToUninstall.Contains(dir))
                        pkg.MarkForUninstall(preferences);
                }
            }
            catch (UnauthorizedAccessException ex) { }
            catch (IOException ex) { }
            catch (ArgumentException ex) { }
        }

        public Package ScanPackageDirectory(string directory)
        {
            try
            {
                var headerPath = Path.Combine(directory, "pkg.json");

                Package discoveredPkg;

                // get the package name and the installed version
                if (PathHelper.IsValidPath(headerPath))
                {
                    discoveredPkg = Package.FromJson(headerPath, AsLogger());
                    if (discoveredPkg == null)
                        throw new LibraryLoadFailedException(directory, String.Format(Properties.Resources.MalformedHeaderPackage, headerPath));
                }
                else
                {
                    throw new LibraryLoadFailedException(directory, String.Format(Properties.Resources.NoHeaderPackage, headerPath));
                }

                // prevent duplicates
                if (LocalPackages.All(pkg => pkg.Name != discoveredPkg.Name))
                {
                    this.Add(discoveredPkg);
                    return discoveredPkg; // success
                }
                throw new LibraryLoadFailedException(directory, String.Format(Properties.Resources.DulicatedPackage, discoveredPkg.Name, discoveredPkg.RootDirectory));
            }
            catch (Exception e)
            {
                Log(String.Format(Properties.Resources.ExceptionEncountered, directory), WarningLevel.Error);
                Log(e);
            }

            return null;
        }

        /// <summary>
        ///     Attempt to load a managed assembly in to ReflectionOnlyLoadFrom context. 
        /// </summary>
        /// <param name="filename">The filename of a DLL</param>
        /// <param name="assem">out Assembly - the passed value does not matter and will only be set if loading succeeds</param>
        /// <returns>Returns Success if success, NotManagedAssembly if BadImageFormatException, AlreadyLoaded if FileLoadException</returns>
        internal static AssemblyLoadingState TryReflectionOnlyLoadFrom(string filename, out Assembly assem)
        {
            try
            {
                assem = Assembly.ReflectionOnlyLoadFrom(filename);
                return AssemblyLoadingState.Success;
            }
            catch (BadImageFormatException)
            {
                assem = null;
                return AssemblyLoadingState.NotManagedAssembly;
            }
            catch (FileLoadException)
            {
                assem = null;
                return AssemblyLoadingState.AlreadyLoaded;
            }
        }

        /// <summary>
        ///     Attempt to load a managed assembly in to LoadFrom context. 
        /// </summary>
        /// <param name="filename">The filename of a DLL</param>
        /// <param name="assem">out Assembly - the passed value does not matter and will only be set if loading succeeds</param>
        /// <returns>Returns true if success, false if BadImageFormatException (i.e. not a managed assembly)</returns>
        internal static bool TryLoadFrom(string filename, out Assembly assem)
        {
            try
            {
                assem = Assembly.LoadFrom(filename);
                return true;
            }
            catch (BadImageFormatException)
            {
                assem = null;
                return false;
            }
        }

        public bool IsUnderPackageControl(string path)
        {
            return LocalPackages.Any(ele => ele.ContainsFile(path));
        }

        public bool IsUnderPackageControl(CustomNodeInfo def)
        {
            return IsUnderPackageControl(def.Path);
        }

        public bool IsUnderPackageControl(Type t)
        {
            return LocalPackages.Any(package => package.LoadedTypes.Contains(t));
        }

        public bool IsUnderPackageControl(Assembly t)
        {
            return LocalPackages.Any(package => package.LoadedAssemblies.Any(x => x.Assembly == t));
        }

        public Package GetPackageFromRoot(string path)
        {
            return LocalPackages.FirstOrDefault(pkg => pkg.RootDirectory == path);
        }

        public Package GetOwnerPackage(Type t)
        {
            return LocalPackages.FirstOrDefault(package => package.LoadedTypes.Contains(t));
        }

        public Package GetOwnerPackage(CustomNodeInfo def)
        {
            return GetOwnerPackage(def.Path);
        }

        public Package GetOwnerPackage(string path)
        {
            return LocalPackages.FirstOrDefault(ele => ele.ContainsFile(path));
        }

        private static bool hasAttemptedUninstall;

        internal void DoCachedPackageUninstalls(IPreferences preferences)
        {
            // this can only be run once per app run
            if (hasAttemptedUninstall) return;
            hasAttemptedUninstall = true;

            var pkgDirsRemoved = new HashSet<string>();
            foreach (var pkgNameDirTup in preferences.PackageDirectoriesToUninstall)
            {
                try
                {
                    Directory.Delete(pkgNameDirTup, true);
                    pkgDirsRemoved.Add(pkgNameDirTup);
                    Log(String.Format("Successfully uninstalled package from \"{0}\"", pkgNameDirTup));
                }
                catch
                {
                    Log(
                        String.Format(
                            "Failed to delete package directory at \"{0}\", you may need to delete the directory manually.",
                            pkgNameDirTup),
                        WarningLevel.Moderate);
                }
            }

            preferences.PackageDirectoriesToUninstall.RemoveAll(pkgDirsRemoved.Contains);
        }
    }
}
