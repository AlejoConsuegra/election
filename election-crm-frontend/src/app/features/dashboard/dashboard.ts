import {
  Component,
  OnInit,
  OnDestroy,
  inject,
  signal,
  computed,
  HostListener,
  ViewEncapsulation,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { DashboardHeader } from './components/header/dashboard-header';
import { ErrorBanner } from './components/error-banner/error-banner';
import { KpiCards } from './components/kpi-cards/kpi-cards';
import { KpiCard } from './models/kpi-card.model';
/* import { VotesChart } from './components/votes-chart/votes-chart';
import { GoalGauge } from './components/goal-gauge/goal-gauge';
import { TopZone } from './components/top-zone/top-zone'; */
import { MuniRow } from './models/muni-row.model';
import { SourceRow } from './models/source-row.model';
/* import { OriginRecords } from './components/origin-records/origin-records'; */
import { PieSlice } from './models/pie-slice.model';
/* import { DistributionVotes } from './components/distribution-votes/distribution-votes'; */
import { LeaderRow } from './models/leader-row.model';
import { LeadersTable } from './components/leaders-table/leaders-table';
import { SEED_LEADERS, SEED_ZONES } from './models/fictitious-data.model';
import { ICON, MONTH_LABELS, PALETTE } from './models/UI-uses.model';
import { AuthService } from '../auth/service/auth.service';

export interface ZoneRow {
  id: number;
  nombre: string;
  municipio: string;
  votos: number;
  potencial: number;
}

interface MonthPoint {
  label: string;
  proyectados: number;
  confirmados: number;
}

interface GaugeTick {
  x1: number;
  y1: number;
  x2: number;
  y2: number;
  filled: boolean;
}

interface GaugeStat {
  label: string;
  value: string;
  paths: string[];
  color: string;
  tint: string;
}

interface FeedItem {
  id: number;
  text: string;
  amount: number;
  time: string;
  color: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DashboardHeader,
    ErrorBanner,
    KpiCards,
    /*  VotesChart,
     GoalGauge,
    TopZone,
    OriginRecords,
    DistributionVotes, */
    LeadersTable,
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
  encapsulation: ViewEncapsulation.None,
})
export class Dashboard implements OnInit, OnDestroy {
  private http = inject(HttpClient);
  private auth = inject(AuthService);
  private router = inject(Router);
  private baseUrl = environment.apiUrl;

  private rafId: number | null = null;
  private liveTimer: ReturnType<typeof setInterval> | null = null;
  private readyTimer: ReturnType<typeof setTimeout> | null = null;
  private feedSeq = 0;
  private nf = new Intl.NumberFormat('es-CO');

  readonly userName = computed(() => this.auth.user$()?.datos?.nombre || 'Usuario');
  readonly userRole = computed(() => this.auth.user$()?.datos?.rol || 'Invitado');

  readonly isLoading = signal(true);
  readonly loadError = signal<string | null>(null);
  readonly demoMode = signal(false);
  readonly leaders = signal<LeaderRow[]>([]);
  readonly zones = signal<ZoneRow[]>([]);
  readonly lastUpdate = signal<Date>(new Date());
  readonly selected = signal<LeaderRow | null>(null);

  readonly liveMode = signal(false);
  readonly liveVotes = signal(0);
  readonly liveFeed = signal<FeedItem[]>([]);

  readonly hoverMonth = signal<number | null>(null);
  readonly chartReady = signal(false);
  readonly kpiDisplay = signal<number[]>([0, 0, 0, 0]);

  protected openDetail(l: LeaderRow): void {
    this.selected.set(l);
  }

  protected closeDetail(): void {
    this.selected.set(null);
  }

  protected initials(name: string): string {
    const p = name.trim().split(/\s+/);
    return ((p[0]?.[0] ?? '') + (p[1]?.[0] ?? '')).toUpperCase() || 'U';
  }

  protected shareOfZone(votos: number): string {
    const v = this.votes();
    return v > 0 ? ((votos / v) * 100).toFixed(1) : '0.0';
  }

  protected dateLabel(iso: string): string {
    const d = new Date(iso);
    return isNaN(d.getTime())
      ? '—'
      : d.toLocaleDateString('es-CO', { day: '2-digit', month: 'short', year: 'numeric' });
  }

  protected shareOfCensus(votos: number): number {
    const c = this.census();
    return c > 0 ? (votos / c) * 100 : 0;
  }

