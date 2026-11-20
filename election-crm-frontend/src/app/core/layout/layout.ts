import { Component, inject, signal, computed, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../features/auth/service/auth.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './layout.html',
  styleUrl: './layout.css',
})
export class Layout {
  private authService = inject(AuthService);
  private router = inject(Router);

  // Estado del UI
  isMobileMenuOpen = signal(false);
  isSidebarCollapsed = signal(false);
  showProfile = signal(false);
  showNotifications = signal(false);
  isLideresMenuOpen = signal(false);
  isZonasMenuOpen = signal(false);

  // Datos del usuario reactivos (se actualizan solos cuando el AuthService cambia)
  /*  userName = computed(() => this.authService.user$()?.datos.nombre || 'Usuario'); */
  userName = computed(() => {
    const user = this.authService.user$();
    return user?.datos?.nombre || 'Usuario';
  });
  userRole = computed(() => this.authService.user$()?.datos.rol || 'Rol');
  userEmail = computed(() => this.authService.user$()?.datos.email || 'usuario@ejemplo.com');

  toggleMobileMenu(): void {
    this.isMobileMenuOpen.update((val) => !val);
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen.set(false);
  }

  toggleSidebar(): void {
    this.isSidebarCollapsed.update((val) => !val);
  }

  toggleLideresMenu(): void {
    this.isLideresMenuOpen.update((val) => !val);
  }

  toggleZonasMenu(): void {
    this.isZonasMenuOpen.update((val) => !val);
  }

  toggleProfile(): void {
    this.showProfile.update((val) => !val);
    this.showNotifications.set(false); // Cierra el otro si está abierto
  }

  toggleNotifications(): void {
    this.showNotifications.update((val) => !val);
    this.showProfile.set(false);
  }

  // Cierra los dropdowns al hacer clic fuera de ellos
  @HostListener('document:click', ['$event'])
  onClickOutside(event: Event): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.profile-dropdown')) {
      this.showProfile.set(false);
    }
    if (!target.closest('.notifications-dropdown')) {
      this.showNotifications.set(false);
    }
  }

  logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/login']);
      },
      error: () => {
        this.authService.clearSession();
        this.router.navigate(['/login']);
      },
    });
  }
}
