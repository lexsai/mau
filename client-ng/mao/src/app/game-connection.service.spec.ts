import { TestBed } from '@angular/core/testing';

import { GameConnectionService } from './game-connection.service';

describe('GameConnectionService', () => {
  let service: GameConnectionService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GameConnectionService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
