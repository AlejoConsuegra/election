import { Component, OnInit, ChangeDetectorRef, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../user/service/auth.service';
import { UserService } from '../../user/service/user.service';
import { ZoneService } from '../../zone/service/zone.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  public router = inject(Router);
  private authService = inject(AuthService);
  private userService = inject(UserService);
  private zoneService = inject(ZoneService);
  private cdr = inject(ChangeDetectorRef);

  users = signal<any[]>([]);
  userName = signal<string>('');
  userRole = signal<string>('');
  totalLideres = signal<number>(0);
  totalZonas = signal<number>(0);
  isLoading = signal<boolean>(false);
  errorMessage = signal<string>('');

  ngOnInit(): void {
    this.loadUserInfo();
    this.loadUsers();
    this.loadZone();
  }

  loadUserInfo(): void {
    const user = this.userService.userAuthenticated();
    if (user) {
      this.userName.set(user.nombre);
      this.userRole.set(user.rol);
    }
  }

  loadUsers(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.userService.getUsers().subscribe({
      next: (data: any) => {
        this.users.set(data.items || []);
        this.totalLideres.set(this.users().length);
        this.isLoading.set(false);

        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error al cargar usuarios:', err);
        this.errorMessage.set('No se pudieron cargar los usuarios');
        this.isLoading.set(false);

        this.cdr.detectChanges();
      },
    });
  }

  loadZone(): void {
    this.zoneService.getAll().subscribe({
      next: (data: any) => {
        this.totalZonas.set(data.length);

        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error al cargar zonas:', err);
        this.errorMessage.set('No se pudieron cargar las zonas');

        this.cdr.detectChanges();
      },
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
