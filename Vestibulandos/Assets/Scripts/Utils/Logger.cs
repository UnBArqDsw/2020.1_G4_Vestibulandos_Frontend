using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;
using UnityEngine.Internal;
using Utils;

namespace Util
{
    public sealed class LoggerHelper
    {
        private static bool _isOpenBubug = true;

        private static bool _isOpenWriteLogTofile = false;
        private static bool _isOpenWriteStrToFile = false;

        public static string LogFileName = "";

        private static Application.LogCallback LogCallBack = null;

        private static StreamWriter WriterForLog = null;
        private static StreamWriter WriterForStr = null;

        private static string LogOutPutPath = null;
        private static string WriteStrFilePath = null;
        private static string OriginalFileContent = null;

        public static bool IsOpenDebug
        {
            get
            {
                return _isOpenBubug;
            }
            set
            {
                _isOpenBubug = value;
            }
        }

        public static bool IsOpenWriteLogToFile
        {
            get
            {
                return _isOpenWriteLogTofile;
            }
            set
            {
                _isOpenWriteLogTofile = value;

                if (_isOpenWriteLogTofile && !IsMethodInDelegateList(LogCallBack, new Application.LogCallback(LogMessageWriteToFile)))
                {
                    LogCallBack = (Application.LogCallback)Delegate.Combine(LogCallBack, new Application.LogCallback(LogMessageWriteToFile));

                    if (WriterForLog == null)
                    {
                        WriterForLog = File.CreateText(SetLogOutPutPath());
                    }
                }
                else if (!_isOpenWriteLogTofile && IsMethodInDelegateList(LogCallBack, new Application.LogCallback(LogMessageWriteToFile)))
                {
                    LogCallBack = (Application.LogCallback)Delegate.Remove(LogCallBack, new Application.LogCallback(LogMessageWriteToFile));
                }

                //Application.RegisterLogCallback(LogCallBack);
                Application.logMessageReceived += LogCallBack;
            }
        }

        public static bool IsOpenWriteStrToFile
        {
            get
            {
                return _isOpenWriteStrToFile;
            }
            set
            {
                _isOpenWriteStrToFile = value;

                if (_isOpenWriteStrToFile && !File.Exists(SetWriteStrFilePath()))
                {
                    FileStream fileStream = File.Create(SetWriteStrFilePath());
                    fileStream.Dispose();
                }
            }
        }

        private static bool IsMethodInDelegateList(Application.LogCallback targetCallBack, Application.LogCallback method)
        {
            return targetCallBack != null && targetCallBack.GetInvocationList().Contains(method);
        }

        public static void Break()
        {
            if (IsOpenDebug)
            {
                UnityEngine.Debug.Break();
            }
        }

        public static void ClearDeveloperConsole()
        {
            if (IsOpenDebug)
            {
                UnityEngine.Debug.ClearDeveloperConsole();
            }
        }

        public static void DebugBreak()
        {
            if (IsOpenDebug)
            {
                UnityEngine.Debug.DebugBreak();
            }
        }

        private static StringBuilder LogArray<T>(string separator, T[] messages)
        {
            StringBuilder str = new StringBuilder();
            str.Append(DateTime.Now.ToString("G"));
            str.Append(" ：");

            if (messages != null)
            {
                if (messages.Length > 0)
                {
                    for (int i = 0; i < messages.Length; i++)
                    {
                        T t = messages[i];
                        if (i < messages.Length - 1)
                        {
                            if (t == null)
                            {
                                str.AppendFormat("{0}{1}", "null", separator);
                            }
                            else
                            {
                                str.AppendFormat("{0}{1}", t, separator);
                            }
                        }
                        else if (t == null)
                        {
                            str.AppendFormat("{0}", "null");
                        }
                        else
                        {
                            str.AppendFormat("{0}", t);
                        }
                    }
                }
                else
                {
                    str.AppendFormat("messages.Length:{0}", messages.Length);
                }
            }
            else
            {
                str.Append("messsages:null");
            }

            return str;
        }

        public static void Log<T>(string separator, T[] messages)
        {
            if (IsOpenDebug || IsOpenWriteLogToFile)
            {
                UnityEngine.Debug.Log(LogArray<T>(separator, messages).ToString());
            }
        }

