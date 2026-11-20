import { Component, HostListener, computed, effect, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LeaderRow } from '../../models/leader-row.model';

type SortKey = 'nombre' | 'municipio' | 'votos' | 'rol';

@Component({
  selector: 'app-leaders-table',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './leaders-table.html',
  host: { class: 'block' },
})
export class LeadersTable {
  constructor() {
    effect(() => {
      const leaders = this.leaders();
      this.page.set(1);
    });
  }
  // ═══ INPUTS ═══
  readonly leaders = input.required<LeaderRow[]>();
  readonly isLoading = input<boolean>(false);
  readonly votes = input<number>(0);
  readonly census = input<number>(0);

  // ═══ OUTPUTS ═══
  readonly createClick = output<void>();
  readonly detailClick = output<LeaderRow>();

  // ═══ ESTADO LOCAL DE UI ═══
  readonly search = signal('');
  readonly rolFilter = signal('todos');
  readonly sortKey = signal<SortKey>('votos');
  readonly sortDir = signal<'asc' | 'desc'>('desc');
  readonly page = signal(1);
  readonly showAll = signal(false);
  protected readonly selected = signal<LeaderRow | null>(null);

  // ═══ COMPUTED ═══
  protected readonly roles = computed(() => {
    const set = new Set(
      this.leaders()
        .map((l) => l.rol)
        .filter(Boolean),
    );
    return ['todos', ...Array.from(set).sort()];
  });

  protected readonly filtered = computed(() => {
    const q = this.search().trim().toLowerCase();
    const rol = this.rolFilter();
    return this.leaders().filter((l) => {
      const okRol = rol === 'todos' || l.rol === rol;
      const okQ =
        !q ||
        l.nombre.toLowerCase().includes(q) ||
        l.email.toLowerCase().includes(q) ||
        l.municipio.toLowerCase().includes(q) ||
        l.zona.toLowerCase().includes(q) ||
        l.cedula.includes(q);
      return okRol && okQ;
    });
  });

  protected readonly sorted = computed(() => {
    const key = this.sortKey();
    const dir = this.sortDir() === 'asc' ? 1 : -1;
    return [...this.filtered()].sort((a, b) => {
      if (key === 'votos') return (a.votos - b.votos) * dir;
      return String(a[key]).localeCompare(String(b[key]), 'es') * dir;
    });
  });

  protected readonly totalPages = computed(() =>
    this.showAll() ? 1 : Math.max(1, Math.ceil(this.sorted().length / 6)),
  );

  protected readonly pages = computed(() =>
    Array.from({ length: this.totalPages() }, (_, i) => i + 1),
  );

  protected readonly pagedLeaders = computed(() => {
    const all = this.sorted();
    if (this.showAll()) return all;
    const p = Math.min(this.page(), this.totalPages());
    return all.slice((p - 1) * 6, p * 6);
  });

  // ═══ CICLO DE VIDA ═══
  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.selected()) this.selected.set(null);
  }

  // ═══ MÉTODOS ═══
  protected setSearch(v: string): void {
    this.search.set(v);
    this.page.set(1);
  }

  protected setRol(v: string): void {
    this.rolFilter.set(v);
    this.page.set(1);
  }

  protected clearFilters(): void {
    this.search.set('');
    this.rolFilter.set('todos');
    this.page.set(1);
  }

  protected toggleSort(key: SortKey): void {
    if (this.sortKey() === key) {
      this.sortDir.set(this.sortDir() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortKey.set(key);
      this.sortDir.set(key === 'votos' ? 'desc' : 'asc');
    }
    this.page.set(1);
  }

  protected setPage(p: number): void {
    this.page.set(Math.min(Math.max(1, p), this.totalPages()));
  }

  protected toggleShowAll(): void {
    this.showAll.set(!this.showAll());
    this.page.set(1);
  }

  protected openDetail(l: LeaderRow): void {
    this.selected.set(l);
  }

  protected closeDetail(): void {
    this.selected.set(null);
  }

  protected goCreate(): void {
    this.createClick.emit();
  }

  protected rowIndex(i: number): number {
    return (this.showAll() ? i : (this.page() - 1) * 6 + i) + 1;
  }

  protected shareOfZone(votos: number): string {
    const v = this.votes();
    return v > 0 ? ((votos / v) * 100).toFixed(1) : '0.0';
  }

  protected shareOfCensus(votos: number): number {
    const c = this.census();
    return c > 0 ? (votos / c) * 100 : 0;
  }

  protected fmt(n: number): string {
    return new Intl.NumberFormat('es-CO').format(Math.round(n || 0));
  }

  protected initials(name: string): string {
    const p = name.trim().split(/\s+/);
    return ((p[0]?.[0] ?? '') + (p[1]?.[0] ?? '')).toUpperCase() || 'U';
  }

  protected dateLabel(iso: string): string {
    const d = new Date(iso);
    return isNaN(d.getTime())
      ? '—'
      : d.toLocaleDateString('es-CO', { day: '2-digit', month: 'short', year: 'numeric' });
  }
}
