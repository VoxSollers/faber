import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AppProgressBar } from './app-progress-bar';

describe('AppProgressBar', () => {
  let component: AppProgressBar;
  let fixture: ComponentFixture<AppProgressBar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppProgressBar],
    }).compileComponents();

    fixture = TestBed.createComponent(AppProgressBar);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render the progress element', () => {
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('.progress')).not.toBeNull();
  });
});