  readonly allLeaders = computed(() => this.leaders());

  readonly leaderCount = computed(
    () => this.leaders().filter((l) => this.isLeader(l.rol) && l.activo).length,
  );
  readonly zoneCount = computed(() => this.zones().length);
  readonly census = computed(() => this.zones().reduce((a, z) => a + z.potencial, 0));
  readonly votes = computed(
    () => this.leaders().reduce((a, l) => a + l.votos, 0) + this.liveVotes(),
  );
  readonly progress = computed(() => {
    const c = this.census();
    return c > 0 ? Math.min(100, Math.round((this.votes() / c) * 100)) : 0;
  });

  readonly kpiCards = computed<KpiCard[]>(() => {
    const d = this.kpiDisplay();
    const total = this.leaders().length || 1;
    return [
      {
        key: 'lideres',
        label: 'Líderes Activos',
        value: d[0] ?? 0,
        delta: this.delta(this.leaderCount(), total, 3.65),
        color: '#6366F1',
        tint: 'rgba(99,102,241,.12)',
        paths: ICON['users'] ?? [],
      },
      {
        key: 'votos',
        label: 'Votos Registrados',
        value: d[1] ?? 0,
        delta: this.delta(this.votes(), this.census(), 4.44),
        color: '#F5A623',
        tint: 'rgba(245,166,35,.16)',
        paths: ICON['check'] ?? [],
      },
      {
        key: 'censo',
        label: 'Votantes en Censo',
        value: d[2] ?? 0,
        delta: this.delta(this.census(), this.census() + 400, 5.25),
        color: '#EF4444',
        tint: 'rgba(239,68,68,.12)',
        paths: ICON['group'] ?? [],
      },
      {
        key: 'zonas',
        label: 'Zonas Geográficas',
        value: d[3] ?? 0,
        delta: this.delta(this.zoneCount(), this.zoneCount() + 1, 6.77),
        color: '#22C55E',
        tint: 'rgba(34,197,94,.14)',
        paths: ICON['pin'] ?? [],
      },
    ];
  });

  readonly months = computed<MonthPoint[]>(() => {
    const base = this.votes();
    if (base <= 0) return [];
    const weights = [0.62, 0.7, 0.68, 0.78, 0.74, 0.86, 0.8, 0.95, 0.83];
    const scale = Math.max(1, base / 22);
    return MONTH_LABELS.map((label, i) => {
      const w = weights[i] ?? 0.7;
      const proyectados = Math.round(scale * w);
      const confirmados = Math.round(proyectados * (0.5 + (i % 3) * 0.06));
      return { label, proyectados, confirmados };
    });
  });

  readonly chartMax = computed(() => {
    const m = this.months().reduce((a, p) => Math.max(a, p.proyectados, p.confirmados), 0);
    return Math.max(30, Math.ceil(m / 30) * 30);
  });

  readonly chartTicks = computed(() => {
    const max = this.chartMax();
    return [max, (max * 3) / 4, max / 2, max / 4, 0];
  });

  readonly chartEmpty = computed(() => this.months().length === 0);

  readonly gaugePercent = computed(() => this.progress());

  readonly gaugeTicks = computed<GaugeTick[]>(() => {
    const n = 44;
    const filled = Math.round((this.gaugePercent() / 100) * n);
    const out: GaugeTick[] = [];
    for (let i = 0; i < n; i++) {
      const deg = 135 + (270 * i) / (n - 1);
      const rad = (deg * Math.PI) / 180;
      const cos = Math.cos(rad);
      const sin = Math.sin(rad);
      out.push({
        x1: +(100 + 66 * cos).toFixed(2),
        y1: +(100 + 66 * sin).toFixed(2),
        x2: +(100 + 86 * cos).toFixed(2),
        y2: +(100 + 86 * sin).toFixed(2),
        filled: i < filled,
      });
    }
    return out;
  });

  readonly gaugeStats = computed<GaugeStat[]>(() => [
    {
      label: 'Nuevos Líderes',
      value: this.fmt(this.leaders().length),
      paths: ICON['plus'] ?? [],
      color: '#6366F1',
      tint: 'rgba(99,102,241,.12)',
    },
    {
      label: 'Zonas Cubiertas',
      value: this.fmt(this.zoneCount()),
      paths: ICON['ok'] ?? [],
      color: '#22C55E',
      tint: 'rgba(34,197,94,.14)',
    },
    {
      label: 'Último Corte',
      value: this.timeLabel(this.lastUpdate()),
      paths: ICON['clock'] ?? [],
      color: '#F5A623',
      tint: 'rgba(245,166,35,.16)',
    },
  ]);

