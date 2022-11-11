/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { Bpfc1Component } from './bpfc-1.component';

describe('Bpfc-1Component', () => {
  let component: Bpfc1Component;
  let fixture: ComponentFixture<Bpfc1Component>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ Bpfc1Component ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(Bpfc1Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
