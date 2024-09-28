import { AsyncPipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [AsyncPipe],
  templateUrl: './card.component.html',
  styleUrl: './card.component.css'
})
export class CardComponent {
  @Input() cardName!: string;
  @Input() awaitingInput$!: BehaviorSubject<boolean>;
  @Output() played = new EventEmitter<void>();
}