  readonly topMunicipios = computed<MuniRow[]>(() => {
    const acc = new Map<string, number>();
    for (const l of this.leaders()) {
      const key = l.municipio || 'Sin asignar';
      acc.set(key, (acc.get(key) ?? 0) + l.votos);
    }
    for (const z of this.zones()) {
      if (!acc.has(z.municipio)) acc.set(z.municipio, z.votos);
    }
    return Array.from(acc.entries())
      .sort((a, b) => b[1] - a[1])
      .slice(0, 6)
      .map(([nombre, votos], i) => ({
        nombre,
        sigla: nombre.slice(0, 2).toUpperCase(),
        votos,
        delta: +(((i % 3) - 1) * 2.4 + 3.1).toFixed(2),
        color: PALETTE[i % PALETTE.length] ?? '#3B82F6',
      }));
  });

  readonly sources = computed<SourceRow[]>(() => {
    const total = this.leaders().length;
    if (total === 0) return [];
    const parts = [
      { label: 'Registro Directo', ratio: 0.34, paths: ICON['direct'] ?? [], delta: 4.2 },
      { label: 'Referido por Líder', ratio: 0.26, paths: ICON['refer'] ?? [], delta: 7.8 },
      { label: 'Redes Sociales', ratio: 0.18, paths: ICON['social'] ?? [], delta: -2.1 },
      { label: 'Jornada Territorial', ratio: 0.13, paths: ICON['field'] ?? [], delta: 5.6 },
      { label: 'Correo / Boletín', ratio: 0.06, paths: ICON['mail'] ?? [], delta: -1.4 },
      { label: 'Otros canales', ratio: 0.03, paths: ICON['other'] ?? [], delta: 1.9 },
    ];
    return parts.map((p) => ({
      label: p.label,
      value: Math.max(1, Math.round(total * p.ratio)),
      delta: p.delta,
      paths: p.paths,
    }));
  });

  readonly sourcesTotal = computed(() => this.sources().reduce((a, s) => a + s.value, 0));

  readonly pieSlices = computed<PieSlice[]>(() => {
    const data = this.zones()
      .slice()
      .sort((a, b) => b.votos - a.votos)
      .slice(0, 5)
      .map((z, i) => ({
        label: z.nombre,
        value: z.votos,
        color: PALETTE[i % PALETTE.length] ?? '#3B82F6',
      }));
    const total = data.reduce((a, d) => a + d.value, 0);
    if (total <= 0) return [];
    let cursor = -90;
    return data.map((d) => {
      const sweep = (d.value / total) * 360;
      const start = cursor;
      const end = cursor + sweep;
      cursor = end;
      const mid = ((start + end) / 2) * (Math.PI / 180);
      const percent = (d.value / total) * 100;
      return {
        label: d.label,
        value: d.value,
        color: d.color,
        percent,
        path: this.arcPath(start, end),
        labelX: +(100 + 58 * Math.cos(mid)).toFixed(2),
        labelY: +(100 + 58 * Math.sin(mid)).toFixed(2),
        showLabel: percent >= 9,
      };
    });
  });

  readonly pieTotal = computed(() => this.pieSlices().reduce((a, s) => a + s.value, 0));

  ngOnInit(): void {
    this.load();
  }

  ngOnDestroy(): void {
    if (this.rafId !== null) cancelAnimationFrame(this.rafId);
    this.stopLive();
    if (this.readyTimer) clearTimeout(this.readyTimer);
  }

  load(): void {
    this.isLoading.set(true);
    this.loadError.set(null);
    this.chartReady.set(false);

    forkJoin({
      users: this.fetchArray(`${this.baseUrl}/api/User`),
      zones: this.fetchArray(`${this.baseUrl}/api/NombreZona`),
    }).subscribe({
      next: ({ users, zones }) => {
        const usersFailed = users === null;
        const zonesFailed = zones === null;

        if (usersFailed && zonesFailed) {
          this.demoMode.set(true);
          this.leaders.set(SEED_LEADERS.map((l) => ({ ...l })));
          this.zones.set(SEED_ZONES.map((z) => ({ ...z })));
          this.loadError.set(
            'No fue posible contactar la API. Se muestra un conjunto de demostración para validar la interfaz.',
          );
        } else {
          this.demoMode.set(false);
          const rows = (users ?? []).map((u, i) => this.normalizeLeader(u, i));
          this.leaders.set(rows);
          this.zones.set(
            zonesFailed || (zones ?? []).length === 0
              ? this.deriveZones(rows)
              : (zones ?? []).map((z, i) => this.normalizeZone(z, i, rows)),
          );
        }

        this.finishLoad();
      },
      error: (err) => {
        this.demoMode.set(true);
        this.leaders.set(SEED_LEADERS.map((l) => ({ ...l })));
        this.zones.set(SEED_ZONES.map((z) => ({ ...z })));
        this.loadError.set('Error inesperado al cargar los datos.');
        this.finishLoad();
      },
    });
  }

