using Election.CRM.API.Common.DTOs;

namespace Election.CRM.API.Features.User.DTOs;

public class PatchUserRequest : UserBaseDto
{
    // Hereda todas las reglas de validación positiva (Regex, Longitudes) de UserBaseDto.
    // No se redefinen propiedades porque en un PATCH los campos son opcionales (nullable).
}