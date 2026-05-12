import { environment } from '../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth';

export interface Notification {
  id: number;
  userId: number;
  message: string;
  isRead: boolean;
  createdAt: Date;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = `${environment.gatewayUrl}/notifications`;
  private hubConnection: signalR.HubConnection | undefined;
  
  public newNotification$ = new Subject<Notification>();

  constructor(private http: HttpClient, private authService: AuthService) { }

  public startConnection() {
    const token = this.authService.getToken();
    if (!token) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.gatewayUrl}/hubs/notifications?access_token=${token}`)
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR Notification Hub Connected'))
      .catch(err => console.error('Error starting Notification Hub:', err));

    this.hubConnection.on('ReceiveNotification', (payload: any) => {
      // Backend sends { notification: Notification, unreadCount: number }
      const notif = payload.notification ? payload.notification : payload;
      this.newNotification$.next(notif);
    });
  }

  public disconnect() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  getNotificationsByRecipient(recipientId: number): Observable<Notification[]> {
    return this.http.get<Notification[]>(`${this.apiUrl}/recipient/${recipientId}`);
  }

  markAsRead(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/read`, {});
  }

  markAllRead(recipientId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/recipient/${recipientId}/read-all`, {});
  }
}
