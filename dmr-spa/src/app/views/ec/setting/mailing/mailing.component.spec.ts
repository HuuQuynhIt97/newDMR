/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { MailingComponent } from './mailing.component';

describe('MailingComponent', () => {
  let component: MailingComponent;
  let fixture: ComponentFixture<MailingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MailingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MailingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
