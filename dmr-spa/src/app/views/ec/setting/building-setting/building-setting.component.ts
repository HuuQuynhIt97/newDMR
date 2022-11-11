import { Component, OnInit, ViewChild } from '@angular/core';
import { GridComponent } from '@syncfusion/ej2-angular-grids';
import { BuildingService } from 'src/app/_core/_service/building.service';
import { SettingService } from 'src/app/_core/_service/setting.service';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { DisplayTextModel } from '@syncfusion/ej2-angular-barcode-generator';

@Component({
  selector: 'app-building-setting',
  templateUrl: './building-setting.component.html',
  styleUrls: ['./building-setting.component.css']
})
export class BuildingSettingComponent implements OnInit {
  data: any;
  public filterSettings: object;
  pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
  editSettings = { showDeleteConfirmDialog: false, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Normal' };
  @ViewChild('gridBuilding') public gridBuilding: GridComponent;
  @ViewChild('gridSetting') public gridSetting: GridComponent;
  fields: object = { text: 'title', value: 'id' };
  toolbarOptions: string[];
  toolbar: string[];
  buildings: object;
  settings: object;
  buildingID: any;
  glueTypes: any;
  glueTypeId: number;
  qrcode: string;
  public displayTextMethod: DisplayTextModel = {
    visibility: false
  };
  index: any;
  constructor(
    private buildingService: BuildingService,
    private alertify: AlertifyService,
    public modalService: NgbModal,

    private settingService: SettingService
  ) { }

  ngOnInit(): void {
    this.filterSettings = { type: 'Excel' };
    this.toolbar = ['Excel Export', 'Search'];
    this.loadData();
    this.getAllGlueType();
  }
  openPrintModal(ref, data) {
    this.modalService.open(ref, { size: 'md', backdrop: 'static', keyboard: false });
    this.qrcode = data.machineCode;
  }
  printData() {
    let html = '';
    const printContent = document.getElementById('qrcode');
    html += `
       <div class='content'>
        <div class='qrcode'>
         ${printContent.innerHTML}
         </div>
          <div class='info'>
          <ul>
              <li style="color: transparent;" class='subInfo'>a</li>
              <li style="color: transparent;" class='subInfo'>a</li>
              <li class='subInfo'>QR Code: ${this.qrcode}</li>
              <li style="color: transparent;" class='subInfo'>a</li>
              <li style="color: transparent;" class='subInfo'>a</li>
          </ul>
         </div>
      </div>
      `;
    this.configurePrint(html);
  }
  configurePrint(html) {
    const WindowPrt = window.open('', '_blank', 'left=0,top=0,width=1000,height=900,toolbar=0,scrollbars=0,status=0');
    WindowPrt.document.write(`
    <html>
      <head>
      </head>
      <style>
        * {
          box-sizing: border-box;
          -moz-box-sizing: border-box;
        }

        .content {
          page-break-after: always;
          clear: both;
        }

        .content .qrcode {
          float:left;
          width: 100px;
          margin-top: 10px;
          padding: 0;
          margin-left: 0px;
        }

        .content .info {
          float:left;
          list-style: none;
          width: 200px;
        }
        .content .info ul {
          float:left;
          list-style: none;
          padding: 0px;
          margin: 0px;
          margin-top: 20px;
          font-weight: bold;
          word-wrap: break-word;
        }

        @page {
          size: 2.65 1.20 in;
          page-break-after: always;
          margin: 0;
        }
        @media print {
          html, body {
            width: 90mm; // Chi co nhan millimeter
          }
        }
      </style>
      <body onload="window.print(); window.close()">
        ${html}
      </body>
    </html>
    `);
    WindowPrt.document.close();
  }
  /// api
  getAllGlueType() {
    return this.settingService.getAllGlueType().toPromise();
  }
  getBuildingForSetting() {
    return this.buildingService.getBuildingsForSetting().toPromise();
   }
  getSettingByBuilding(buildingID) {
    return this.settingService.getSettingByBuilding(buildingID).toPromise();
  }
  editSetting(model) {
    return this.settingService.updateSetting(model).toPromise();
  }
  createSetting(model) {
    return this.settingService.addSetting(model).toPromise();
  }
  deleteSetting(id) {
    return this.settingService.deleteSetting(id).toPromise();
  }
  onSelectGlueType(args) {
    this.glueTypeId = +args.itemData.id;
  }
  /// end api
  async loadData() {
    try {
      this.buildings = await this.getBuildingForSetting();
      this.glueTypes = ((await this.getAllGlueType()) as []).filter( (item: any) => item.level === 1);
    } catch (error) {
      this.alertify.error(error + '');
    }
  }

  async edit(data) {
    const model = {
      id: data.id,
      minRPM: data.minRPM,
      maxRPM: data.maxRPM,
      glueTypeID: this.glueTypeId,
      machineType: data.machineType,
      machineCode: data.machineCode,
      buildingID: this.buildingID
    };
    try {
      await this.editSetting(model);
      this.alertify.success('Success');
      this.settings = await this.getSettingByBuilding(this.buildingID);
    } catch (error) {
      this.alertify.error(error + '');
    }
  }
  async add(data) {
    const model = {
      id: 0,
      minRPM: data.minRPM,
      maxRPM: data.maxRPM,
      glueTypeID: this.glueTypeId,
      machineType: data.machineType,
      machineCode: data.machineCode,
      buildingID: this.buildingID
    };
    try {
      await this.createSetting(model);
      this.alertify.success('Success');
      this.settings = await this.getSettingByBuilding(this.buildingID);
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
     this.toolbarOptions = ['Add', 'Edit', 'Delete', 'Cancel', 'Excel Export', 'Search'];
     this.buildingID = args.data.id;
     this.settings = await this.getSettingByBuilding(this.buildingID);
    }
    this.index = [args.rowIndex];
  }
  toolbarClick(args): void {
    switch (args.item.text) {
      /* tslint:disable */
      case 'Excel Export':
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
      case 'Excel Export':
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
}
