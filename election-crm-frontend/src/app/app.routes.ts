import { Routes } from '@angular/router';
import { Login } from './features/user/components/login/login';
import { Layout } from './core/layout/layout';
import { authGuard } from './core/guards/auth.guard';

import { Dashboard } from './features/dashboard/components/dashboard';
import { DashboardZone } from './features/zone/component/dashboard-zone/dashboard-zone';
import { CreateZone } from './features/zone/component/create-zone/create-zone';
import { CreateUser } from './features/user/components/create-user/create-user';

export const routes: Routes = [
  // Ruta pública
  { path: 'login', component: Login },

  // Rutas protegidas dentro del Layout
  {
    path: '',
    component: Layout,
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', component: Dashboard },
      { path: 'crear-usuario', component: CreateUser },
      { path: 'zonas', component: DashboardZone },
      { path: 'crear-zona', component: CreateZone },
      { path: 'editar-zona/:id', component: CreateZone },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }, // Redirección por defecto al entrar autenticado
    ],
  },

  // Ruta comodín
  { path: '**', redirectTo: 'login' },
];
