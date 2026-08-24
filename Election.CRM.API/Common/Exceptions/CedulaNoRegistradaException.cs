namespace Election.CRM.API.Common.Exceptions;

public class CedulaNoRegistradaException : Exception
{
    public string Cedula { get; }

    public CedulaNoRegistradaException(string cedula)
        : base($"La cédula {cedula} no está registrada en el sistema")
    {
        Cedula = cedula;
    }
}