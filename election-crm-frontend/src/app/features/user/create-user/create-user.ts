import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UserRequest } from '../models/user.request';
import { UserService } from '../service/user.service';
import { UserValidators } from './validators/user.validators';
import { extractErrorMessage } from '../../../core/utils/error-handler';

@Component({
  selector: 'app-create-user',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule], // Importante: ReactiveFormsModule
  templateUrl: './create-user.html',
  styleUrl: './create-user.css',
})
export class CreateUser implements OnInit {
  public router = inject(Router);
  private userService = inject(UserService);
  private fb = inject(FormBuilder);

  userForm!: FormGroup;

  readonly message = signal('');
  readonly isError = signal(false);
  readonly isLoading = signal(false);

  ngOnInit(): void {
    this.initForm();
  }

  private initForm(): void {
    this.userForm = this.fb.group({
      nombre: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      telefono: ['', [Validators.required, UserValidators.phoneFormat()]],
      municipio: ['', [Validators.required, Validators.minLength(2)]],
      cedula: ['', [Validators.required, UserValidators.cedulaFormat()]],
      password: ['', [Validators.required, UserValidators.passwordMinLength(12)]], // OWASP: 12 chars
      idRol: [2, [Validators.required]], // Valor por defecto: Líder
    });
  }

  // Helper para el HTML: verifica si hay un error específico y si el campo fue tocado
  hasError(controlName: string, errorType: string): boolean {
    const control = this.userForm.get(controlName);
    return !!(control && control.hasError(errorType) && (control.dirty || control.touched));
  }

  onSubmit(): void {
    this.message.set('');
    this.isError.set(false);

    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      this.message.set('Por favor, corrige los errores resaltados en el formulario.');
      this.isError.set(true);
      return;
    }

    this.isLoading.set(true);
    const payload: UserRequest = this.userForm.value;

    this.userService.create(payload).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.message.set('¡Usuario creado exitosamente!');
        this.isError.set(false);
        this.userForm.reset({ idRol: 2 });
        setTimeout(() => this.router.navigate(['/dashboard']), 1500);
      },
      error: (err: any) => {
        this.isLoading.set(false);
        this.isError.set(true);
        this.message.set(
          extractErrorMessage(err, 'Error al crear el usuario. Verifique los datos.'),
        );
      },
    });
  }
}
