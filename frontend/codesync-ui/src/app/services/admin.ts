import { environment } from '../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private authUrl = `${environment.gatewayUrl}/auth`;
  private notifUrl = `${environment.gatewayUrl}/notifications`;
  private projectUrl = `${environment.gatewayUrl}/projects`;

  constructor(private http: HttpClient, private authService: AuthService) { }

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.authService.getToken()}`
    });
  }

  getUsersByRole(role: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.authUrl}/roles/${role}`, { headers: this.getHeaders() });
  }

  deactivateUser(userId: number): Observable<any> {
    return this.http.post(`${this.authUrl}/deactivate/${userId}`, {}, { headers: this.getHeaders() });
  }

  getAllProjects(): Observable<any[]> {
    return this.http.get<any[]>(`${this.projectUrl}/admin/all`, { headers: this.getHeaders() });
  }

  deleteProjectAdmin(projectId: number): Observable<any> {
    return this.http.delete(`${this.projectUrl}/admin/${projectId}`, { headers: this.getHeaders() });
  }

  sendBulkNotification(title: string, message: string): Observable<any> {
    const payload = {
      title,
      message,
      type: 'SYSTEM',
      relatedType: 'GLOBAL',
      relatedId: '0'
    };
    return this.http.post(`${this.notifUrl}/bulk`, payload);
  }
}
