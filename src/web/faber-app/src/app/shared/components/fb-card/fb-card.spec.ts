import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FbCard } from './fb-card';

describe('FbCard', () => {
  let component: FbCard;
  let fixture: ComponentFixture<FbCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FbCard],
    }).compileComponents();

    fixture = TestBed.createComponent(FbCard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
