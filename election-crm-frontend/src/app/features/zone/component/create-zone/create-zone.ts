import { ChangeDetectorRef, Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ZoneRequest } from '../../interfaces/zone.request';
import { ZoneService } from '../../service/zone.service';
import { DashboardZone } from '../dashboard-zone/dashboard-zone';

@Component({
  selector: 'app-create-zone',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './create-zone.html',
  styleUrl: './create-zone.css',
})
export class CreateZone {
  public router = inject(Router);
  private zoneService = inject(ZoneService);
  private route = inject(ActivatedRoute);

  nameZona = signal<string>('');
  message = signal<string>('');
  zonaId = signal<number | null>(null);
  isEditMode = signal<boolean>(false);
  isError = signal<boolean>(false);
  isLoading = signal<boolean>(false);

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam) {
      this.isEditMode.set(true);
      const id = Number(idParam);
      this.zonaId.set(id);
      this.loadZonaData(id);
    } else {
      this.isEditMode.set(false);
      this.zonaId.set(null);
    }
  }

  loadZonaData(id: number): void {
    this.isLoading.set(true);

    this.zoneService.getById(id).subscribe({
      next: (zona) => {
        this.nameZona.set(zona.nameZona);
        this.isLoading.set(false);
      },
      error: () => {
        this.showMessage('Error al cargar los datos de la zona.', true);
        this.isLoading.set(false);
      },
    });
  }

  onSubmit(): void {
    if (!this.nameZona().trim()) {
      this.showMessage('El nombre de la zona es obligatorio.', true);
      return;
    }

    this.isLoading.set(true);
    const payload = { nameZona: this.nameZona() };

    if (this.isEditMode() && this.zonaId()) {
      this.zoneService.update(this.zonaId()!, payload).subscribe({
        next: () => {
          this.showMessage('Zona actualizada correctamente.', false);
          setTimeout(() => this.router.navigate(['/zonas']), 1500);
        },
        error: () => {
          this.showMessage('Error al actualizar la zona.', true);
          this.isLoading.set(false);
        },
      });
    } else {
      this.zoneService.create(payload).subscribe({
        next: () => {
          this.showMessage('Zona creada correctamente.', false);
          setTimeout(() => this.router.navigate(['/zonas']), 1500);
        },
        error: () => {
          this.showMessage('Error al crear la zona.', true);
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
