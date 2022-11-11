import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ScalingSettingComponent } from './scaling-setting.component';

describe('ScalingSettingComponent', () => {
  let component: ScalingSettingComponent;
  let fixture: ComponentFixture<ScalingSettingComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ScalingSettingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ScalingSettingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
