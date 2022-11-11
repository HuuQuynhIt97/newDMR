/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { MixingComponent } from './mixing.component';

describe('MixingComponent', () => {
  let component: MixingComponent;
  let fixture: ComponentFixture<MixingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MixingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MixingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
