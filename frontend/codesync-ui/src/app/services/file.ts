import { environment } from '../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CodeFile {
  fileId: number;
  projectId: number;
  name: string;
  path: string;
  language: string;
  isFolder: boolean;
}

export interface FileContent {
  fileId: number;
  name: string;
  path: string;
  content: string;
}

export interface FileTreeNode {
  fileId: number;
  name: string;
  path: string;
  isFolder: boolean;
  language: string;
  children: FileTreeNode[];
}

@Injectable({
  providedIn: 'root'
})
export class FileService {
  private apiUrl = `${environment.gatewayUrl}/files`;

  constructor(private http: HttpClient) { }

  getProjectTree(projectId: number): Observable<FileTreeNode[]> {
    return this.http.get<FileTreeNode[]>(`${this.apiUrl}/project/${projectId}/tree`);
  }

  getFileContent(fileId: number): Observable<FileContent> {
    return this.http.get<FileContent>(`${this.apiUrl}/${fileId}/content`);
  }

  createFile(projectId: number, name: string, path: string, isFolder: boolean, language: string): Observable<CodeFile> {
    return this.http.post<CodeFile>(this.apiUrl, { projectId, name, path, isFolder, language });
  }

  saveFileContent(fileId: number, content: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${fileId}/content`, { content });
  }
}
