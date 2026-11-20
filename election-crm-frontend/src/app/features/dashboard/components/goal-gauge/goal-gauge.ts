import { Component, input } from '@angular/core';
import { GaugeStat, GaugeTick } from '../../models/gauge.model';

@Component({
  selector: 'app-goal-gauge',
  standalone: true,
  templateUrl: './goal-gauge.html',
  host: { class: 'block h-full' },
})
export class GoalGauge {
  readonly ticks = input.required<GaugeTick[]>();
  readonly percent = input.required<number>();
  readonly votes = input.required<number>();
  readonly census = input.required<number>();
  readonly stats = input.required<GaugeStat[]>();

  private nf = new Intl.NumberFormat('es-CO');

  protected fmt(n: number): string {
    return this.nf.format(Math.round(n || 0));
  }

  protected tickDelay(i: number): number {
    return i * 12;
  }
}
