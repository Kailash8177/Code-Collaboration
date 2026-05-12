import { environment } from '../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Snapshot {
  id: number;
  projectId: number;
  fileId: number;
  content: string;
  createdBy: number;
  createdAt: Date;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class SnapshotService {
  private apiUrl = `${environment.gatewayUrl}/snapshots`;

  constructor(private http: HttpClient) { }

  createSnapshot(projectId: number, fileId: number, content: string, message: string): Observable<Snapshot> {
    return this.http.post<Snapshot>(this.apiUrl, { projectId, fileId, content, message });
  }

  getSnapshotsForFile(fileId: number): Observable<Snapshot[]> {
    return this.http.get<Snapshot[]>(`${this.apiUrl}/file/${fileId}`);
  }
}
