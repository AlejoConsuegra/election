import { Component, inject } from '@angular/core';
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

  cedula: string = '';
  password: string = '';
  errorMessage: string = '';
  isLoading: boolean = false;

  onSubmit(): void {
    this.errorMessage = '';
    this.isLoading = true;

    const credentials: LoginRequest = {
      cedula: this.cedula,
      password: this.password,
    };

    this.authService.login(credentials).subscribe({
      next: (success) => {
        if (!success) {
          this.errorMessage = 'Credenciales inválidas';
          this.isLoading = false;
          return;
        }

        // 1. Cargamos el perfil para llenar la memoria (Signal)
        this.authService.loadProfile().subscribe({
          next: () => {
            this.isLoading = false;
            // 2. Redirección explícita al dashboard tras login exitoso
            this.router.navigate(['/dashboard']);
          },
          error: () => {
            this.isLoading = false;
            this.errorMessage = 'Error al cargar el perfil del usuario.';
          },
        });
      },
      error: (err) => {
        this.isLoading = false;
        if (err.status === 401 || err.status === 403) {
          this.errorMessage = 'Credenciales inválidas';
        } else if (err.status === 429) {
          this.errorMessage = 'Demasiados intentos fallidos.';
        } else {
          this.errorMessage = 'Error de conexión con el servidor';
        }
      },
    });
  }
}
