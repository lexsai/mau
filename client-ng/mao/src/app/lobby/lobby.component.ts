import { Component } from '@angular/core';
import { GameConnectionService } from '../game-connection.service';
import { GameDataService, GameState } from '../game-data.service';
import { ActivatedRoute, Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { AsyncPipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { LobbyExists } from '../models/lobby-exists';

@Component({
  selector: 'app-lobby',
  standalone: true,
  imports: [AsyncPipe],
  templateUrl: './lobby.component.html',
  styleUrl: './lobby.component.css'
})
export class LobbyComponent {
  
  public shareUrl = '';

  public gameStates = GameState;

  constructor(
    private gameConnection: GameConnectionService,
    private gameData: GameDataService,
    private activatedRoute: ActivatedRoute,
    private http: HttpClient, 
    private router: Router,
  ) {}
  
  ngOnInit() {
    this.shareUrl = location.origin + location.pathname;

    const lobbyName = this.activatedRoute.snapshot.params['name'];
    this.http.post<LobbyExists>("http://localhost:5000/lobby/exists", { name: lobbyName }).subscribe(
      response => {
        if (!response.exists) {
          this.router.navigate(['/']);
        } else if (this.gameData.playerName$.getValue() != '') {
          // playerName must have already been set from '/'.
          this.joinLobby(this.gameData.playerName$.getValue());
        }
      }
    );
  }

  joinLobby(playerName: string) {    
    this.gameConnection.startConnection().subscribe(() => {
      console.log('subscription to startConnection triggered');

      this.gameData.playerName$.next(playerName);
      const lobbyName = this.activatedRoute.snapshot.params['name'];
      this.gameConnection.joinLobby(lobbyName, playerName);
    });

  }

  start() {
    this.gameConnection.startGame();
  }

  get playerName$(): BehaviorSubject<string> {
    return this.gameData.playerName$;
  }

  get gameState$(): BehaviorSubject<GameState> {
    return this.gameData.state$;
  }

  get lobbyMembers$(): BehaviorSubject<string[]> {
    return this.gameData.lobbyMembers$;
  }
  
  get hand$(): BehaviorSubject<string[]> {
    return this.gameData.hand$;
  }

  get message$(): BehaviorSubject<string> {
    return this.gameData.message$;
  }
}
