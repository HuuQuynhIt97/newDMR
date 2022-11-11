/* tslint:disable:no-unused-variable */

import { TestBed, inject, waitForAsync } from '@angular/core/testing';
import { JobTypeService } from './jobType.service';

describe('Service: JobType', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [JobTypeService]
    });
  });

  it('should ...', inject([JobTypeService], (service: JobTypeService) => {
    expect(service).toBeTruthy();
  }));
});
