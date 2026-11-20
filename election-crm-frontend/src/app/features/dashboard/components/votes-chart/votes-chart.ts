import { Component, OnDestroy, computed, input, output, signal } from '@angular/core';
import { MonthPoint } from '../../models/month-point.model';

@Component({
  selector: 'app-votes-chart',
  standalone: true,
  templateUrl: './votes-chart.html',
})
export class VotesChart implements OnDestroy {
  readonly months = input.required<MonthPoint[]>();
  readonly ticks = input.required<number[]>();
  readonly max = input.required<number>();

  readonly createClick = output<void>();

  protected readonly hoverMonth = signal<number | null>(null);
  protected readonly chartEmpty = computed(() => this.months().length === 0);

  private readonly grown = signal(false);
  private timer: ReturnType<typeof setTimeout> | null = null;
  private nf = new Intl.NumberFormat('es-CO');

  ngOnChanges(): void {
    if (this.months().length > 0 && !this.grown()) {
      if (this.timer) clearTimeout(this.timer);
      this.timer = setTimeout(() => this.grown.set(true), 90);
    }
  }

  ngOnDestroy(): void {
    if (this.timer) clearTimeout(this.timer);
  }

  protected fmt(n: number): string {
    return this.nf.format(Math.round(n || 0));
  }

  protected barHeight(v: number): number {
    if (!this.grown()) return 0;
    const m = this.max();
    return m > 0 ? Math.max(2, (v / m) * 100) : 0;
  }

  protected gridTop(i: number): number {
    const len = this.ticks().length;
    return len > 1 ? (i / (len - 1)) * 100 : 0;
  }
}
