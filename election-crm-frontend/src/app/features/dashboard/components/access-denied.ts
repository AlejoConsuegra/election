import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../auth/service/auth.service';

@Component({
  selector: 'app-access-denied',
  standalone: true,
  template: `
    <div class="flex items-center justify-center min-h-screen bg-gray-50">
      <div class="bg-white p-10 rounded-lg shadow-md text-center max-w-md">
        <div class="text-6xl mb-4">🚧</div>
        <h2 class="text-2xl font-bold text-gray-800 mb-2">Módulo en construcción</h2>
        <p class="text-gray-600 mb-6">Esta sección está disponible próximamente para tu rol.</p>
        <button
          (click)="goBack()"
          class="bg-[#0B2545] hover:bg-[#1E5AA8] text-white font-semibold py-2 px-6 rounded-lg transition-colors"
        >
          Volver al inicio
        </button>
      </div>
    </div>
  `,
})
export class AccessDeniedComponent {
  private router = inject(Router);
  private auth = inject(AuthService);

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }
}
