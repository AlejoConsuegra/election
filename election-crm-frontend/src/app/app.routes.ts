import { Routes } from '@angular/router';
import { Login } from './features/auth/login/login';
import { Layout } from './core/layout/layout';
import { authGuard } from './core/guards/auth.guard';
import { redirectIfAuthenticatedGuard } from './core/guards/redirect-if-authenticated.guard';

import { Dashboard } from './features/dashboard/dashboard';
import { DashboardZone } from './features/zone/dashboard-zone/dashboard-zone';
import { CreateZone } from './features/zone/create-zone/create-zone';
import { CreateUser } from './features/user/create-user/create-user';

export const routes: Routes = [
  // Ruta pública, pero protegida contra usuarios ya autenticados
  {
    path: 'login',
    component: Login,
    canActivate: [redirectIfAuthenticatedGuard],
  },

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
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    ],
  },

  // Ruta comodín: cualquier ruta no definida redirige al login
  { path: '**', redirectTo: 'login' },
];
