import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-error-banner',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './error-banner.html',
})
export class ErrorBanner {
  // ═══ INPUT: El mensaje de error que viene del padre ═══
  readonly loadError = input.required<string>();

  // ═══ OUTPUT: Avisa al padre cuando el usuario hace clic en "Reintentar" ═══
  readonly refresh = output<void>();
}
