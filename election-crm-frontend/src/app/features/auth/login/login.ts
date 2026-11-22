import { Component, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { LoginRequest } from '../model/login.request';
import { AuthService } from '../service/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private authService = inject(AuthService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  cedula: string = '';
  password: string = '';
  errorMessage: string = '';
  isLoading: boolean = false;

  onSubmit(): void {
    this.errorMessage = '';
    this.isLoading = true;
    this.cdr.detectChanges();

    const credentials: LoginRequest = {
      cedula: this.cedula,
      password: this.password,
    };

    this.authService.login(credentials).subscribe({
      next: () => {
        this.authService.loadProfile().subscribe({
          next: (profile) => {
            this.isLoading = false;
            if (!profile) {
              this.errorMessage = 'No se pudo cargar el perfil. Intente nuevamente.';
              this.cdr.detectChanges();
              return;
            }
            this.router.navigate(['/dashboard']);
          },
          error: () => {
            this.isLoading = false;
            this.errorMessage = 'Error al cargar el perfil del usuario.';
            this.cdr.detectChanges();
          },
        });
      },
      error: (err) => {
        this.isLoading = false;
        if (err.status === 400 || err.status === 401 || err.status === 403) {
          this.errorMessage = err.error?.message || 'Credenciales inválidas';
        } else if (err.status === 429) {
          this.errorMessage = err.error?.message || 'Demasiados intentos fallidos.';
        } else {
          this.errorMessage = 'Error de conexión con el servidor';
        }
        this.cdr.detectChanges();
      },
    });
  }
}
