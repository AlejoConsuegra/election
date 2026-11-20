import { Component, OnDestroy, computed, input, output, signal } from '@angular/core';
import { MonthPoint } from '../../models/month-point.model';
import { MuniRow } from '../../models/muni-row.model';

@Component({
  selector: 'app-top-zone',
  standalone: true,
  templateUrl: './top-zone.html',
  host: { class: 'block h-full' },
})
export class TopZone {
  readonly topMunicipios = input.required<MuniRow[]>();
  readonly goZones = output<void>();

  private nf = new Intl.NumberFormat('es-CO');
  protected fmt(n: number): string {
    return this.nf.format(Math.round(n || 0));
  }

  protected trendColor(delta: number): string {
    return delta >= 0 ? '#22C55E' : '#EF4444';
  }

  protected readonly maxVotes = computed(() =>
    this.topMunicipios().reduce((a, r) => Math.max(a, r.votos), 0),
  );

  private readonly grown = signal(false);
  private timer: ReturnType<typeof setTimeout> | null = null;

  ngOnChanges(): void {
    if (this.topMunicipios().length > 0 && !this.grown()) {
      if (this.timer) clearTimeout(this.timer);
      this.timer = setTimeout(() => this.grown.set(true), 260);
    }
  }

  ngOnDestroy(): void {
    if (this.timer) clearTimeout(this.timer);
  }

  protected share(votos: number): number {
    if (!this.grown()) return 0;
    const max = this.maxVotes();
    return max > 0 ? Math.max(3, (votos / max) * 100) : 0;
  }
}
