import { Injectable } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { UserResponse } from '../models/user.response';
import { UserRequest } from '../models/user.request';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private apiUrl = 'http://localhost:5070/api/User';

  constructor(private http: HttpClient) {}

  getUsers(): Observable<UserResponse[]> {
    return this.http.get<UserResponse[]>(this.apiUrl).pipe(
      catchError((error) => {
        console.error('Error al obtener usuarios:', error);
        return throwError(() => error);
      }),
    );
  }

  create(payload: UserRequest): Observable<UserResponse> {
    return this.http.post<UserResponse>(this.apiUrl, payload);
  }
}
