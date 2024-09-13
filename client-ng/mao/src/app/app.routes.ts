import { Routes } from '@angular/router';
import { OutComponent } from './out/out.component';
import { LobbyComponent } from './lobby/lobby.component';

export const routes: Routes = [
    { path: '', component: OutComponent },
    { path: 'lobby/:name', component: LobbyComponent },
];
