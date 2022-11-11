import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { IncomingComponent } from './incoming.component';

describe('IncomingComponent', () => {
  let component: IncomingComponent;
  let fixture: ComponentFixture<IncomingComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ IncomingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(IncomingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
