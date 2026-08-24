import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../user/service/auth.service';
import { LoginRequest } from '../../../user/interfaces/login.request';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  public router = inject(Router);
  private authService = inject(AuthService);

  cedula: string = '';
  password: string = '';
  errorMessage: string = '';

  onSubmit(): void {
    this.errorMessage = '';

    const credentials: LoginRequest = {
      cedula: this.cedula,
      password: this.password,
    };

    this.authService.login(credentials).subscribe({
      next: (response) => {
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        console.error('Error en login:', err);
        if (err.status === 401 || err.status === 403) {
          this.errorMessage = 'Credenciales inválidas';
        } else {
          this.errorMessage = 'Error de conexión con el servidor';
        }
      },
    });
  }
}
