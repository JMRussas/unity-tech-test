using System;
using UnityEditor;
using UnityEngine;

namespace TinyGiantStudio.PackageImporter
{
    /// <summary>
    /// Contains information about a package
    /// </summary>
    [Serializable]
    internal class TGS_Package
    {
        public string packageName = string.Empty;
        public DefaultAsset package = null;
        [Tooltip("The description of the unitypackage")]
        public string Description = string.Empty;
        public string packageIdentifier = "Unique Package Indentifier";

        public bool hasPipelineDependancy = false;
        public Pipeline targetPipeline = new Pipeline();

        public int[] minimumPipelineVersion;
        public int[] maximumPipelineVersion;

        public bool hasUnityVersionDependancy = false;
        public double minimumUnityVersion = 0.0;
        public double maximumUnityVersion = 0.0;



        public int currentVersion = 1;

        public int GetLastInstalledVersion() => EditorPrefs.GetInt("TinyGiantStudio.PackageImporter." + packageIdentifier + ".LastInstalledVersion");
        public void UpdateLastInstalledVersion() => EditorPrefs.SetInt("TinyGiantStudio.PackageImporter." + packageIdentifier + ".LastInstalledVersion", currentVersion);


        public bool ignoreUpdates;

        public enum Pipeline
        {
            SRP,
            URP,
            HDRP
        }
    }
}

