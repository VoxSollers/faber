import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FbSpinner } from './fb-spinner';

describe('FbSpinner', () => {
  let component: FbSpinner;
  let fixture: ComponentFixture<FbSpinner>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FbSpinner],
    }).compileComponents();

    fixture = TestBed.createComponent(FbSpinner);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should default size to "medium"', () => {
    expect(component.size()).toBe('medium');
  });

  it('should accept "small" size input', () => {
    fixture.componentRef.setInput('size', 'small');
    expect(component.size()).toBe('small');
  });

  it('should accept "large" size input', () => {
    fixture.componentRef.setInput('size', 'large');
    expect(component.size()).toBe('large');
  });
});
