import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, map, catchError, of } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { UserProfile } from '../../../core/models/user-profile.model';
import { LoginRequest } from '../model/login.request';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  private user = signal<UserProfile | null>(null);
  private isLoading = signal<boolean>(true);

  public readonly user$ = this.user.asReadonly();
  public readonly isLoading$ = this.isLoading.asReadonly();

  public readonly isLoggedIn = computed(() => !!this.user());
  public readonly userRole = computed(() => this.user()?.datos.rol || '');
  public readonly isUserAdmin = computed(() => this.user()?.datos.rol === 'Admin');
  public readonly isUserCoordinator = computed(() => this.user()?.datos.rol === 'Coordinator');
  public readonly isUserLeader = computed(() => this.user()?.datos.rol === 'Lider');

  login(credentials: LoginRequest): Observable<boolean> {
    return this.http
      .post<unknown>(`${this.baseUrl}/api/Auth/login`, credentials)
      .pipe(map(() => true));
  }

  /**
   * Carga el perfil del usuario.
   * NOTA: Este método SOLO actualiza el estado local. NO redirige.
   */
  loadProfile(): Observable<UserProfile | null> {
    this.isLoading.set(true);

    return this.http.get<UserProfile>(`${this.baseUrl}/api/Auth/me`).pipe(
      tap((profile) => {
        this.user.set(profile);
        this.isLoading.set(false);
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
        this.user.set(null);
        this.isLoading.set(false);
      }),
      map(() => true),
      catchError(() => {
        this.user.set(null);
        this.isLoading.set(false);
        return of(false);
      }),
    );
  }

  clearSession(): void {
    this.user.set(null);
    this.isLoading.set(false);
    document.cookie = 'session_active=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
  }
}
