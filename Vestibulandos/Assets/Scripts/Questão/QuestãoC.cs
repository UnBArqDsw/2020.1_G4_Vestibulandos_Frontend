using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
public class QuestaoC : IQuestao
{
    [SerializeField]
    public TipoQuestao TipoQuestao => TipoQuestao.C;

    [SerializeField]
    //public string Anunciado { get; set; }
    public string Anunciado;

    [SerializeField]
    public TipoDadosQuestao TipoDadosQuestao { get; set; }

    public Sprite Imagem;
    public List<string> Opcoes;
    public string RespostaCorreta;

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        throw new System.NotImplementedException();
    }
}
