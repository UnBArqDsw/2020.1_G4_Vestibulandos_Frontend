using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestaoC : IQuestao
{
    public string Anunciado { get; set; }
    public TipoDadosQuestao TipoDadosQuestao { get; set; }

    public TipoQuestao TipoQuestao { get => TipoQuestao.C; }

    public Sprite Imagem;
    public List<string> Opcoes;
    public string RespostaCorreta;
}
