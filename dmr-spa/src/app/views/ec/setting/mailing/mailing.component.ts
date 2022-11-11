import { Component, OnInit, ViewChild } from '@angular/core';
import { MailingService } from 'src/app/_core/_service/mailing.service';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { GridComponent } from '@syncfusion/ej2-angular-grids';
import { IMailing } from 'src/app/_core/_model/mailing';
import { BuildingUserService } from 'src/app/_core/_service/building.user.service';
import { DatePipe } from '@angular/common';
@Component({
  selector: 'app-mailing',
  templateUrl: './mailing.component.html',
  styleUrls: ['./mailing.component.scss'],
  providers: [DatePipe]
})
export class MailingComponent implements OnInit {

  mailing: any;
  data: IMailing[];
  editSettings = { showDeleteConfirmDialog: false, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Normal' };
  toolbarOptions = ['ExcelExport', 'Add', 'Edit', 'Delete', 'Cancel', 'Search'];
  @ViewChild('grid') grid: GridComponent;
  pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
  filterSettings = { type: 'Excel' };
  frequencyOption = ['Daily', 'Weekly', 'Monthly'];
  frequencyItem = '';
  reportOption = ['Done List', 'Cost', 'Achievement Rate'];
  reportItem = '';
  fields: object = { text: 'username', value: 'id' };
  userData: { id: any; username: any; email: any; }[];
  box = 'Box';
  timeSend = new Date();
  datetimeSend = new Date();
  userIDList = [];
  userList = [];
  constructor(
    private mailingService: MailingService,
    private alertify: AlertifyService,
    private datePipe: DatePipe,
    private buildingUserService: BuildingUserService
  ) { }

  ngOnInit() {
    this.mailing = {
      id: 0,
      name: ''
    };
    this.getAllMailing();
  }
  // api
  getAllUsers() {
    this.buildingUserService.getAllUsers(1, 1000).subscribe(res => {
      const data = res.result.map((i: any) => {
        return {
          id: i.id,
          username: i.username,
          email: i.email,
        };
      });
      this.userData = data;
    });
  }
  getAllMailing() {
    this.mailingService.getAllMailing().subscribe(res => {
      this.data = res.map(item => {
        return {
          id: item.id,
          email: item.email,
          userID: item.userID,
          userName: item.userName,
          frequency: item.frequency,
          userNames: item.userNames,
          report: item.report,
          userIDList: item.userIDList,
          userList: item.userList,
          pathName: item.pathName,
          timeSend: new Date(item.timeSend),
        };
      });
      this.getAllUsers();
    });
  }
  create() {
    this.mailingService.create(this.mailing).subscribe(() => {
      this.alertify.success('Add mailing Successfully');
      this.getAllMailing();
      this.mailing.name = '';
    });
  }
  onReport(args) {
    this.reportItem = args.value;
  }
  onFrequency(args) {
    this.frequencyItem = args.value;
  }
  onChangeTimeSend(time: Date) {
    this.timeSend = time;
  }
  onChangeDatetimeSend(args: any) {
    this.datetimeSend = args.value;
  }
  removing(args) {
    const filteredItems = this.userIDList.filter(item => item !== args.itemData.id);
    this.userIDList = filteredItems;
    this.userList = this.userList.filter(item => item.id !== args.itemData.id);
  }
  onSelectUsername(args) {
    const data = args.itemData;
    this.userIDList.push(data.id);
    this.userList.push({ mailingID: 0 , id: data.id, email: data.email});
  }
  update() {
    this.mailingService.update(this.mailing).subscribe(() => {
      this.alertify.success('Add mailing Successfully');
      this.getAllMailing();
      this.mailing.name = '';
    });
  }
  delete(obj) {
    this.alertify.confirm('Delete mailing', 'Are you sure you want to delete this mailings ?', () => {
      this.mailingService.deleteRange(obj).subscribe(() => {
        this.getAllMailing();
        this.alertify.success('The mailing has been deleted');
      }, error => {
        this.alertify.error('Failed to delete the mailing');
      });
    });
  }
  createRange(obj) {
    this.mailingService.createRange(obj).subscribe(() => {
      this.alertify.success('Add mailing Successfully');
      this.getAllMailing();
      this.clearForm();
    }, err => {
        this.alertify.error(err);
        this.getAllMailing();
    });
  }
  clearForm() {
    this.reportItem = '';
    this.frequencyItem = '';
    this.timeSend = new Date();
    this.datetimeSend = new Date();
    this.mailing = {};
  }
  updateRange(obj) {
    this.mailingService.updateRange(obj).subscribe(() => {
      this.alertify.success('Add mailing Successfully');
      this.getAllMailing();
      this.clearForm();
    }, err => {
      this.alertify.error(err);
      this.getAllMailing();
    });
  }
  // end api

  // grid event
  toolbarClick(args): void {
    switch (args.item.id) {
      /* tslint:disable */
      case 'grid_excelexport':
        this.grid.excelExport();
        break;
      /* tslint:enable */
      default:
        break;
    }
  }
  actionBegin(args) {
    if (args.requestType === 'beginEdit') {
      const data = args.rowData;
      this.userIDList = data.userIDList;
      this.reportItem = data.report;
      this.frequencyItem = data.frequency;
      this.timeSend = data.timeSend;
      this.datetimeSend = data.timeSend;
      this.userIDList = data.userIDList;
      this.userList = data.userList || [];
      if (data.frequency === 'Daily') {
        this.timeSend = data.timeSend;
      } else {
        this.datetimeSend = data.timeSend;
      }
    }
    if (args.requestType === 'save') {
      if (args.action === 'add') {
        if (this.userList.length === 0) {
          this.alertify.warning('Please select recipients', true);
          args.cancel = true;
        }
        const obj = [];
        for (const item of this.userList) {
          const data = {
            report: this.reportItem,
            frequency: this.frequencyItem,
            timeSend: this.frequencyItem === 'Daily' || this.frequencyItem === 'Monthly' ?
            this.datePipe.transform(this.timeSend, 'MM-dd-yyyy HH:mm') : this.datePipe.transform(this.datetimeSend, 'MM-dd-yyyy HH:mm'),
            userID: item.id,
            id: item.mailingID,
            email: item.email
          };
          obj.push(data);
        }
        this.createRange(obj);
      }
      if (args.action === 'edit') {
        const itemData = args.data;
        const obj = [];
        for (const item of this.userList) {
          const data = {
            report: this.reportItem,
            frequency: this.frequencyItem,
            timeSend: this.frequencyItem === 'Daily' ?
              this.datePipe.transform(this.timeSend, 'MM-dd-yyyy HH:mm') : this.datePipe.transform(this.datetimeSend, 'MM-dd-yyyy HH:mm'),
            userID: item.id,
            id: item.mailingID,
            email: item.email,
            pathName: itemData.pathName
          };
          obj.push(data);
        }
        this.updateRange(obj);
      }
    }
    if (args.requestType === 'delete') {
      const itemData = args.data[0];
      const obj = [];
      for (const item of itemData.userList) {
        const data = {
          report: itemData.reportItem,
          frequency: itemData.frequencyItem,
          timeSend: itemData.timeSend,
          userID: item.id,
          id: item.mailingID,
          email: item.email,
          pathName: itemData.pathName
        };
        obj.push(data);
      }
      this.delete(obj);
    }
  }
  actionComplete(e: any): void {
    if (e.requestType === 'add') {
      // (e.form.elements.namedItem('name') as HTMLInputElement).focus();
      (e.form.elements.namedItem('id') as HTMLInputElement).disabled = true;
    }
  }
  // end event
  NO(index) {
    return (this.grid.pageSettings.currentPage - 1) * this.grid.pageSettings.pageSize + Number(index) + 1;
  }
}
