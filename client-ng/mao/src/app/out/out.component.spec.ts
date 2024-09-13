import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OutComponent } from './out.component';

describe('OutComponent', () => {
  let component: OutComponent;
  let fixture: ComponentFixture<OutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OutComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(OutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
