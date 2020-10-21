using System;
using System.Resources;
using UnityEngine;
using Util;

namespace Utils
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T s_instance;
        private static bool m_bApplicationIsQuitting;

        public static T Instance
        {
            get
            {
                if (m_bApplicationIsQuitting) return null;
                if (s_instance != null) return s_instance;

                try
                {
                    s_instance = (FindObjectOfType(typeof(T)) as T);
                    if (s_instance == null)
                    {
                        s_instance = new GameObject("[Singleton] " + typeof(T).ToString(), typeof(T)).GetComponent<T>();

                        // Make instance persistent.
                        DontDestroyOnLoad(s_instance);

                        LoggerHelper.Log(
                            $"[Singleton] An instance of <color=yellow>{typeof(T)}</color> is needed in the scene, so <color=yellow>'{s_instance}'</color> was created with DontDestroyOnLoad.");

                        s_instance.Init();
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.LogError(ex.ToString());
                    return null;
                }

                return s_instance;
            }
        }

        public void Create() { }

        private void Awake()
        {
            if (s_instance == null)
            {
                s_instance = (this as T);
            }
        }

        public virtual void Init() { }

        public virtual void OnApplicationQuit()
        {
            s_instance = null;
            m_bApplicationIsQuitting = true;
        }
    }
}
