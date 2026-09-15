import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FbCardContent } from './fb-card-content';

describe('FbCardContent', () => {
  let component: FbCardContent;
  let fixture: ComponentFixture<FbCardContent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FbCardContent],
    }).compileComponents();

    fixture = TestBed.createComponent(FbCardContent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
