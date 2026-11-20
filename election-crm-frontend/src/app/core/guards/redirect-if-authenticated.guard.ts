import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../../features/auth/service/auth.service';
import { map, catchError, of } from 'rxjs';

export const redirectIfAuthenticatedGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // 1. Verificación instantánea por memoria (Navegación interna)
  if (authService.isLoggedIn()) {
    router.navigate(['/dashboard']);
    return false;
  }

  // 2. Verificación por cookie de estado (Réplica de middleware tipo Next.js)
  const hasSession = document.cookie.includes('session_active=true');

  if (!hasSession) {
    return true; // No hay sesión, mostrar login. CERO llamadas al backend.
  }

  // 3. Verificación por backend (Si el usuario hizo F5 en /login)
  return authService.loadProfile().pipe(
    map((profile) => {
      if (profile) {
        // ¡Hay sesión válida! Redirigimos activamente al dashboard y cancelamos la carga de /login
        router.navigate(['/dashboard']);
        return false;
      }
      // No hay sesión válida, permitir ver el login
      return true;
    }),
    catchError(() => {
      // Si el backend respondió 401, permitir ver el login
      return of(true);
    }),
  );
};
