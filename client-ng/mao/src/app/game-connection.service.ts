import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class GameConnectionService {
  private hubConnection: HubConnection;

  constructor() { 
    this.hubConnection = new HubConnectionBuilder()
      .withUrl("http://localhost:5000/game")
      .build();
  }

  startConnection() {
    return new Observable<void>((observer) => {
      this.hubConnection
        .start()
        .then(() => {
          console.log('connected to hub.');
          observer.next();
          observer.complete();
        }).catch((error) => {
          console.error('error connecting to hub:', error);
          observer.error(error);
        })
    })
  }

  joinLobby(lobbyName: string, playerName$: string) {
    this.hubConnection.invoke('JoinLobby', lobbyName, playerName$);
  }

  receiveLobbyUsersUpdate() {
    return new Observable<string[]>((observer) => {
      this.hubConnection.on('LobbyUsersUpdate', (users: string[]) => {
        observer.next(users);
      })
    })
  }
}
