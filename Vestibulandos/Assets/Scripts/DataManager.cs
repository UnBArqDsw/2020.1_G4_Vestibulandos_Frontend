using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using Utils;

public class DataManager : Singleton<DataManager>
{
    /// <summary>
    /// Item Meta Data.
    /// </summary>
    private Dictionary<int, QuestaoData> m_dictQuestao = null;
    public Dictionary<int, QuestaoData> QuestaoDic => m_dictQuestao;

    public bool LoadData()
    {
        if (!LoadQuestionData())
        {
            LoggerHelper.LogError("Failed to load the Questions.");
            return false;
        }

        return true;
    }

    private static T LoadAsset<T>(string strPath)
        where T : UnityEngine.Object
    {
        return Resources.Load<T>(strPath);
    }

    private bool LoadQuestionData()
    {
        TextAsset textAsset = LoadAsset<TextAsset>("Questao");
        XElement xml = null;

        try
        {
            MemoryStream stream = new MemoryStream(textAsset.bytes);
            xml = XElement.Load(stream);
            if (xml == null)
            {
                LoggerHelper.LogError("Error to load 'Questao.xml'.");
                return false;
            }

            List<XElement> nodes = XMLHelper.GetXElementList(xml, "Questao");
            if (null == nodes || nodes.Count <= 0)
            {
                LoggerHelper.LogError("Error to get node Item");
                return false;
            }

            m_dictQuestao = new Dictionary<int, QuestaoData>(nodes.Count);

            XElement node = null;
            for (int nIndex = 0; nIndex < nodes.Count; nIndex++)
            {
                node = nodes[nIndex];

                QuestaoData questao = new QuestaoData(XMLHelper.GetSafeAttributeInt(node, "ID"), (TipoQuestao)XMLHelper.GetSafeAttributeInt(node, "Tipo"),
                    XMLHelper.GetSafeAttributeStr(node, "Anunciado"), XMLHelper.GetSafeAttributeInt(node, "NumeroAlternativa"),
                    XMLHelper.GetSafeAttributeStr(node, "Alternativa_0"), XMLHelper.GetSafeAttributeStr(node, "Alternativa_1"), XMLHelper.GetSafeAttributeStr(node, "Alternativa_2"),
                    XMLHelper.GetSafeAttributeStr(node, "Alternativa_3"), XMLHelper.GetSafeAttributeStr(node, "Alternativa_4"),
                    XMLHelper.GetSafeAttributeInt(node, "Alternativa_Resposta"));

                m_dictQuestao.Add(questao.ID, questao);
            }

        }
        catch (Exception ex)
        {
            LoggerHelper.LogException(ex);
            return false;
        }

        return true;
    }
}
