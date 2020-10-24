public class QuestaoData
{
    public int ID { get; set; }
    public TipoQuestao Tipo { get; set; }
    public string Anunciado { get; set; }
    public int NumeroAlternativas { get; set; }
    public string Alternativa0 { get; set; }
    public string Alternativa1 { get; set; }
    public string Alternativa2 { get; set; }
    public string Alternativa3 { get; set; }
    public string Alternativa4 { get; set; }
    public int AlternativaResposta { get; set; }

    public QuestaoData(int id, TipoQuestao tipo, string anunciado, int numeroAlternativas, 
        string alternativa0, string alternativa1, string alternativa2, string alternativa3, string alternativa4,
        int alterantivaResposta)
    {
        ID = id;
        Tipo = tipo;
        Anunciado = anunciado;
        NumeroAlternativas = numeroAlternativas;
        Alternativa0 = alternativa0;
        Alternativa1 = alternativa1;
        Alternativa2 = alternativa2;
        Alternativa3 = alternativa3;
        Alternativa4 = alternativa4;
        AlternativaResposta = alterantivaResposta;
    }
}
