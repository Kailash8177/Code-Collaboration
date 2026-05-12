import { environment } from '../../environments/environment';
import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CollabService {
  private hubConnection: signalR.HubConnection | undefined;
  
  public codeChange$ = new Subject<{content: string, isFullUpdate: boolean}>();
  public participantJoined$ = new Subject<any>();
  public participantLeft$ = new Subject<any>();

  constructor(private authService: AuthService) {}

  public startConnection() {
    const token = this.authService.getToken();
    if (!token) return;

    // Use API Gateway route for collab hub
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.gatewayUrl}/hubs/collab?access_token=${token}`)
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR Collab Hub Connected via Gateway'))
      .catch(err => console.error('Error while starting connection: ' + err));

    this.addListeners();
  }

  private addListeners() {
    if (!this.hubConnection) return;

    this.hubConnection.on('ReceiveCodeChange', (payload) => {
      this.codeChange$.next(payload);
    });

    this.hubConnection.on('ParticipantJoined', (payload) => {
      this.participantJoined$.next(payload);
    });

    this.hubConnection.on('ParticipantLeft', (payload) => {
      this.participantLeft$.next(payload);
    });
  }

  public joinSession(sessionId: string) {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('JoinSessionRoom', sessionId)
        .catch(err => console.error('Error joining session:', err));
    }
  }

  public leaveSession(sessionId: string) {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('LeaveSessionRoom', sessionId)
        .catch(err => console.error('Error leaving session:', err));
    }
  }

  public sendCodeChange(sessionId: string, content: string) {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      this.hubConnection.invoke('SendCodeChange', sessionId, content, 'full')
        .catch(err => console.error('Error sending code change:', err));
    }
  }

  public disconnect() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }
}
