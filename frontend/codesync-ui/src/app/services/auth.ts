import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.gatewayUrl}/auth`;
  private readonly TOKEN_KEY = 'codesync_jwt';

  constructor(private http: HttpClient, private router: Router) { }

  login(email: string, password: string): Observable<any> {
    const body = { email, password };
    return this.http.post(`${this.apiUrl}/login`, body);
  }

  register(username: string, email: string, password: string, fullName: string): Observable<any> {
    const body = { username, email, password, fullName };
    return this.http.post(`${this.apiUrl}/register`, body);
  }

  // --- State Management ---
  saveToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getUserId(): number {
    const token = this.getToken();
    if (!token) return 0;
    try {
      const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
      const userId = payload['userId'] || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
      return Number(userId) || 0;
    } catch (e) {
      console.error('Error decoding JWT', e);
      return 0;
    }
  }

  getUsername(): string {
    const token = this.getToken();
    if (!token) return 'Developer';
    try {
      const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
      return payload['unique_name'] || 
             payload['name'] || 
             payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 
             payload['email'] || 
             'Developer';
    } catch {
      return 'Developer';
    }
  }

  getEmail(): string {
    const token = this.getToken();
    if (!token) return '';
    try {
      const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
      return payload['email'] || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || 'user@example.com';
    } catch {
      return '';
    }
  }

  getRole(): string {
    const token = this.getToken();
    if (!token) return 'DEVELOPER';
    try {
      const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
      
      // Some .NET versions map ClaimTypes.Role to different URLs.
      // We will loop through the keys and find the one that contains 'role'
      for (const key in payload) {
        if (key.toLowerCase().includes('role')) {
          return payload[key].toUpperCase(); // Ensure it returns 'ADMIN' uppercase
        }
      }
      return 'DEVELOPER';
    } catch {
      return 'DEVELOPER';
    }
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;
    
    try {
      const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
      // Check if token is expired (exp is in seconds)
      if (payload.exp && payload.exp * 1000 < Date.now()) {
        localStorage.removeItem(this.TOKEN_KEY);
        return false;
      }
      return true;
    } catch {
      return false;
    }
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    this.router.navigate(['/login']);
  }
}