        public static void Log(params object[] messages)
        {
            Log("   ", messages);
        }

        public static void Log<T>(params T[] messages)
        {
            Log("   ", messages);
        }

        public static void Log<T>(List<T> messages)
        {
            Log("   ", messages.ToArray());
        }

        public static void Log(Vector3 vector)
        {
            if (IsOpenDebug)
            {
                StringBuilder str = new StringBuilder();
                str.Append(DateTime.Now.ToString("G"));
                str.Append(" ：");
                str.Append("Vector3(");
                str.AppendFormat("{0},", vector.x);
                str.AppendFormat("{0},", vector.y);
                str.AppendFormat("{0}", vector.z);
                str.Append(")");

                UnityEngine.Debug.Log(str.ToString());
            }
        }

        public static void Log<T, M>(Dictionary<T, M> dics)
        {
            if (IsOpenDebug)
            {
                StringBuilder str = new StringBuilder();
                str.Append(DateTime.Now.ToString("G"));
                str.Append(" ：");
                str.Append("Dictionary ");

                if (dics != null)
                {
                    foreach (T t in dics.Keys)
                    {
                        M m = dics[t];

                        if (m == null)
                        {
                            str.AppendFormat(" {0}:{1} ,", t, "null");
                        }
                        else
                        {
                            str.AppendFormat(" {0}:{1} ,", t, m);
                        }
                    }

                    str.AppendFormat("    keys Count:{0}", dics.Keys.Count);
                }
                else
                {
                    str.Append("be null");
                }

                UnityEngine.Debug.Log(str.ToString());
            }
        }

        public static void Log(object message, UnityEngine.Object context)
        {
            if (IsOpenDebug)
            {
                UnityEngine.Debug.Log(message, context);
            }
        }

        public static void LogWarning(params object[] messages)
        {
            LogWarning("   ", messages);
        }

        public static void LogWarning<T>(string separator, T[] messages)
        {
            if (IsOpenDebug)
            {
                UnityEngine.Debug.LogWarning(LogArray<T>(separator, messages).ToString());
            }
        }

        public static void LogWarning<T>(params T[] messages)
        {
            LogWarning("  ", messages);
        }

        public static void LogWarning<T>(List<T> messages)
        {
            LogWarning("  ", messages.ToArray());
        }

        public static void LogWarning(object message, UnityEngine.Object context)
        {
            if (IsOpenDebug)
            {
                UnityEngine.Debug.LogWarning(message, context);
            }
        }

        public static void LogError(params object[] messages)
        {
            LogError("   ", messages);
        }

        public static void LogError<T>(string separator, T[] messages)
        {
            if (IsOpenDebug || IsOpenWriteLogToFile)
            {
                UnityEngine.Debug.LogError(LogArray(separator, messages).ToString());
            }
        }

        public static void LogError<T>(params T[] messages)
        {
            LogError("   ", messages);
        }

        public static void LogError(object message, UnityEngine.Object context)
        {
            if (IsOpenDebug)
            {
                UnityEngine.Debug.LogError(message, context);
            }
        }

        public static void LogException(Exception exception)
        {
            if (IsOpenDebug)
            {
                UnityEngine.Debug.LogException(exception);
            }
        }

        public static void LogException(Exception exception, UnityEngine.Object context)
        {
            if (IsOpenDebug)
            {
                UnityEngine.Debug.LogException(exception, context);
            }
        }

        public static void LogStackMsg(string message)
        {
            if (IsOpenDebug)
            {
                StackTrace arg = new StackTrace();
                UnityEngine.Debug.LogError(message + arg);
            }
        }

        [ExcludeFromDocs]
        public static void DrawLine(Vector3 start, Vector3 end)
        {
            if (IsOpenDebug)
            {
                UnityEngine.Debug.DrawLine(start, end);
            }
        }

        [ExcludeFromDocs]
        public static void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            if (IsOpenDebug)
            {
                UnityEngine.Debug.DrawLine(start, end, color);
            }
        }

