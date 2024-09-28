import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { GameDataService, GameState } from './game-data.service';

const until = (predFn: Function, timeout: number) => {
  let timeoutElapsed = false;
  setTimeout(() => timeoutElapsed = true, timeout);
  const poll = (done: Function) => (predFn() || timeoutElapsed ? done() : setTimeout(() => poll(done), 500));
  return new Promise(poll);
};

@Injectable({
  providedIn: 'root'
})
export class GameConnectionService {
  private hubConnection: HubConnection;

  public awaitingInput$ = new BehaviorSubject<boolean>(false);

  private cardInput: number = -1;

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

      this.hubConnection.on("RequestCard", async () => {
        this.awaitingInput$.next(true);
        
        await until(() => this.cardInput != -1, 5000);
        
        this.awaitingInput$.next(false);
        
        let tmpInput = this.cardInput;
        this.cardInput = -1;
        return tmpInput.toString();
      })
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

  setCardInput(cardInput: number) {
    this.cardInput = cardInput;
  }
}
