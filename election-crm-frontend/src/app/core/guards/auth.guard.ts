import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from '../../features/user/service/auth.service';

/**
 * Guard para rutas protegidas (equivale a PRIVATE_ROUTES + ROLE_ROUTES del middleware de Next.js).
 * Si no hay perfil en memoria, intenta cargarlo desde la cookie HttpOnly.
 * Si la cookie es válida, deja pasar. Si no, redirige al login.
 */
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Caso 1: ya hay perfil cargado (navegación interna). Decidir directo.
  if (authService.loadProfile()) {
    return true;
  }

  // Caso 2: recarga de página en ruta protegida. Consultar /me con la cookie.
  return authService
    .loadProfile()
    .pipe(map((profile) => (profile ? true : router.parseUrl('/login'))));
};
