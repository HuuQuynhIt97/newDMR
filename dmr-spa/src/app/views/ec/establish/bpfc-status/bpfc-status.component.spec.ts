import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { BpfcStatusComponent } from './bpfc-status.component';

describe('BpfcStatusComponent', () => {
  let component: BpfcStatusComponent;
  let fixture: ComponentFixture<BpfcStatusComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ BpfcStatusComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BpfcStatusComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
