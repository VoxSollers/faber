import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FbCardHeader } from './fb-card-header';

describe('FbCardHeader', () => {
  let component: FbCardHeader;
  let fixture: ComponentFixture<FbCardHeader>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FbCardHeader],
    }).compileComponents();

    fixture = TestBed.createComponent(FbCardHeader);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
