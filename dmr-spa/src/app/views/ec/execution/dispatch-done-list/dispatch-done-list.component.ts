import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { GridComponent } from '@syncfusion/ej2-angular-grids';
import { IBuilding } from 'src/app/_core/_model/building.js';
import { IRole } from 'src/app/_core/_model/role.js';
import { AlertifyService } from 'src/app/_core/_service/alertify.service.js';
import { DispatchService } from 'src/app/_core/_service/dispatch.service.js';
import { PlanService } from 'src/app/_core/_service/plan.service.js';
import { SettingService } from 'src/app/_core/_service/setting.service.js';
import { TodolistService } from 'src/app/_core/_service/todolist.service.js';
@Component({
  selector: 'app-dispatch-done-list',
  templateUrl: './dispatch-done-list.component.html',
  styleUrls: ['./dispatch-done-list.component.css']
})
export class DispatchDoneListComponent implements OnInit {
  @ViewChild('dispatchGrid')
  dispatchGrid: GridComponent;
  @Input() value: any;
  @Input() buildingSetting: any;
  toolbarOptions: any;
  filterSettings: { type: string; };
  fieldsLine: object = { text: 'line', value: 'line' };
  editSettings = { showDeleteConfirmDialog: false, allowEditing: false, allowAdding: false, allowDeleting: false, mode: 'Normal' };
  setFocus: any;
  title: string;
  data: any[];
  buildingID: number;
  role: IRole;
  building: IBuilding;
  startDispatchingTime: any;
  finishDispatchingTime: any;
  mixedConsumption: number;
  unitTitle: string;
  line: any;
  qrCode: string;
  user: any;
  isShow: boolean;
  isSingleGlue: boolean;
  sortSettings = { columns: [{ field: 'lineName', direction: 'Ascending' }] };
  constructor(
    public activeModal: NgbActiveModal,
    public settingService: SettingService,
    public dispatchService: DispatchService,
    public planService: PlanService,
    public todolistService: TodolistService,
    public alertify: AlertifyService

  ) { }

  ngOnInit() {
    this.isSingleGlue = !this.value.glueName.includes(' + ');
    this.toolbarOptions = [
      'Edit', 'Cancel', 'Search'];
    this.title = this.value.glueName;
    const BUIDLING: IBuilding = JSON.parse(localStorage.getItem('building'));
    const ROLE: IRole = JSON.parse(localStorage.getItem('level'));
    const USER = JSON.parse(localStorage.getItem('user')).user;
    this.role = ROLE;
    this.user = USER;
    this.building = BUIDLING;
    if (this.value.glueName.includes(' + ')) {
      this.unitTitle = 'Actual Consumption';
      this.mixedConsumption = +(this.value.mixedConsumption * 1000).toFixed(0);
    } else {
      this.unitTitle = 'Standard Consumption';
      this.mixedConsumption = +(this.value.standardConsumption * 1000).toFixed(0);
    }
    this.loadData();
    this.startDispatchingTime = new Date();
    this.isShow = this.value.mixingInfoID === 0 && !this.value.glueName.includes(' + ');
  }
  actionBegin(args) {
    if (args.requestType === 'save' && args.action === 'edit') {
      const data = args.data;
      const obj =
      {
        id: data.id,
        amount: data.amount,
        glueNameID: this.value.glueNameID,
        estimatedStartTime: this.value.estimatedStartTime,
        estimatedFinishTime: this.value.estimatedFinishTime,
        startTimeOfPeriod: this.value.startTimeOfPeriod,
        finishTimeOfPeriod: this.value.finishTimeOfPeriod,
        lineID: data.lineID
      }
        ;
      this.updateDispatchDetail(obj);
    }
  }
  actionComplete(e) {
    // if (e.requestType === 'beginEdit') {
    //   const input = (e.form.elements.namedItem('amount') as HTMLInputElement);
    //   input.focus();
    //   if (input.value === '0') {
    //     input.select();
    //   }
    // }
  }
  toolbarClick(args) {
  }
  loadData() {
    this.todolistService.getDispatchDetail(this.value.buildingID, this.value.glueNameID,
      this.value.estimatedStartTime, this.value.estimatedFinishTime).subscribe((data: any) => {
        this.data = data;
      });
  }
  finishDispatch() {
    const lines = this.data.map(x => {
      return x.lineID;
    });
    const obj = {
      glueNameID: this.value.glueNameID,
      estimatedStartTime: this.value.estimatedStartTime,
      estimatedFinishTime: this.value.estimatedFinishTime,
      lines
    };
    this.todolistService.finishDispatch(obj).subscribe((data: any) => {
      this.todolistService.setValue(true);
      this.alertify.success('Success');
      this.activeModal.dismiss();
    });
  }
  updateDispatchDetail(obj) {
    this.todolistService.updateDispatchDetail(obj).subscribe((res: any) => {
      this.loadData();
      this.alertify.success('Success');
    }, error => {
      this.alertify.warning(error);
    });
  }
  updateStartDispatchingTime(id: number) {
    this.dispatchService.updateStartDispatchingTime(id).subscribe((res) => {
    }, error => {
      this.alertify.warning(error);
    });
  }
  save() {
    this.finishDispatch();
  }
}
