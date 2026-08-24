import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { catchError, Observable } from 'rxjs';
import { ZoneRequest } from '../interfaces/zone.request';

@Injectable({
  providedIn: 'root',
})
export class ZoneService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5070/api/NombreZona';

  getAll(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl).pipe(
      catchError((error) => {
        console.error('Error al obtener usuarios:', error);
        throw error;
      }),
    );
  }

  getById(id: number): Observable<ZoneRequest> {
    return this.http.get<ZoneRequest>(`${this.apiUrl}/${id}`);
  }

  create(zona: Omit<ZoneRequest, 'idZona'>): Observable<ZoneRequest> {
    return this.http.post<ZoneRequest>(this.apiUrl, zona);
  }

  update(id: number, zona: Omit<ZoneRequest, 'idZona'>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, zona);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
