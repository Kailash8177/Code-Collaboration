import { environment } from '../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Project {
  projectId: number;
  ownerId: number;
  name: string;
  description: string;
  language: string;
  visibility: string;
  starCount: number;
  forkCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class ProjectService {
  private apiUrl = `${environment.gatewayUrl}/projects`;

  constructor(private http: HttpClient) { }

  getProjectsByOwner(ownerId: number): Observable<Project[]> {
    return this.http.get<Project[]>(`${this.apiUrl}/owner/${ownerId}`);
  }

  getPublicProjects(): Observable<Project[]> {
    return this.http.get<Project[]>(`${this.apiUrl}/public`);
  }

  getProject(id: number): Observable<Project> {
    return this.http.get<Project>(`${this.apiUrl}/${id}`);
  }

  createProject(name: string, description: string, language: string, visibility: string): Observable<Project> {
    return this.http.post<Project>(this.apiUrl, { name, description, language, visibility });
  }

  deleteProject(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
