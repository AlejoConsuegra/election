import {
  HttpInterceptorFn,
  HttpRequest,
  HttpHandlerFn,
  HttpErrorResponse,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../../features/auth/service/auth.service';

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const authReq = req.clone({
    withCredentials: true,
  });

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        // EXCEPCIÓN CRÍTICA: Si el 401 viene de la verificación de perfil (/me),
        // NO hacemos nada aquí. Dejamos que el método loadProfile() maneje el error silenciosamente.
        if (error.url?.includes('/api/Auth/me')) {
          return throwError(() => error);
        }

        // Para cualquier otro endpoint, limpiamos la sesión y redirigimos.
        authService.clearSession();
        if (!router.url.includes('/login')) {
          router.navigate(['/login']);
        }
      }

      // CRÍTICO: re-lanzar el error para que el componente (create-user) lo reciba y pueda mostrar el mensaje.
      return throwError(() => error);
    }),
  );
};
