using UnityEngine;
using UnityEngine.UI;

public class CategoriaBotaoComponente : MonoBehaviour
{
    [SerializeField] 
    private Text m_txtTitulo;

    [SerializeField] 
    private Text m_txtPontuacao;

    [SerializeField] 
    private Button m_btnBotao;

    public Text Titulo => m_txtTitulo;
    public Text Pontuacao => m_txtPontuacao;
    public Button Btn => m_btnBotao;

    public void Inserir(string titutlo, int numeroQuestoes)
    {
        m_txtTitulo.text = titutlo;
        m_txtPontuacao.text = PlayerPrefs.GetInt(titutlo, 0) + "/" + numeroQuestoes;
    }
}
