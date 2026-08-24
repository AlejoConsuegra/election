import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ZoneService } from '../../service/zone.service';
import { ZoneRequest } from '../../interfaces/zone.request';

@Component({
  selector: 'app-zone',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard-zone.html',
  styleUrl: './dashboard-zone.css',
})
export class DashboardZone implements OnInit {
  public router = inject(Router);
  private zoneService = inject(ZoneService);

  zonas = signal<any[]>([]);
  isLoading = signal<boolean>(false);
  message = signal<string>('');
  isError = signal<boolean>(false);

  ngOnInit(): void {
    this.loadZonas();
  }

  loadZonas(): void {
    this.isLoading.set(true);
    this.zoneService.getAll().subscribe({
      next: (data: any) => {
        this.zonas.set(data || []);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error cargando zonas:', err);
        this.showMessage('Error al cargar las zonas. Intenta nuevamente.', true);
        this.isLoading.set(false);
      },
    });
  }

  deleteZona(zona: ZoneRequest): void {
    if (
      confirm(
        `¿Estás seguro de eliminar la zona "${zona.nameZona}"? Esta acción no se puede deshacer.`,
      )
    ) {
      this.isLoading.set(true);

      this.zoneService.delete(zona.idZona).subscribe({
        next: () => {
          this.zonas.set(this.zonas().filter((z) => z.idZona !== zona.idZona));

          this.showMessage(`Zona "${zona.nameZona}" eliminada correctamente.`, false);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Error eliminando zona:', err);
          this.showMessage(
            'Error al eliminar la zona. Verifica que no tenga registros asociados.',
            true,
          );
          this.isLoading.set(false);
        },
      });
    }
  }

  private showMessage(text: string, isError: boolean): void {
    this.message.set(text);
    this.isError.set(isError);

    setTimeout(() => {
      this.message.set('');
    }, 4000);
  }
}
