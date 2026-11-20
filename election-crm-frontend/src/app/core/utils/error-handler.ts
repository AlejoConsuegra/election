import { HttpErrorResponse } from '@angular/common/http';

export function extractErrorMessage(
  err: HttpErrorResponse,
  fallback = 'Ocurrió un error inesperado.',
): string {
  if (!err || !err.error) return fallback;

  // PRIORIDAD 1: Mensaje específico de un campo (viene en 'extra' como diccionario)
  // Funciona tanto para ModelState como para FluentValidation ahora unificados.
  if (err.error.extra && typeof err.error.extra === 'object') {
    const firstKey = Object.keys(err.error.extra)[0];
    const messages = err.error.extra[firstKey];
    if (Array.isArray(messages) && messages.length > 0) {
      return messages[0]; // Ej: "El rol debe ser Líder (2) o Coordinador (3)..."
    }
  }

  // PRIORIDAD 2: Mensaje general de negocio (ej: "Ya existe un usuario con esa cédula")
  if (err.error.message && typeof err.error.message === 'string') {
    return err.error.message;
  }

  // PRIORIDAD 3: Fallbacks de red o servidor
  if (err.status === 0) return 'No hay conexión con el servidor.';
  if (err.status === 401) return 'Su sesión ha expirado. Ingrese nuevamente.';
  if (err.status === 403) return 'No tiene permisos para ejecutar esta operación.';

  return fallback;
}
