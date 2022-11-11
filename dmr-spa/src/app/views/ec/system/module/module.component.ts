import { ActivatedRoute } from '@angular/router';
import { BaseComponent } from 'src/app/_core/_component/base.component';
import { Component, OnInit, ViewChild } from '@angular/core';
import { GridComponent } from '@syncfusion/ej2-angular-grids';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { PermissionService } from 'src/app/_core/_service/permission.service';

@Component({
  selector: 'app-module',
  templateUrl: './module.component.html',
  styleUrls: ['./module.component.css']
})
export class ModuleComponent extends BaseComponent implements OnInit {
  module: any;
  data: any;
  @ViewChild('grid') grid: GridComponent;
  pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
  fieldsPermissionType: object = { text: 'name', value: 'name' };
  filterSettings = { type: 'Excel' };
  permissionTypeData: object;
  permissionTypeID: any;
  constructor(
    private permissionService: PermissionService,
    private alertify: AlertifyService,
    private route: ActivatedRoute
  ) { super(); }

  ngOnInit() {
    this.Permission(this.route);
    this.module = {
    };
    this.getAllModule();
  }
  // api
  getAllModule() {
    this.permissionService.getAllModule().subscribe(res => {
      this.data = res;
    });
  }

  create() {
    this.permissionService.createModule(this.module).subscribe(() => {
      this.alertify.success('Add Module Successfully');
      this.getAllModule();
      this.module = {
      };
    });
  }

  update() {
    this.permissionService.updateModule(this.module).subscribe(() => {
      this.alertify.success('Add Module Successfully');
      // this.modalReference.close() ;
      this.getAllModule();
      this.module = {
      };
    });
  }
  delete(id) {
    this.alertify.confirm('Delete Module', 'Are you sure you want to delete this Module "' + id + '" ?', () => {
      this.permissionService.deleteModule(id).subscribe(() => {
        this.getAllModule();
        this.alertify.success('The Module has been deleted');
      }, error => {
        this.alertify.error('Failed to delete the Module');
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
        this.module.id = 0;
        this.module.name = args.data.name;
        this.module.sequence = args.data.sequence;
        this.create();
      }
      if (args.action === 'edit') {
        this.module.id = args.data.id;
        this.module.name = args.data.name;
        this.module.sequence = args.data.sequence;
        this.update();
      }
    }
    if (args.requestType === 'delete') {
      this.delete(args.data[0].id);
    }
  }
  actionComplete(e: any): void {
    if (e.requestType === 'add') {
      (e.form.elements.namedItem('name') as HTMLInputElement).focus();
      (e.form.elements.namedItem('id') as HTMLInputElement).disabled = true;
    }
  }
  // end event
  NO(index) {
    return (this.grid.pageSettings.currentPage - 1) * this.pageSettings.pageSize + Number(index) + 1;
  }
  onChangePermissionType(args) {
    this.permissionTypeID = args.itemData.id;
  }
}
