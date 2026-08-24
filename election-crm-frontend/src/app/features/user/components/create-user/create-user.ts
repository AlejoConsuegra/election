import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { UserRequest } from '../../interfaces/user.request';
import { UserService } from '../../service/user.service';

@Component({
  selector: 'app-create-user',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './create-user.html',
  styleUrl: './create-user.css',
})
export class CreateUser {
  public router = inject(Router);
  private userService = inject(UserService);

  nombre: string = '';
  email: string = '';
  telefono: string = '';
  municipio: string = '';
  cedula: string = '';
  password: string = '';
  idRol: number = 2;

  message: string = '';
  isError: boolean = false;

  onSubmit(): void {
    this.message = '';
    this.isError = false;

    const payload: UserRequest = {
      nombre: this.nombre,
      email: this.email,
      telefono: this.telefono,
      municipio: this.municipio,
      cedula: this.cedula,
      password: this.password,
      idRol: this.idRol,
    };

    console.log(payload);

    this.userService.add(payload).subscribe({
      next: (data: any) => {
        this.message = '¡Usuario creado exitosamente!';

        this.nombre = '';
        this.email = '';
        this.telefono = '';
        this.municipio = '';
        this.cedula = '';
        this.password = '';
      },
      error: (err) => {
        this.isError = true;
        this.message =
          err.error?.message ||
          'Error al crear el usuario. Verifica que la cédula o email no existan.';
      },
    });
  }
}
