try
{
    A.ExecutandoA();
}
catch (Exception ex)
{
    Console.WriteLine($"A execção foi tratada na chamada de A: {ex.Message}");
}

class A{
    public static void ExecutandoA(){
        //try
        //{
            B.ExecutandoB();
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine($"A execção foi tratada em A: {ex.Message}");
        //}
    }
}


class B{
    public static void ExecutandoB(){
        //try
        //{
            C.ExecutandoC();
        //}
        //catch (Exception ex)
        //{
        //   Console.WriteLine($"A execção foi tratada em B: {ex.Message}");
        //}
    }
}

class C{
    public static void ExecutandoC(){
        throw new NotImplementedException("O método não foi implementado em C");
    }
}