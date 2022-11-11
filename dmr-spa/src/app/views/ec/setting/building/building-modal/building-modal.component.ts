import { SystemConstant } from 'src/app/_core/_constants';
import { IBuilding } from 'src/app/_core/_model/building';
import { Component, OnInit, Input } from '@angular/core';
import { BuildingService } from 'src/app/_core/_service/building.service';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { KindService } from 'src/app/_core/_service/kind.service';
import { Thickness } from '@syncfusion/ej2-charts';

@Component({
  selector: 'app-building-modal',
  templateUrl: './building-modal.component.html',
  styleUrls: ['./building-modal.component.css']
})
export class BuildingModalComponent implements OnInit {
  @Input() title: string;
  @Input() building: IBuilding;
  @Input() parent: IBuilding;
  kindID: any;
  kinds: any;
  isLine: boolean;
  fieldsKindEdit: object = { text: 'name', value: 'name' };
  fieldsBuildingType: object = { text: 'name', value: 'name' };
  buildingTypeID: number;
  buildingTypeData: object;
  isShowBuildingType: boolean;
  constructor(
    public activeModal: NgbActiveModal,
    private buildingService: BuildingService,
    private alertify: AlertifyService,
    private kindService: KindService,

  ) { }

  ngOnInit() {
    this.isLine = false;
    this.isShowBuildingType = false;
    console.log(this.parent);
    if (this.parent !== null && this.parent?.level === SystemConstant.BUILDING_LEVEL
      && this.parent?.buildingType?.id === 2
      ) {
      this.isLine = true;
    } else {
      this.isLine = false;
    }
    if (this.parent === null || this.parent?.level === SystemConstant.ROOT_LEVEL) {
      this.isShowBuildingType = true;
    } else {
      this.isShowBuildingType = false;
    }
    this.getAllKind();
    this.getAllBuildingType();
  }
  validation() {
    if (this.building.name === '') {
      this.alertify.warning('Please enter building name!', true);
      return false;
    } else {
      return true;
    }
  }
  onChangeKindEdit(args) {
    this.kindID = args.itemData.id;
  }
  getAllKind() {
    this.kindService.getAllKind().subscribe((res) => {
      this.kinds = res;
    });
  }
  createBuilding() {
    if (this.validation()) {
      if (this.building.parentID > 0) {
        this.building.kindID = this.kindID;
        this.building.buildingTypeID = this.buildingTypeID;
        this.buildingService.createSubBuilding(this.building).subscribe(res => {
          this.alertify.success('The building has been created!!');
          this.activeModal.dismiss();
          this.buildingService.changeMessage(200);
        });
      } else {
        // this.building.buildingTypeID = this.buildingTypeID;
        this.buildingService.createMainBuilding(this.building).subscribe(res => {
          this.buildingService.changeMessage(200);
          this.alertify.success('The building has been created!!');
          this.activeModal.dismiss();
        });
      }
    }
  }

  onChangeBuildingType(args) {
    this.buildingTypeID = args.itemData.id;
  }
  getAllBuildingType() {
    this.buildingService.getAllBuildingType().subscribe((res) => {
      this.buildingTypeData = res;
    });
  }
}
