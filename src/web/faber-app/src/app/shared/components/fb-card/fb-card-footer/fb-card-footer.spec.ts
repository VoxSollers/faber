import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FbCardFooter } from './fb-card-footer';

describe('FbCardFooter', () => {
  let component: FbCardFooter;
  let fixture: ComponentFixture<FbCardFooter>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FbCardFooter],
    }).compileComponents();

    fixture = TestBed.createComponent(FbCardFooter);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
