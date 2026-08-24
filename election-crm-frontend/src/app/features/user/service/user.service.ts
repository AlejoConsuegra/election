import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { UserResponse } from '../interfaces/user.response';
import { UserRequest } from '../interfaces/user.request';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private apiUrl = 'http://localhost:5070/api/User';

  constructor(private http: HttpClient) {}

  getUsers(): Observable<any[]> {
    return this.http.get<any[]>('http://localhost:5070/api/User').pipe(
      catchError((error) => {
        console.error('Error al obtener usuarios:', error);
        throw error;
      }),
    );
  }

  add(credentials: UserRequest): Observable<UserResponse> {
    return this.http.post<UserResponse>(this.apiUrl, credentials).pipe(
      tap((response) => {
        if (response) {
          console.log(response);
        }
      }),
    );
  }

  loginUrl() {
    return 'http://localhost:5070/api/User/login';
  }

  userAuthenticated() {
    const userStr = localStorage.getItem('user');
    if (userStr) {
      return JSON.parse(userStr);
    }
  }
}
