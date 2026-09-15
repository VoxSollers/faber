import { TestBed } from '@angular/core/testing';
import { Offset } from './offset';

describe('Offset', () => {
  let service: Offset;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Offset);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should expose height as readonly signal', () => {
    expect(service.height()).toBe(0);
    service.setHeight(64);
    expect(service.height()).toBe(64);
  });
});
