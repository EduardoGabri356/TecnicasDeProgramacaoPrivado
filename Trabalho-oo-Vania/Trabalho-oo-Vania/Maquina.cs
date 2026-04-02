using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Maquina : Equipamento
{
    public string? Modelo { get; set; }
    public string? HoraOperacao { get; set; }
    public Guid NumeroSerie { get; } = Guid.NewGuid();

    private string? _observacao;
    public string Observacao { set { _observacao = value; } }
}
