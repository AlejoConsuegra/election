import { Component, OnDestroy, input, signal } from '@angular/core';
import { KpiCard } from '../../models/kpi-card.model';

@Component({
  selector: 'app-kpi-cards',
  standalone: true,
  templateUrl: './kpi-cards.html',
})
export class KpiCards implements OnDestroy {
  readonly kpiCards = input.required<KpiCard[]>();
  readonly isLoading = input<boolean>(false);

  protected readonly display = signal<number[]>([]);

  private raf: number | null = null;
  private nf = new Intl.NumberFormat('es-CO');

  ngOnChanges(): void {
    this.animateTo(this.kpiCards().map((c) => c.value));
  }

  ngOnDestroy(): void {
    if (this.raf !== null) cancelAnimationFrame(this.raf);
  }

  protected fmt(n: number): string {
    return this.nf.format(Math.round(n || 0));
  }

  protected valueAt(i: number, fallback: number): number {
    return this.display()[i] ?? fallback;
  }

  private animateTo(targets: number[]): void {
    if (this.raf !== null) {
      cancelAnimationFrame(this.raf);
      this.raf = null;
    }

    const reduce =
      typeof window !== 'undefined' &&
      window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    if (reduce || targets.length === 0) {
      this.display.set(targets);
      return;
    }

    const from = this.display();
    const start = performance.now();
    const duration = 850;

    const step = (now: number): void => {
      const t = Math.min(1, (now - start) / duration);
      const eased = 1 - Math.pow(1 - t, 3);

      this.display.set(
        targets.map((to, i) => {
          const origin = from[i] ?? 0;
          return Math.round(origin + (to - origin) * eased);
        }),
      );

      this.raf = t < 1 ? requestAnimationFrame(step) : null;
    };

    this.raf = requestAnimationFrame(step);
  }
}
