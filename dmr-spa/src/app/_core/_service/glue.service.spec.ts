/* tslint:disable:no-unused-variable */

import { TestBed, inject, waitForAsync } from '@angular/core/testing';
import { GlueService } from './glue.service';

describe('Service: Glue', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [GlueService]
    });
  });

  it('should ...', inject([GlueService], (service: GlueService) => {
    expect(service).toBeTruthy();
  }));
});
