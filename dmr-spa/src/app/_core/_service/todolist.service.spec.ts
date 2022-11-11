/* tslint:disable:no-unused-variable */

import { TestBed, inject, waitForAsync } from '@angular/core/testing';
import { TodolistService } from './todolist.service';

describe('Service: Todolist', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [TodolistService]
    });
  });

  it('should ...', inject([TodolistService], (service: TodolistService) => {
    expect(service).toBeTruthy();
  }));
});
