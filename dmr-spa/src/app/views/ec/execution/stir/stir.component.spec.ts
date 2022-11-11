import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { StirComponent } from './stir.component';

describe('StirComponent', () => {
  let component: StirComponent;
  let fixture: ComponentFixture<StirComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ StirComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StirComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
