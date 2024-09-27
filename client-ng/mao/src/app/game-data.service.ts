import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { GameConnectionService } from './game-connection.service';

export enum GameState {
  Waiting,
  InGame
}

@Injectable({
  providedIn: 'root'
})
export class GameDataService {
  public playerName$ = new BehaviorSubject<string>('');  
  public state$ = new BehaviorSubject<GameState>(GameState.Waiting);

  public lobbyMembers$ = new BehaviorSubject<string[]>([]);
  public hand$ = new BehaviorSubject<string[]>([]);
  public message$ = new BehaviorSubject<string>('');

  constructor() { }

  subscribe(conn: GameConnectionService) {
    conn.receiveUpdate<string[]>('LobbyUsersUpdate').subscribe((lobbyMembers: string[]) => {
      this.lobbyMembers$.next(lobbyMembers);
    });

    conn.receiveUpdate<string[]>('GameStarted').subscribe((hand: string[]) => {
      this.state$.next(GameState.InGame);

      this.hand$.next(hand);
    });

    conn.receiveUpdate<string[]>('HandUpdate').subscribe((hand: string[]) => {
      this.hand$.next(hand);
    });

    conn.receiveUpdate<string>('WriteMessage').subscribe((message: string) => {
      this.message$.next(message);
    })
  }
}
