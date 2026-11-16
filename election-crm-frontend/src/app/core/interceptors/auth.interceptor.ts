import {
  HttpInterceptorFn,
  HttpRequest,
  HttpHandlerFn,
  HttpErrorResponse,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../../features/user/service/auth.service';

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // 1. Clonar la petición para forzar el envío de cookies HttpOnly
  // Esta es la línea MÁGICA: le dice al navegador "envía las cookies de este dominio"
  const authReq = req.clone({
    withCredentials: true,
  });

  // 2. Enviar la petición y manejar errores de autenticación
  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Si el backend responde con 401 (Token expirado o revocado en la blacklist)
      if (error.status === 401) {
        // Limpiar el estado reactivo de la aplicación (Signals)
        authService.clearSession();

        // Redirigir al login (evitando bucles infinitos si la petición fallida ya era el login)
        if (!router.url.includes('/login')) {
          router.navigate(['/login']);
        }
      }

      // Propagar el error para que el componente que hizo la petición también pueda manejarlo si lo necesita
      return throwError(() => error);
    }),
  );
};
