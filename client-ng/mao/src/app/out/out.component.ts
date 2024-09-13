import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { GameDataService } from '../game-data.service';
import { LobbyCreated } from '../models/lobby-created';
import { Router } from '@angular/router';

@Component({
  selector: 'app-out',
  standalone: true,
  imports: [],
  templateUrl: './out.component.html',
  styleUrl: './out.component.css'
})
export class OutComponent { 
  constructor(
    private http: HttpClient, 
    private gameData: GameDataService,
    private router: Router) {}
  
  createLobby(playerName$: string) {
    this.http.post<LobbyCreated>("http://localhost:5000/lobby/start", {}).subscribe(
      response => {   
        this.gameData.playerName$.next(playerName$);
        
        this.router.navigate(['/lobby/' + response.name]);
      }
    );

  }
}
