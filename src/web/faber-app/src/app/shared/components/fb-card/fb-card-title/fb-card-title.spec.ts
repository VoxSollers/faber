import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FbCardTitle } from './fb-card-title';

describe('FbCardTitle', () => {
  let component: FbCardTitle;
  let fixture: ComponentFixture<FbCardTitle>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FbCardTitle],
    }).compileComponents();

    fixture = TestBed.createComponent(FbCardTitle);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
