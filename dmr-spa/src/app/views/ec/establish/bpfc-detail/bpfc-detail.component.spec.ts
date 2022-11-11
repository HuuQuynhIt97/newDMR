import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BpfcDetailComponent } from './bpfc-detail.component';

describe('BpfcDetailComponent', () => {
  let component: BpfcDetailComponent;
  let fixture: ComponentFixture<BpfcDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BpfcDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BpfcDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