  private finishLoad(): void {
    this.lastUpdate.set(new Date());
    this.isLoading.set(false);

    this.tween();
    if (this.readyTimer) clearTimeout(this.readyTimer);
    this.readyTimer = setTimeout(() => this.chartReady.set(true), 90);
  }

  refresh(): void {
    this.liveVotes.set(0);
    this.liveFeed.set([]);
    this.load();
  }

  toggleLive(): void {
    if (this.liveMode()) {
      this.liveMode.set(false);
      this.stopLive();
      return;
    }
    this.liveMode.set(true);
    this.stopLive();
    this.liveTimer = setInterval(() => this.pushLiveEvent(), 2400);
    this.pushLiveEvent();
  }

  private stopLive(): void {
    if (this.liveTimer) {
      clearInterval(this.liveTimer);
      this.liveTimer = null;
    }
  }

  private pushLiveEvent(): void {
    const pool = this.leaders();
    if (pool.length === 0) return;
    const idx = Math.floor(Math.random() * pool.length);
    const target = pool[idx];
    if (!target) return;
    const amount = 1 + Math.floor(Math.random() * 8);

    this.leaders.set(pool.map((l, i) => (i === idx ? { ...l, votos: l.votos + amount } : l)));
    this.liveVotes.set(this.liveVotes() + amount);
    this.lastUpdate.set(new Date());

    const item: FeedItem = {
      id: ++this.feedSeq,
      text: `${target.nombre.split(' ')[0]} · ${target.zona || target.municipio}`,
      amount,
      time: this.timeLabel(new Date()),
      color: PALETTE[idx % PALETTE.length] ?? '#3B82F6',
    };
    this.liveFeed.set([item, ...this.liveFeed()].slice(0, 6));
    this.tween();
  }

  private tween(): void {
    const targets = [this.leaderCount(), this.votes(), this.census(), this.zoneCount()];
    const from = this.kpiDisplay();
    const start = performance.now();
    const dur = 850;
    if (this.rafId !== null) cancelAnimationFrame(this.rafId);

    const step = (now: number): void => {
      const t = Math.min(1, (now - start) / dur);
      const e = 1 - Math.pow(1 - t, 3);
      this.kpiDisplay.set(
        targets.map((to, i) => {
          const f = from[i] ?? 0;
          return Math.round(f + (to - f) * e);
        }),
      );
      if (t < 1) {
        this.rafId = requestAnimationFrame(step);
      } else {
        this.rafId = null;
      }
    };
    this.rafId = requestAnimationFrame(step);
  }

  goCreate(): void {
    this.router.navigate(['/crear-usuario']);
  }

  goZones(): void {
    this.router.navigate(['/zonas']);
  }

