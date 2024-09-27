import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Observable } from 'rxjs';
import { GameDataService, GameState } from './game-data.service';

@Injectable({
  providedIn: 'root'
})
export class GameConnectionService {
  private hubConnection: HubConnection;

  constructor(private gameData: GameDataService) { 
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
        });
      this.gameData.subscribe(this);
    });
  }

  joinLobby(lobbyName: string, playerName$: string) {
    this.hubConnection.invoke('JoinLobby', lobbyName, playerName$);
  }

  startGame() {
    this.hubConnection.invoke('StartGame');
  }

  receiveUpdate<T>(eventName: string) {
    return new Observable<T>((observer) => {
      this.hubConnection.on(eventName, (users: T) => {
        observer.next(users);
      })
    });
  }
}
