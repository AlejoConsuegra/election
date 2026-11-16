import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../user/service/auth.service'; // Ajusta la ruta si es necesario
import { LoginRequest } from '../../../user/interfaces/login.request';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private authService = inject(AuthService);

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

        // Le delegamos TODO al AuthService: cargar perfil y redirigir por rol
        this.authService.loadProfile(true).subscribe({
          next: (profile) => {
            this.isLoading = false;
            if (!profile) {
              this.errorMessage = 'No se pudo cargar el perfil del usuario.';
            }
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
