using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    [SerializeField]
    private List<QuestaoScriptableObject> m_listQuestoesData;

    [SerializeField]
    private float m_nTempo;

    private string m_strAtualCategoria = "";

    private int m_nAcertoQuestao = 0;

    private List<IQuestao> m_listQuestoes = new List<IQuestao>();

    private IQuestao m_atualQuestao;

    private int m_nPontuacao;
    private int m_nVida;
    private float m_fAtualTempo;

    private QuestaoScriptableObject m_scriptableQuestoa;

    private JogoEstado m_enEstado = JogoEstado.Proximo;

    public JogoEstado Estado => m_enEstado;

    public List<QuestaoScriptableObject> QuizData { get => m_listQuestoesData; }

    public void Iniciar(int categoriaId, string categoria)
    {
        m_strAtualCategoria = categoria;
        m_nAcertoQuestao = 0;
        m_nPontuacao = 0;

        m_nVida = 3;

        m_fAtualTempo = m_nTempo;

        m_scriptableQuestoa = m_listQuestoesData[categoriaId];
        m_listQuestoes.AddRange(m_scriptableQuestoa.Questoes);

        SelecionarQuestao();
        m_enEstado = JogoEstado.Jogando;
    }

    private void SelecionarQuestao()
    {
        int randonizado = UnityEngine.Random.Range(0, m_listQuestoes.Count);
        m_atualQuestao = m_listQuestoes[randonizado];

        UIManager.Instance.AdicionarQuestao(m_atualQuestao);

        m_listQuestoes.RemoveAt(randonizado);
    }

    private void Update()
    {
        if (m_enEstado != JogoEstado.Jogando)
            return;

        m_fAtualTempo -= Time.deltaTime;
        AtualizarTempo(m_fAtualTempo);
    }

    void AtualizarTempo(float value)
    {
        UIManager.Instance.Tempo.text = TimeSpan.FromSeconds(m_fAtualTempo).ToString("mm':'ss");
        if (m_fAtualTempo <= 0) Fim();
    }

    public bool Resposta(string opcaoSelecionada)
    {
        bool correta = false;

        switch (m_atualQuestao.TipoQuestao)
        {
            case TipoQuestao.C:
                {
                    QuestaoC c = m_atualQuestao as QuestaoC;

                    if (c.RespostaCorreta == opcaoSelecionada)
                    {
                        m_nAcertoQuestao++;

                        correta = true;
                        m_nPontuacao += 1;

                        UIManager.Instance.Pontuacao.text = (m_nPontuacao > 1) ? $"Ponto: {m_nPontuacao}" : $"Pontos: {m_nPontuacao}";

                    }
                    else
                    {
                        m_nVida--;

                        UIManager.Instance.ReduzirVida(m_nVida);

                        if (m_nVida == 0)
                        {
                            Fim();
                        }
                    }
                }
                break;
            default:
                {
                    UnityEngine.Debug.LogError($"Tipo de questao não adicionado. Tipo: {m_atualQuestao.TipoQuestao}.");
                }
                break;
        }

        if (m_enEstado == JogoEstado.Jogando)
        {
            if (m_listQuestoes.Count > 0)
            {
                Invoke("SelecionarQuestao", 0.45f);
            }
            else
            {
                Fim();
            }
        }

        return correta;
    }

    private void Fim()
    {
        m_enEstado = JogoEstado.Proximo;
        UIManager.Instance.FimJogo.SetActive(true);

        PlayerPrefs.SetInt(m_strAtualCategoria, m_nAcertoQuestao);
    }
}
