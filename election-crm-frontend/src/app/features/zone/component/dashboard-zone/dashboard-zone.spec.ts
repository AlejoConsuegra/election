import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Zone } from './zone';

describe('Zone', () => {
  let component: Zone;
  let fixture: ComponentFixture<Zone>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Zone],
    }).compileComponents();

    fixture = TestBed.createComponent(Zone);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