  exportCsv(): void {
    const head = [
      'ID',
      'Nombre',
      'Cedula',
      'Email',
      'Telefono',
      'Municipio',
      'Zona',
      'Rol',
      'Votos',
      'Estado',
    ];
    const lines = this.allLeaders().map((l) =>
      [
        l.id,
        l.nombre,
        l.cedula,
        l.email,
        l.telefono,
        l.municipio,
        l.zona,
        l.rol,
        l.votos,
        l.activo ? 'Activo' : 'Inactivo',
      ]
        .map((v) => `"${String(v).replace(/"/g, '""')}"`)
        .join(','),
    );
    const csv = '\uFEFF' + [head.join(','), ...lines].join('\r\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `lideres-electoral-${new Date().toISOString().slice(0, 10)}.csv`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  }

  barHeight(v: number): number {
    if (!this.chartReady()) return 0;
    const max = this.chartMax();
    return max > 0 ? Math.max(2, (v / max) * 100) : 0;
  }

  trendColor(delta: number): string {
    return delta >= 0 ? '#22C55E' : '#EF4444';
  }

  fmt(n: number): string {
    return this.nf.format(Math.round(n || 0));
  }

  timeLabel(d: Date): string {
    return d.toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit' });
  }

  private delta(part: number, whole: number, fallback: number): number {
    if (whole <= 0) return fallback;
    const d = (part / whole) * 100;
    return +Math.max(0.4, Math.min(99, d)).toFixed(2);
  }

  private isLeader(rol: string): boolean {
    const r = (rol || '').toLowerCase();
    return r.includes('lider') || r.includes('líder');
  }

  private arcPath(startDeg: number, endDeg: number): string {
    const r = 92;
    const s = (startDeg * Math.PI) / 180;
    const e = (endDeg * Math.PI) / 180;
    const x1 = +(100 + r * Math.cos(s)).toFixed(3);
    const y1 = +(100 + r * Math.sin(s)).toFixed(3);
    const x2 = +(100 + r * Math.cos(e)).toFixed(3);
    const y2 = +(100 + r * Math.sin(e)).toFixed(3);
    const large = endDeg - startDeg > 180 ? 1 : 0;
    return `M100,100 L${x1},${y1} A${r},${r} 0 ${large} 1 ${x2},${y2} Z`;
  }

  private fetchArray(url: string): Observable<any[] | null> {
    return this.http.get<any>(url).pipe(
      map((r) => this.unwrap(r)),
      catchError(() => of(null)),
    );
  }

  private unwrap(r: any): any[] {
    if (Array.isArray(r)) return r;
    if (r && Array.isArray(r.datos)) return r.datos;
    if (r && r.datos && Array.isArray(r.datos.items)) return r.datos.items;
    if (r && Array.isArray(r.data)) return r.data;
    return [];
  }

  private normalizeLeader(u: any, i: number): LeaderRow {
    const nombre = String(u?.nombre ?? u?.nombreCompleto ?? `Usuario ${i + 1}`);
    const rol = String(u?.nombreRol ?? u?.rol ?? u?.nombre_rol ?? 'Líder');
    const municipio = String(u?.municipio ?? u?.nombreMunicipio ?? 'Sin asignar');
    return {
      id: Number(u?.id ?? u?.idUser ?? u?.idUsuario ?? i + 1),
      nombre,
      email: String(u?.email ?? u?.correo ?? ''),
      telefono: String(u?.telefono ?? u?.celular ?? ''),
      cedula: String(u?.cedula ?? u?.documento ?? ''),
      municipio,
      zona: String(u?.zona ?? u?.nombreZona ?? municipio),
      rol,
      votos: Number(u?.votos ?? u?.totalVotos ?? u?.votosRegistrados ?? 0),
      activo: u?.activo === undefined ? true : Boolean(u.activo),
      fecha: String(u?.fechaRegistro ?? u?.createdAt ?? u?.fecha ?? ''),
    };
  }

  private normalizeZone(z: any, i: number, rows: LeaderRow[]): ZoneRow {
    const nombre = String(z?.nombre ?? z?.nombreZona ?? z?.zona ?? `Zona ${i + 1}`);
    const municipio = String(z?.municipio ?? z?.nombreMunicipio ?? nombre);
    const fromLeaders = rows
      .filter((l) => l.zona === nombre || l.municipio === municipio)
      .reduce((a, l) => a + l.votos, 0);
    const votos = Number(z?.votos ?? z?.totalVotos ?? fromLeaders);
    const potencial = Number(
      z?.potencial ?? z?.potencialVotos ?? z?.censo ?? Math.round(votos * 1.45),
    );
    return {
      id: Number(z?.id ?? z?.idZona ?? i + 1),
      nombre,
      municipio,
      votos,
      potencial: Math.max(potencial, votos),
    };
  }

  private deriveZones(rows: LeaderRow[]): ZoneRow[] {
    const acc = new Map<string, { votos: number; municipio: string }>();
    for (const l of rows) {
      const key = l.zona || l.municipio || 'Sin zona';
      const cur = acc.get(key) ?? { votos: 0, municipio: l.municipio };
      cur.votos += l.votos;
      acc.set(key, cur);
    }
    return Array.from(acc.entries()).map(([nombre, v], i) => ({
      id: i + 1,
      nombre,
      municipio: v.municipio,
      votos: v.votos,
      potencial: Math.max(v.votos, Math.round(v.votos * 1.45)),
    }));
  }
}
