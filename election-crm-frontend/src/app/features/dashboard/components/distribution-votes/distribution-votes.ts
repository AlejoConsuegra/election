import { Component, computed, input } from '@angular/core';
import { PieSlice } from '../../models/pie-slice.model';

@Component({
  selector: 'app-distribution-votes',
  standalone: true,
  templateUrl: './distribution-votes.html',
  host: { class: 'block h-full' },
})
export class DistributionVotes {
  readonly pieSlices = input.required<PieSlice[]>();

  protected readonly pieTotal = computed(() => this.pieSlices().reduce((a, s) => a + s.value, 0));

  private nf = new Intl.NumberFormat('es-CO');
  protected fmt(n: number): string {
    return this.nf.format(Math.round(n || 0));
  }
}
