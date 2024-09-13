import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class GameDataService {
  public playerName$ = new BehaviorSubject<string>('');

  constructor() { }
}
