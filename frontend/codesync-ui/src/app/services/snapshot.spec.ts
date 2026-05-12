import { TestBed } from '@angular/core/testing';

import { Snapshot } from './snapshot';

describe('Snapshot', () => {
  let service: Snapshot;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Snapshot);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
