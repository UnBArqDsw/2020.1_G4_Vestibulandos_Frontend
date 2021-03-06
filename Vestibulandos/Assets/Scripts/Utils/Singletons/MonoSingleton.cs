﻿using System;
using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T s_instance;

    private static bool m_bApplicationIsQuitting;

    public static T Instance
    {
        get
        {
            if (m_bApplicationIsQuitting) 
                return null;

            if (s_instance != null) 
                return s_instance;

            try
            {
                s_instance = (FindObjectOfType(typeof(T)) as T);

                if (s_instance == null)
                {
                    s_instance = new GameObject("[Singleton] " + typeof(T).ToString(), typeof(T)).GetComponent<T>();

                    DontDestroyOnLoad(s_instance);

                    UnityEngine.Debug.Log(
                        $"[Singleton] An instance of <color=yellow>{typeof(T)}</color> is needed in the scene, so <color=yellow>'{s_instance}'</color> was created with DontDestroyOnLoad.");

                    s_instance.Init();
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex.ToString());
                return null;
            }

            return s_instance;
        }
    }

    public static bool IsCreated()
    {
        return (s_instance != null);
    }

    public static void Destroy()
    {
        s_instance = default;
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
