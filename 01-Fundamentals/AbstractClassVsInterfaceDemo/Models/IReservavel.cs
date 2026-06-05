namespace AbstractClassVsInterfaceDemo.Models;

public interface IReservavel
{
    string Nome { get; }

    bool EstaReservado { get; }

    void ReservarPara(string responsavel);

    string ObterStatusReserva();
}
