/* tslint:disable:no-unused-variable */

import { TestBed, inject, waitForAsync } from '@angular/core/testing';
import { KindService } from './kind.service';

describe('Service: Kind', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [KindService]
    });
  });

  it('should ...', inject([KindService], (service: KindService) => {
    expect(service).toBeTruthy();
  }));
});
