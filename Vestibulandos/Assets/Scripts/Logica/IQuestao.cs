public interface IQuestao
{
    int Id { get; set; }
    AreaConhecimento AreaConhecimento { get; set; }
    int Dificuldade { get; set; }
    string Anunciado { get; set; }
    int Resposta { get; set; }
}