        [ExcludeFromDocs]
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration)
        {
            if (IsOpenDebug)
            {
                UnityEngine.Debug.DrawLine(start, end, color, duration);
            }
        }

        public static void DrawLine(Vector3 start, Vector3 end, [DefaultValue("Color.white")] Color color, [DefaultValue("0.0f")] float duration, [DefaultValue("true")] bool depthTest)
        {
            if (IsOpenDebug)
            {
                UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
            }
        }

        [ExcludeFromDocs]
        public static void DrawRay(Vector3 start, Vector3 dir)
        {
            if (IsOpenDebug)
            {
                UnityEngine.Debug.DrawRay(start, dir);
            }
        }

        [ExcludeFromDocs]
        public static void DrawRay(Vector3 start, Vector3 dir, Color color)
        {
            if (IsOpenDebug)
            {
                UnityEngine.Debug.DrawRay(start, dir, color);
            }
        }

        [ExcludeFromDocs]
        public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration)
        {
            if (IsOpenDebug)
            {
                UnityEngine.Debug.DrawRay(start, dir, color, duration);
            }
        }

        public static void DrawRay(Vector3 start, Vector3 dir, [DefaultValue("Color.white")] Color color, [DefaultValue("0.0f")] float duration, [DefaultValue("true")] bool depthTest)
        {
            if (IsOpenDebug)
            {
                UnityEngine.Debug.DrawRay(start, dir, color, duration, depthTest);
            }
        }

        public static string GetWriteLogFilePath()
        {
            return LogOutPutPath;
        }

        public static string GetWriteStrFilePath()
        {
            return WriteStrFilePath;
        }

        private static string SetLogOutPutPath()
        {
            DirectoryInfo directoryInfo = CheckDirectoryOrCreate(Application.persistentDataPath + "/DebugLog");
            FileInfo[] files = directoryInfo.GetFiles("*.txt", SearchOption.AllDirectories);

            if (files.Length >= 5)
            {
                double totalMinutes = (files[0].CreationTime - DateTime.MinValue).TotalMinutes;
                FileInfo fileInfo = files[0];

                foreach (FileInfo f in files)
                {
                    TimeSpan timeSpan = f.CreationTime - DateTime.MinValue;
                    if (timeSpan.TotalMinutes < totalMinutes)
                    {
                        totalMinutes = timeSpan.TotalMinutes;
                        fileInfo = f;
                    }
                }

                fileInfo.Delete();
            }

            LogOutPutPath = Application.persistentDataPath + "/DebugLog/" + DateTime.Now.ToString("yyyy_MM_dd , hh-mm-ss") + ".txt";
            return LogOutPutPath;
        }

        private static string SetWriteStrFilePath()
        {
            CheckDirectoryOrCreate(Application.persistentDataPath + "/DebugLog");
            return string.Concat(Application.persistentDataPath, "/DebugLog/", "_", DateTime.Now.ToString("yyyy_MM_dd"), ".log");
        }

        private static DirectoryInfo CheckDirectoryOrCreate(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            if (!dir.Exists)
                dir.Create();

            return dir;
        }

        private static void LogMessageWriteToFile(string message, string stackTrace, LogType type)
        {
            WriterForLog.WriteLine(string.Concat(type.ToString(), ":", message, "  StackTrace:", stackTrace));

            WriterForLog.WriteLine("");
            WriterForLog.Flush();
        }

        public static void WriteStrToFile(string strContent)
        {
            if (IsOpenWriteStrToFile)
            {
                if (OriginalFileContent == null)
                {
                    try
                    {
                        OriginalFileContent = File.ReadAllText(WriteStrFilePath);
                    }
                    catch (Exception /*ex*/)
                    {
                        return;
                    }
                }

                if (WriterForStr == null)
                {
                    WriterForStr = File.CreateText(WriteStrFilePath);
                    WriterForStr.Write(OriginalFileContent);
                }

                WriterForStr.WriteLine(strContent);
                WriterForStr.WriteLine("");
                WriterForStr.Flush();
            }
        }

        public static void Dispose()
        {
            if (WriterForLog != null)
            {
                WriterForLog.Dispose();
            }

            if (WriterForStr != null)
            {
                WriterForStr.Dispose();
            }
        }
    }
}