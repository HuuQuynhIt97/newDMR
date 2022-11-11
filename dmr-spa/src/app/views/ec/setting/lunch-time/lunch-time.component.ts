import { BaseComponent } from 'src/app/_core/_component/base.component';
import { Component, OnInit, ViewChild } from '@angular/core';
import { GridComponent } from '@syncfusion/ej2-angular-grids';
import { LunchTimeService } from 'src/app/_core/_service/lunch.time.service';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { LunchTime } from 'src/app/_core/_model/lunch.time';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-lunch-time',
  templateUrl: './lunch-time.component.html',
  styleUrls: ['./lunch-time.component.css']
})
export class LunchTimeComponent extends BaseComponent implements OnInit {

  lunchTime: LunchTime;
  data: any;
  editSettings = { showDeleteConfirmDialog: false, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Normal' };
  toolbarOptions = ['ExcelExport', 'Add', 'Edit', 'Delete', 'Cancel', 'Search'];
  @ViewChild('grid') grid: GridComponent;
  pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
  filterSettings = { type: 'Excel' };
  endTime: Date;
  startTime: Date;
  constructor(
    private lunchTimeService: LunchTimeService,
    private alertify: AlertifyService,
    private route: ActivatedRoute,
  ) { super(); }

  ngOnInit() {
    this.Permission(this.route);
    this.lunchTime = {} as LunchTime;
    this.getAllLunchTime();
  }
  // api
  getAllLunchTime() {
    this.lunchTimeService.getAllLunchTime().subscribe(res => {
      this.data = res;
    });
  }
  create() {
    this.lunchTimeService.create(this.lunchTime).subscribe(() => {
      this.alertify.success('Add LunchTime Successfully');
      this.getAllLunchTime();
      this.lunchTime = {} as LunchTime;
    });
  }

  update() {
    this.lunchTimeService.update(this.lunchTime).subscribe(() => {
      this.alertify.success('Add LunchTime Successfully');
      // this.modalReference.close() ;
      this.getAllLunchTime();
      this.lunchTime = {} as LunchTime;
    });
  }
  delete(id) {
    this.alertify.confirm('Delete LunchTime', 'Are you sure you want to delete this LunchTime "' + id + '" ?', () => {
      this.lunchTimeService.delete(id).subscribe(() => {
        this.getAllLunchTime();
        this.alertify.success('The lunchTime has been deleted');
      }, error => {
        this.alertify.error('Failed to delete the lunchTime');
      });
    });
  }
  // end api

  // grid event
  toolbarClick(args): void {
    switch (args.item.text) {
      /* tslint:disable */
      case 'Excel Export':
        this.grid.excelExport();
        break;
      /* tslint:enable */
      default:
        break;
    }
  }
  onChangeStartTime(value: Date): void {
    this.startTime = value;

  }
  onChangeEndTime(value: Date): void {
    this.endTime = value;

  }

  actionBegin(args) {
    if (args.requestType === 'save') {
      if (args.action === 'add') {
        this.lunchTime.startTime = this.startTime;
        this.lunchTime.endTime = this.endTime;
        this.create();
      }
      if (args.action === 'edit') {
        this.lunchTime = args.data;
        this.lunchTime.startTime = this.startTime;
        this.lunchTime.endTime = this.endTime;
        this.update();
      }
    }
    if (args.requestType === 'delete') {
      this.delete(args.data[0].id);
    }
  }
  actionComplete(e: any): void {
    if (e.requestType === 'edit') {
      (e.form.elements.namedItem('startTime') as HTMLInputElement).focus();
      (e.form.elements.namedItem('id') as HTMLInputElement).disabled = true;
    }
  }
  // end event
  NO(index) {
    return (this.grid.pageSettings.currentPage - 1) * this.pageSettings.pageSize + Number(index) + 1;
  }
}
