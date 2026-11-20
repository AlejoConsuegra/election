import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard-header.html',
})
export class DashboardHeader {
  // ═══ INPUTS: Datos que recibe del padre ═══
  readonly userName = input.required<string>();
  readonly userRole = input.required<string>();
  readonly lastUpdate = input.required<Date>();
  readonly demoMode = input<boolean>(false);
  readonly liveMode = input<boolean>(false);
  readonly isLoading = input<boolean>(false);

  // ═══ OUTPUTS: Eventos que emite al padre ═══
  readonly toggleLive = output<void>();
  readonly refresh = output<void>();
  readonly exportCsv = output<void>();

  // ═══ UTILIDAD LOCAL ═══
  // Mantenemos esto aquí para que el componente sea 100% autónomo
  timeLabel(d: Date): string {
    return d.toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit' });
  }
}
