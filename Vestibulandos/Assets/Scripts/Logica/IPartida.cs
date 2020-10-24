using System;
using System.Collections;
using System.Collections.Generic;

public interface IPartida
{
    int Id { get; set; }
    TipoPartida TipoPartida { get; set; }
    DateTime DataCriado { get; set; }
    DateTime DataFinalizado { get; set; }
    int JogadorCriado { get; set; }
    int Dificuldade { get; set; }

    AreaConhecimento AreaConhecimento { get; set; }

    List<IQuestao> Questao { get; set; }
    int QuantidadeQuestao { get; set; }
    bool Revisao { get; set; }
}
