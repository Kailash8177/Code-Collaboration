import { environment } from '../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ExecutionRequest {
  sourceCode: string;
  language: string;
  projectId: number;
  fileId?: number;
}

export interface ExecutionResponse {
  stdout: string;
  stderr: string;
  executionTimeMs: number;
}

@Injectable({
  providedIn: 'root'
})
export class ExecutionService {
  private apiUrl = `${environment.gatewayUrl}/executions`;

  constructor(private http: HttpClient) { }

  submitCode(request: ExecutionRequest): Observable<ExecutionResponse> {
    return this.http.post<ExecutionResponse>(`${this.apiUrl}/submit`, request);
  }
}
