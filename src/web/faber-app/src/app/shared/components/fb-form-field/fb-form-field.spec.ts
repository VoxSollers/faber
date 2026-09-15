import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FbFormField } from './fb-form-field';

describe('FbFormField', () => {
  let component: FbFormField;
  let fixture: ComponentFixture<FbFormField>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FbFormField],
    }).compileComponents();

    fixture = TestBed.createComponent(FbFormField);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
