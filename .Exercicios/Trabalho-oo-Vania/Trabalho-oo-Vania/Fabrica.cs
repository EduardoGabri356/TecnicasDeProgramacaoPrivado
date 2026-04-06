using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
Pseudocódigo detalhado (plano):
1. Garantir que o método BuscarMaquinaPorModelo sempre retorne um valor em todos os caminhos:
   - Se encontrar uma máquina cujo modelo corresponde ao parâmetro, retornar essa instância.
   - Se não encontrar, retornar null.
2. Como vamos acessar a coleção de máquinas da instância (`Maquinas`), o método permanece de instância
   (resolvendo o aviso CA1822).
3. Permitir o retorno de null explicitamente alterando o tipo de retorno para `Maquina?`.
4. Implementar comparação de string de forma segura (tratar null no parâmetro e comparar com StringComparison).
5. Manter o restante da classe inalterado para não introduzir mudanças não solicitadas.

Implementação:
- Alterar assinatura: `public Maquina? BuscarMaquinaPorModelo(string modelo)`
- Implementar loop sobre `Maquinas` e comparação `string.Equals(m.Modelo, modelo, StringComparison.OrdinalIgnoreCase)`
- Retornar `m` quando houver correspondência; se não, `return null;`
*/

public class Fabrica
{
    public Fabrica(string nomeFabrica, string modelo, string horaOperacao, string obs, string nomeMaquina, DateTime dataFabricacaoMaquina) : base()
    {
        Nome = nomeFabrica;
        Maquinas.Add(new Maquina
        {
            Nome = nomeMaquina,
            Modelo = nomeMaquina,
            HoraOperacao = horaOperacao,
            Observacao = obs,
            DataFabricacao = dataFabricacaoMaquina,
        });
    }

    public string? Nome { get; set; }
    public ICollection<Maquina> Maquinas { get; set; } = new List<Maquina>();

    public void AdicionarMaquina(string modelo, string horaOperacao, string obs, string nome, DateTime dataFabricacaoMaquina)
    {

    }

    public void ListarMaquinas()
    {
        Console.WriteLine($"\nFábrica: {Nome}");
        foreach (var m in Maquinas)
        {
            Console.WriteLine($"nome: {m.Nome} | Modelo: {m.Modelo} | Fabricação: {m.DataFabricacao:dd/MM/yyyy} | Nº Série: {m.NumeroSerie} Horas");
        }
    }

    public Maquina? BuscarMaquinaPorModelo(string modelo)
    {
        if (modelo == null)
            return null;

        foreach (var m in Maquinas)
        {
            if (string.Equals(m.Modelo, modelo, StringComparison.OrdinalIgnoreCase))
            {
                return m;
            }
        }

        return null;
    }
}

