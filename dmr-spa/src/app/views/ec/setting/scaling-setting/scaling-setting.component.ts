import { Component, OnInit, ViewChild } from '@angular/core';
import { GridComponent } from '@syncfusion/ej2-angular-grids';
import { BuildingService } from 'src/app/_core/_service/building.service';
import { SettingService } from 'src/app/_core/_service/setting.service';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';

@Component({
  selector: 'app-scaling-setting',
  templateUrl: './scaling-setting.component.html',
  styleUrls: ['./scaling-setting.component.css'],
})
export class ScalingSettingComponent implements OnInit {
  data: any;
  public filterSettings: object;
  pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
  editSettings = {
    showDeleteConfirmDialog: false,
    allowEditing: true,
    allowAdding: true,
    allowDeleting: true,
    mode: 'Normal',
  };
  @ViewChild('gridBuilding') public gridBuilding: GridComponent;
  @ViewChild('gridSetting') public gridSetting: GridComponent;
  toolbarOptions: string[];
  fields: object = { text: 'text', value: 'value' };
  buildings: object;
  settings: object;
  buildingID: any;
  qrcode: string;
  toolbar: string[];
  unit: any;
  units = [{ value: 'k', text: 'Kilogram' }, { value: 'g', text: 'Gram' }];
  machine: any;
  WeighingScale30KG = 0;
  WeighingScale3KG = 1;
  machines = ['Weighing Scale 30KG', 'Weighing Scale 3KG'];
  machineType: any;
  constructor(
    private buildingService: BuildingService,
    private alertify: AlertifyService,
    private settingService: SettingService
  ) {}

  ngOnInit(): void {
    this.filterSettings = { type: 'Excel' };
    this.toolbar = ['Excel Export', 'Search'];
    this.loadData();
  }
  /// api
  getBuildingForSetting() {
    return this.buildingService.getBuildingsForSetting().toPromise();
  }

  getSettingByBuilding(buildingID) {
    return this.settingService.getMachineByBuilding(buildingID).toPromise();
  }
  onChange(args) {
    // this.unit = args.itemData.value;
  }
  onChangeMachine(args, data) {
    this.machineType = args.value;
    if (this.machineType === this.machines[this.WeighingScale30KG]) {
      this.unit = 'k';
    } else if (this.machineType === this.machines[this.WeighingScale3KG]) {
      this.unit = 'g';
    } else {
      this.unit = '';
    }
  }
  editSetting(model) {
    return this.settingService.updateMachine(model).toPromise();
  }

  createSetting(model) {
    return this.settingService.addMachine(model).toPromise();
  }

  deleteSetting(id) {
    return this.settingService.deleteMachine(id).toPromise();
  }

  /// end api
  async loadData() {
    try {
      this.buildings = await this.getBuildingForSetting();
    } catch (error) {
      this.alertify.error(error + '');
    }
  }

  async edit(data) {
    const model = {
      id: data.id,
      machineType: this.machineType,
      unit: this.unit,
      buildingID: this.buildingID,
      machineID: this.unit === 'k' ? 2 : 1,
    };
    try {
      await this.editSetting(model);
      this.alertify.success('Success');
      this.settings = await this.getSettingByBuilding(this.buildingID);
      this.unit = '';
      this.machineType = '';
    } catch (error) {
      this.alertify.error(error + '');
    }
  }

  async add(data) {
    const model = {
      id: 0,
      unit: this.unit,
      machineType: this.machineType,
      buildingID: this.buildingID,
      machineID: this.unit === 'k' ? 2 : 1,
    };
    try {
      await this.createSetting(model);
      this.alertify.success('Success');
      this.settings = await this.getSettingByBuilding(this.buildingID);
      this.unit = '';
      this.machineType = '';
    } catch (error) {
      this.alertify.error(error + '');
    }
  }

  async delete(id) {
    try {
      await this.deleteSetting(id);
      this.alertify.success('Success');
      this.settings = await this.getSettingByBuilding(this.buildingID);
    } catch (error) {
      this.alertify.error(error + '');
    }
  }
  /// event
  actionCompleteSetting(e) {
    if (e.requestType === 'add') {
      (e.form.elements.namedItem('#') as HTMLInputElement).disabled = true;
    }
  }

  async actionBeginSetting(args) {
    if (args.requestType === 'beginEdit') {
      const item = args.rowData;
      // this.unit = item.unit;
      this.machineType = item.machineType;
    }
    if (args.requestType === 'save') {
      if (args.action === 'add') {
        await this.add(args.data);
      }
      if (args.action === 'edit') {
        await this.edit(args.data);
      }
    }
    if (args.requestType === 'delete') {
      await this.delete(args.data[0].id);
    }
  }

  async rowSelectedBuilding(args: any) {
    if (args.isInteracted) {
      this.toolbarOptions = [
        'Add',
        'Edit',
        'Delete',
        'Cancel',
        'Excel Export',
        'Search',
      ];
      this.buildingID = args.data.id;
      this.settings = await this.getSettingByBuilding(this.buildingID);
    }
  }

  toolbarClick(args): void {
    switch (args.item.text) {
      /* tslint:disable */
      case "Excel Export":
        this.gridSetting.excelExport();
        break;
      /* tslint:enable */
      case 'PDF Export':
        break;
    }
  }

  toolbarClickBuilding(args): void {
    switch (args.item.text) {
      /* tslint:disable */
      case "Excel Export":
        this.gridBuilding.excelExport();
        break;
      /* tslint:enable */
      case 'PDF Export':
        break;
    }
  }

  /// end event
  NO(index) {
    return +index + 1;
  }
  getUnitItem(value: string) {
    return this.units.filter( x => x.value === value)[0]?.text || "N/A";
  }
}
