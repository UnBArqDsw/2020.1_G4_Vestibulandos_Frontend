using UnityEngine;

[System.Serializable]
public enum TipoQuestao
{
    A,
    B,
    C,
    D
}

[System.Serializable]
public enum TipoDadosQuestao
{
    Texto,
    Imagem
}

[SerializeField]
public enum JogoEstado
{
    Jogando,
    Proximo
}