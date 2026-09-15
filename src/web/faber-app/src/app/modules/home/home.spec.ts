import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideRouter } from '@angular/router';
import { Home } from './home';
import { AuthStore } from '../../core/auth/auth-store';

describe('Home', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Home],
      providers: [
        provideRouter([]),
        {
          provide: AuthStore,
          useValue: {
            authenticated: signal(false),
            user: signal(null),
            loading: signal(false),
            error: signal(null),
            clearUser: () => {},
            reload: () => {},
          },
        },
      ],
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(Home);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
