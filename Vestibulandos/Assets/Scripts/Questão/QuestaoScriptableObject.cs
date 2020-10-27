using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestãoData", menuName = "Criar Questao", order = 1)]
public class QuestaoScriptableObject : ScriptableObject
{
    public string Categoria;
    public List<QuestaoC> QuestoesC;
}
