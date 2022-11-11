import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { SuppilerComponent } from './suppiler.component';

describe('SuppilerComponent', () => {
  let component: SuppilerComponent;
  let fixture: ComponentFixture<SuppilerComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SuppilerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SuppilerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
