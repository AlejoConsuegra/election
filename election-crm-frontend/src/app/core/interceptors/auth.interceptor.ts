import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  // 1. Obtener el token del almacenamiento (ajusta a sessionStorage si lo usas ahí)
  const token = localStorage.getItem('token');

  // 2. Clonar la petición y agregar el header de autorización si el token existe
  let authReq = req;
  if (token) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  // 3. Enviar la petición modificada y capturar errores
  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Si el backend responde con 401 (No autorizado / Token vencido)
      if (error.status === 401) {
        // Limpiar el almacenamiento local
        localStorage.removeItem('token');
        localStorage.removeItem('user'); // Si guardas el objeto del usuario, límpialo también

        // Redirigir al login
        router.navigate(['/login']);
      }

      return throwError(() => error);
    }),
  );
};
