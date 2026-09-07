namespace Election.CRM.API.Common.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("Uno o más errores de validación ocurrieron.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Uno o más errores de validación ocurrieron.")
    {
        Errors = errors;
    }
}