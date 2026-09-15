import { TestBed } from '@angular/core/testing';
import { FbHeroBlobs } from './fb-hero-blobs';

describe('FbHeroBlobs', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FbHeroBlobs],
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(FbHeroBlobs);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
