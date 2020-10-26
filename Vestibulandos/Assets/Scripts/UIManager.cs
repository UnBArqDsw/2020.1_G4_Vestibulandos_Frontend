using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoSingleton<UIManager>
{
    [SerializeField]
    private CategoryBtnScript m_categoriaBotao;

    [SerializeField]
    private GameObject scrollHolder;

    [SerializeField]
    private Text m_txtPontuacao;

    [SerializeField]
    private Text m_txtTempo;

    [SerializeField]
    private List<Image> m_listVida;

    [SerializeField]
    private GameObject m_goFimJogo;

    [SerializeField]
    private GameObject m_goInicio;

    [SerializeField]
    private GameObject m_goPartida;

    [SerializeField]
    private Color m_colorCorreto;

    [SerializeField]
    private Color m_colorErrado;

    [SerializeField]
    private Color m_colorNormal;

    [SerializeField]
    private Image m_imgAnuciado;

    [SerializeField]
    private Text m_txtAnunciado;

    [SerializeField]
    private List<Button> m_listRespostas;

    private IQuestao atualQuestao;
    private bool respondido = false;

    public Text Tempo => m_txtTempo;
    public Text Pontuacao => m_txtPontuacao;
    public GameObject FimJogo => m_goFimJogo;

    private void Start()
    {
        for (int i = 0; i < m_listRespostas.Count; i++)
        {
            Button localBtn = m_listRespostas[i];
            localBtn.onClick.AddListener(() => OnClick(localBtn));
        }

        CriarCategoriaBtns();
    }

    public void AdicionarQuestao(IQuestao questao)
    {
        atualQuestao = questao;

        switch (questao.TipoDadosQuestao)
        {
            case TipoDadosQuestao.Texto:
                {
                    m_imgAnuciado.transform.parent.gameObject.SetActive(false);
                }
                break;
            case TipoDadosQuestao.Imagem:
                {
                    m_imgAnuciado.transform.parent.gameObject.SetActive(true);
                    m_imgAnuciado.transform.gameObject.SetActive(true);
                    //questionImg.sprite = question.Imagem;
                }
                break;
        }

        m_txtAnunciado.text = questao.Anunciado;

        if (questao.TipoQuestao == TipoQuestao.C)
        {
            QuestaoC q = questao as QuestaoC;
            List<string> respostas = ShuffleList.ShuffleListItems<string>(q.Opcoes);

            for (int i = 0; i < respostas.Count; i++)
            {
                m_listRespostas[i].GetComponentInChildren<Text>().text = respostas[i];
                m_listRespostas[i].name = respostas[i];
                m_listRespostas[i].image.color = m_colorNormal;
            }
        }

        respondido = false;
    }

    public void ReduzirVida(int vida)
    {
        m_listVida[vida].color = Color.red;
    }

    void OnClick(Button btn)
    {
        if (GameManager.Instance.Estado == JogoEstado.Jogando)
        {
            if (!respondido)
            {
                respondido = true;

                if (GameManager.Instance.Resposta(btn.name))
                {
                    //btn.image.color = correctCol;
                    StartCoroutine(BlinkImg(btn.image));
                }
                else
                {
                    btn.image.color = m_colorErrado;
                }
            }
        }
    }

    void CriarCategoriaBtns()
    {
        for (int i = 0; i < GameManager.Instance.QuizData.Count; i++)
        {
            CategoryBtnScript categoria = Instantiate(m_categoriaBotao, scrollHolder.transform);

            categoria.SetButton(GameManager.Instance.QuizData[i].Categoria, GameManager.Instance.QuizData[i].Questoes.Count);
            
            int index = i;
            categoria.Btn.onClick.AddListener(() => OnCategoriaBtn(index, GameManager.Instance.QuizData[index].Categoria));
        }
    }

    private void OnCategoriaBtn(int id, string categoria)
    {
        GameManager.Instance.Iniciar(id, categoria);

        m_goInicio.SetActive(false);
        m_goPartida.SetActive(true);
    }

    IEnumerator BlinkImg(Image image)
    {
        for (int i = 0; i < 2; i++)
        {
            image.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            image.color = m_colorCorreto;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void OnNovamenteBtn()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
