using System.Runtime.Serialization;
using UnityEngine;

public interface IQuestao : ISerializable
{
    //string Anunciado { get; set; }

    TipoQuestao TipoQuestao { get;}

    TipoDadosQuestao TipoDadosQuestao { get; set; }
}