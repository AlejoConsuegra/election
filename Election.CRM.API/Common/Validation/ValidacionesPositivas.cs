using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Election.CRM.API.Common.Validation;

/// <summary>
/// Nombre de persona: solo letras (incluye tildes y ñ) y espacios, entre 3 y 100 caracteres.
/// Los valores nulos se omiten para permitir actualizaciones parciales (PATCH).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class NombreValidoAttribute : ValidationAttribute
{
    private static readonly Regex Expresion = new(
        @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(200));

    public NombreValidoAttribute()
    {
        ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres y solo puede contener letras y espacios.";
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is not string texto) return false;

        return texto.Length >= 3 && texto.Length <= 100 && Expresion.IsMatch(texto);
    }
}

/// <summary>
/// Cédula de ciudadanía: solo dígitos, entre 6 y 12 caracteres.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class CedulaValidaAttribute : ValidationAttribute
{
    private static readonly Regex Expresion = new(
        @"^[0-9]{6,12}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(200));

    public CedulaValidaAttribute()
    {
        ErrorMessage = "La cédula debe contener solo números y tener entre 6 y 12 dígitos.";
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is not string texto) return false;

        return Expresion.IsMatch(texto);
    }
}

/// <summary>
/// Teléfono móvil: exactamente 10 dígitos numéricos.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class TelefonoValidoAttribute : ValidationAttribute
{
    private static readonly Regex Expresion = new(
        @"^[0-9]{10}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(200));

    public TelefonoValidoAttribute()
    {
        ErrorMessage = "El teléfono debe tener exactamente 10 dígitos numéricos.";
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is not string texto) return false;

        return Expresion.IsMatch(texto);
    }
}

/// <summary>
/// Municipio: solo letras (incluye tildes y ñ) y espacios, entre 2 y 100 caracteres.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class MunicipioValidoAttribute : ValidationAttribute
{
    private static readonly Regex Expresion = new(
        @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(200));

    public MunicipioValidoAttribute()
    {
        ErrorMessage = "El municipio debe tener entre 2 y 100 caracteres y solo puede contener letras y espacios.";
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is not string texto) return false;

        return texto.Length >= 2 && texto.Length <= 100 && Expresion.IsMatch(texto);
    }
}

/// <summary>
/// Correo electrónico con formato válido y longitud máxima de 150 caracteres.
/// Reutiliza el validador de formato del framework para no duplicar la regla RFC.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class EmailValidoAttribute : ValidationAttribute
{
    private static readonly EmailAddressAttribute ValidadorFormato = new();

    public EmailValidoAttribute()
    {
        ErrorMessage = "El email debe tener un formato válido y no exceder 150 caracteres.";
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is not string texto) return false;

        return texto.Length <= 150 && ValidadorFormato.IsValid(texto);
    }
}

/// <summary>
/// Rol asignable desde la API: únicamente Líder (2) y Coordinador (3).
/// El rol Admin (1) se gestiona por seed y nunca puede asignarse por petición.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class RolAsignableAttribute : ValidationAttribute
{
    private const int RolLider = 2;
    private const int RolCoordinator = 3;

    public RolAsignableAttribute()
    {
        ErrorMessage = "El valor seleccionado para el rol no es válido";
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is not int rol) return false;

        return rol == RolLider || rol == RolCoordinator;
    }
}

    /// <summary>
    /// Dirección de residencia. Admite letras, dígitos, espacios y los símbolos
    /// propios de la nomenclatura vial colombiana (# . , / -).
    /// Ejemplos válidos: "Calle 45 # 12-34", "Cra 7B Bis # 45-67 Sur".
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class DireccionValidaAttribute : ValidationAttribute
    {
        private static readonly Regex Expresion = new(
            @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ0-9\s#.,/\-]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(200));

        public DireccionValidaAttribute()
        {
            ErrorMessage = "La dirección debe tener entre 5 y 255 caracteres y solo admite letras, números, espacios y los símbolos # . , / -";
        }

        public override bool IsValid(object? value)
        {
            if (value is null) return true;
            if (value is not string texto) return false;

            return texto.Length >= 5 && texto.Length <= 255 && Expresion.IsMatch(texto);
        }
    }

    /// <summary>
    /// Barrio, vereda o urbanización. Admite letras, dígitos, espacios y los
    /// símbolos . , - para casos como "13 de Mayo" o "Villa del Río - Sector 2".
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class BarrioValidoAttribute : ValidationAttribute
    {
        private static readonly Regex Expresion = new(
            @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ0-9\s.,\-]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(200));

        public BarrioValidoAttribute()
        {
            ErrorMessage = "El barrio debe tener entre 2 y 255 caracteres y solo admite letras, números, espacios y los símbolos . , -";
        }

        public override bool IsValid(object? value)
        {
            if (value is null) return true;
            if (value is not string texto) return false;

            return texto.Length >= 2 && texto.Length <= 255 && Expresion.IsMatch(texto);
        }
    }

    /// <summary>
    /// Puesto de votación. Admite letras, dígitos, espacios y los símbolos # . , / -
    /// para casos como "IE Distrital La Esperanza - Sede B" o "Colegio Nacional #3".
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class PuestoValidoAttribute : ValidationAttribute
    {
        private static readonly Regex Expresion = new(
            @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ0-9\s#.,/\-]+$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(200));

        public PuestoValidoAttribute()
        {
            ErrorMessage = "El puesto debe tener entre 3 y 255 caracteres y solo admite letras, números, espacios y los símbolos # . , / -";
        }

        public override bool IsValid(object? value)
        {
            if (value is null) return true;
            if (value is not string texto) return false;

            return texto.Length >= 3 && texto.Length <= 255 && Expresion.IsMatch(texto);
        }
    }