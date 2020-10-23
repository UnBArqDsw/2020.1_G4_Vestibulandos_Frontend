using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Utils
{
    public class PathUtils
    {
        private static Dictionary<string, byte> PersistentPathDict = new Dictionary<string, byte>();

        public static string GetPersistentPath(string path)
        {
            return Application.persistentDataPath + "/" + path;
        }

        public static string GetWWWPath(string path)
        {
            if (path.StartsWith("http://") || path.StartsWith("ftp://") ||
                path.StartsWith("https://") || path.StartsWith("file://") ||
                path.StartsWith("jar:file://"))
            {
                return path;
            }
            if (Application.platform == RuntimePlatform.Android)
            {
                return path.Insert(0, "file://");
            }
            else
            {
                return path.Insert(0, "file:///");
            }
        }

        public static string ProjectPath()
        {
            return "file:///" + Application.dataPath + "/../";
        }

        private static string SteamingAssetsPath(string path = "")
        {
            if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                return "file:///" + Application.dataPath + "/StreamingAssets/" + path;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                return Application.streamingAssetsPath + "/" + path;
            }

            return "file:///" + Application.streamingAssetsPath + "/" + path;
        }

        public static string WebPath(string uri)
        {
            if (PersistentPathDict.TryGetValue(uri, out byte found))
            {
                string persistentPath = GetPersistentPath(uri);
                return GetWWWPath(persistentPath);
            }

            string path = GetPersistentPath(uri);
            if (File.Exists(path))
            {
                PersistentPathDict[path] = 1;
                return GetWWWPath(path);
            }

            return SteamingAssetsPath(uri);
        }

        public static string GetAssetName(string resName)
        {
            int index = resName.LastIndexOf(".");
            if (index < 0)
                return resName;

            return resName.Substring(0, index);
        }
    }
}
