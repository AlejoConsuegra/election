import { Component, computed, input } from '@angular/core';
import { SourceRow } from '../../models/source-row.model';

@Component({
  selector: 'app-origin-records',
  standalone: true,
  templateUrl: './origin-records.html',
  host: { class: 'block h-full' },
})
export class OriginRecords {
  readonly sources = input.required<SourceRow[]>();

  protected readonly sourcesTotal = computed(() => this.sources().reduce((a, s) => a + s.value, 0));

  private nf = new Intl.NumberFormat('es-CO');
  protected fmt(n: number): string {
    return this.nf.format(Math.round(n || 0));
  }

  protected trendColor(delta: number): string {
    return delta >= 0 ? '#22C55E' : '#EF4444';
  }
}
