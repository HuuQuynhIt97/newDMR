/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { SubpackageComponent } from './subpackage.component';

describe('SubpackageComponent', () => {
  let component: SubpackageComponent;
  let fixture: ComponentFixture<SubpackageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SubpackageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SubpackageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
