import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, map, catchError, of } from 'rxjs';
import { LoginRequest } from '../interfaces/login.request';
import { environment } from '../../../../environments/environment';
import { UserProfile } from '../../../core/models/user-profile.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private baseUrl = environment.apiUrl;

  private user = signal<UserProfile | null>(null);
  private isLoading = signal<boolean>(true);

  public readonly user$ = this.user.asReadonly();
  public readonly isLoading$ = this.isLoading.asReadonly();

  public readonly isLoggedIn = computed(() => !!this.user());
  public readonly userRole = computed(() => this.user()?.datos.rol || '');
  public readonly isUserAdmin = computed(() => this.user()?.datos.rol === 'Admin');
  public readonly isUserCoordinator = computed(() => this.user()?.datos.rol === 'Coordinador');
  public readonly isUserLeader = computed(() => this.user()?.datos.rol === 'Líder');

  login(credentials: LoginRequest): Observable<boolean> {
    return this.http.post<unknown>(`${this.baseUrl}/api/Auth/login`, credentials).pipe(
      map(() => true),
      catchError(() => of(false)),
    );
  }

  /**
   * Carga el perfil del usuario.
   * @param forceRedirect Si es true, redirige a la ruta por defecto del rol sin importar dónde esté.
   */
  loadProfile(forceRedirect: boolean = false): Observable<UserProfile | null> {
    // Si ya está cargado, solo redirigimos si se fuerza y estamos en una ruta pública
    if (this.user()) {
      if (forceRedirect && (this.router.url === '/login' || this.router.url === '/')) {
        this.navigateToDefaultRoute();
      }
      return of(this.user());
    }

    this.isLoading.set(true);

    return this.http.get<UserProfile>(`${this.baseUrl}/api/Auth/me`).pipe(
      tap((profile) => {
        this.user.set(profile);
        this.isLoading.set(false);

        // Redirección centralizada: si venimos del login (forceRedirect) o estamos en una ruta pública
        if (forceRedirect || this.router.url === '/login' || this.router.url === '/') {
          this.navigateToDefaultRoute();
        }
      }),
      catchError(() => {
        this.user.set(null);
        this.isLoading.set(false);
        return of(null);
      }),
    );
  }

  logout(): Observable<boolean> {
    return this.http.post<unknown>(`${this.baseUrl}/api/Auth/logout`, {}).pipe(
      tap(() => {
        // Limpiamos el estado reactivo local
        this.user.set(null);
        this.isLoading.set(false);
      }),
      map(() => true),
      catchError(() => {
        // Incluso si falla, limpiamos el estado local
        this.user.set(null);
        this.isLoading.set(false);
        return of(false);
      }),
    );
  }

  clearSession(): void {
    this.user.set(null);
    this.isLoading.set(false);
  }

  /**
   * Lógica centralizada de enrutamiento por rol.
   */
  private navigateToDefaultRoute(): void {
    const rol = this.user()?.datos.rol;
    let route = '/login';

    switch (rol) {
      case 'Admin':
        route = '/dashboard';
        break;
      case 'Coordinador':
        route = '/dashboard';
        break;
      case 'Líder':
        route = '/dashboard';
        break;
    }

    this.router.navigate([route]);
  }
}
