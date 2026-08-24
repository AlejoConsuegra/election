import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../features/user/service/auth.service';
import { UserService } from '../../features/user/service/user.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './layout.html',
  styleUrl: './layout.css',
})
export class Layout {
  public router = inject(Router);
  private authService = inject(AuthService);
  private userService = inject(UserService);

  // ✅ Signals para controlar estados de la UI
  isMobileMenuOpen = signal(false);
  isLideresMenuOpen = signal(false);

  get userName(): string {
    return this.userService.userAuthenticated()?.nombre || 'Usuario';
  }

  get userRole(): string {
    return this.userService.userAuthenticated()?.rol || 'Rol';
  }

  toggleMobileMenu(): void {
    this.isMobileMenuOpen.update((val) => !val);
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen.set(false);
  }

  toggleLideresMenu(): void {
    this.isLideresMenuOpen.update((val) => !val);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
