import { TestBed } from '@angular/core/testing';
import { IdentityClient } from './identity-client';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

describe('IdentityClient', () => {
  let service: IdentityClient;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(IdentityClient);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
