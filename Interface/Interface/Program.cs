IAnimal animal1 = new Cachorro();

IAnimal animal2 = new Gato();

IAnimal animal67 = new KungFuuWhatsapp();

animal1.FazerSom();
//Não da para adicionar mais de uma caracteristica sem instanciar a classe

animal2.FazerSom();

Cachorro animal3 = new Cachorro();
animal3.QuantidadePatas();
animal3.FazerSom();

animal67.FazerSom();


public interface IAnimal
{
    public void FazerSom();
}

public class Cachorro : IAnimal
{
    public void FazerSom()
    {
        Console.WriteLine("AUAUAUAUAUAUAUAU");
    }

    public void QuantidadePatas()
    {
        Console.WriteLine("Tem 4 patas");
    }
}

public class Gato : IAnimal
{
    public void FazerSom()
    {
        Console.WriteLine("MIAAAAAAAAAAAAUU");
    }
}

public class KungFuuWhatsapp : IAnimal
{
    public void FazerSom()
    {
        Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}