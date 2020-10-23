using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using Utils.Singletons;

public class UIManager : MonoSingleton<UIManager>
{
    [SerializeField]
    private Canvas m_canvas = null;

    [SerializeField]
    private InicioUI m_inicio = null;

    [SerializeField]
    private PartidaUI m_partida = null;

    public InicioUI InicioUI => m_inicio;

    public PartidaUI PartidaUI => m_partida;

    private void Awake()
    {
        if (!Initialize())
        {
            LoggerHelper.LogError("Failed to init the UI Manager.");
            return;
        }
    }

    private bool Initialize()
    {
        if (m_canvas == null)
        {
            var trCanvas = GameManager.Instance.MainCamera.transform.Find("Canvas");
            if (trCanvas == null)
            {
                m_canvas = trCanvas.GetComponent<Canvas>();
            }
        }

        var main = m_canvas.transform.Find("Background");

        m_inicio = main.Find("Inicio").GetComponent<InicioUI>();
        m_partida = main.Find("Partida").GetComponent<PartidaUI>();

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
