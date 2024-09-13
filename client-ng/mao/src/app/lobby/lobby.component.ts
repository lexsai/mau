import { Component } from '@angular/core';
import { GameConnectionService } from '../game-connection.service';
import { GameDataService } from '../game-data.service';
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
  
  public shareUrl: string = '';
  public lobbyMembers: string[] = [];

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
    this.http.post<LobbyExists>("http://localhost:5000/lobby/exists", {
      name: lobbyName,
    }).subscribe(
      response => {
        if (!response.exists) {
          this.router.navigate(['/']);
        } else if (this.gameData.playerName$.getValue() != '') {
          this.start(this.gameData.playerName$.getValue());
        }
      }
    );
  }

  start(playerName: string) {    
    this.gameConnection.startConnection().subscribe(() => {
      console.log('subscription to startConnection triggered');

      this.gameConnection.receiveLobbyUsersUpdate().subscribe((lobbyMembers: string[]) => {
        this.lobbyMembers = lobbyMembers;
      });

      this.gameData.playerName$.next(playerName);
      const lobbyName = this.activatedRoute.snapshot.params['name'];
      this.gameConnection.joinLobby(lobbyName, playerName);
    });

  }

  playerName(): BehaviorSubject<string> {
    return this.gameData.playerName$;
  }
}
