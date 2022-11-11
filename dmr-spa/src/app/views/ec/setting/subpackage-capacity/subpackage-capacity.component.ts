import { Component, OnInit, ViewChild } from '@angular/core';
import { SubpackageCapacityService } from 'src/app/_core/_service/subpackage.capacity.service';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { GridComponent } from '@syncfusion/ej2-angular-grids';

@Component({
  selector: 'app-subpackage-capacity',
  templateUrl: './subpackage-capacity.component.html',
  styleUrls: ['./subpackage-capacity.component.css']
})
export class SubpackageCapacityComponent implements OnInit {
  subpackageCapacity: any;
  data: any;
  editSettings = { showDeleteConfirmDialog: false, allowEditing: true, allowAdding: false, allowDeleting: false, mode: 'Normal' };
  toolbarOptions = [];
  @ViewChild('grid') grid: GridComponent;
  pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
  filterSettings = { type: 'Excel' };
  constructor(
    private subpackageCapacityService: SubpackageCapacityService,
    private alertify: AlertifyService,
  ) { }

  ngOnInit() {
    this.subpackageCapacity = {
      id: 0,
      capacity: ''
    };
    this.getAllSubpackageCapacity();
  }
  // api
  getAllSubpackageCapacity() {
    this.subpackageCapacityService.getAllSubpackageCapacity().subscribe(res => {
      this.data = res;
    });
  }
  create() {
    this.subpackageCapacityService.create(this.subpackageCapacity).subscribe(() => {
      this.alertify.success('Add SubpackageCapacity Successfully');
      this.getAllSubpackageCapacity();
      this.subpackageCapacity.name = '';
    });
  }

  update() {
    this.subpackageCapacityService.update(this.subpackageCapacity).subscribe(() => {
      this.alertify.success('Add SubpackageCapacity Successfully');
      this.getAllSubpackageCapacity();
      this.subpackageCapacity.name = '';
    });
  }
  delete(id) {
    this.alertify.confirm('Delete SubpackageCapacity', 'Are you sure you want to delete this SubpackageCapacity "' + id + '" ?', () => {
      this.subpackageCapacityService.delete(id).subscribe(() => {
        this.getAllSubpackageCapacity();
        this.alertify.success('The subpackageCapacity has been deleted');
      }, error => {
        this.alertify.error('Failed to delete the subpackageCapacity');
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
  actionBegin(args) {
    if (args.requestType === 'save') {
      if (args.action === 'add') {
        this.subpackageCapacity.id = 0;
        this.subpackageCapacity.capacity = args.data.capacity;
        this.create();
      }
      if (args.action === 'edit') {
        this.subpackageCapacity.id = args.data.id;
        this.subpackageCapacity.capacity = args.data.capacity;
        this.update();
      }
    }
    if (args.requestType === 'delete') {
      this.delete(args.data[0].id);
    }
  }
  actionComplete(e: any): void {
    if (e.requestType === 'add') {
      (e.form.elements.namedItem('capacity') as HTMLInputElement).focus();
      (e.form.elements.namedItem('id') as HTMLInputElement).disabled = true;
    }
  }
  // end event
  NO(index) {
    return (this.grid.pageSettings.currentPage - 1) * this.grid.pageSettings.pageSize + Number(index) + 1;
  }
}
