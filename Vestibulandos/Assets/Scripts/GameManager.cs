using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Utils.Singletons;

public class GameManager : MonoSingleton<GameManager>
{
    [SerializeField]
    private Camera m_mainCamera = null;

    public Camera MainCamera => m_mainCamera;

    private void Awake()
    {
        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        if (m_mainCamera == null)
        {
            var mainCamera = GameObject.Find("Main Camera");
            if (mainCamera)
            {
                // Get the camera component.
                var cam = mainCamera.GetComponent<UnityEngine.Camera>();

                // Set the camera.
                m_mainCamera = cam;
            }
            else
            {
                LoggerHelper.LogError("Failed to find the 'main camera'.");
                return;
            }
        }

        Initialize();
    }

    private bool Initialize()
    {
        if(!DataManager.GetInstance().LoadData())
        {
            LoggerHelper.LogError("Failed to load the data.");
            return false;
        }

        return true;
    }

    // Start is called before the first frame update
    void Start()

    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}
