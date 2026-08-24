import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const token = localStorage.getItem('token');

  if (token) {
    // OPCIONAL: Si deseas validar si expiró sin esperar respuesta del servidor,
    // puedes decodificar la fecha de expiración del JWT (claim 'exp').
    return true;
  }

  // Si no hay token, redirigir al login
  router.navigate(['/login']);
  return false;
};
