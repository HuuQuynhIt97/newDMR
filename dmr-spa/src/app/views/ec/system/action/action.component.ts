import { BaseComponent } from 'src/app/_core/_component/base.component';
import { Component, OnInit, ViewChild } from '@angular/core';
import { GridComponent } from '@syncfusion/ej2-angular-grids';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { PermissionService } from 'src/app/_core/_service/permission.service';
import { ActivatedRoute } from '@angular/router';


@Component({
  selector: 'app-action',
  templateUrl: './action.component.html',
  styleUrls: ['./action.component.css']
})
export class ActionComponent extends BaseComponent implements OnInit {
  action: any;
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
    private route: ActivatedRoute,
  ) { super(); }

  ngOnInit() {
    this.Permission(this.route);
    this.action = {
    };
    this.getAllAction();
  }
  // api

  getAllAction() {
    this.permissionService.getAllAction().subscribe(res => {
      this.data = res;
    });
  }

  create() {
    this.permissionService.createAction(this.action).subscribe(() => {
      this.alertify.success('Add Action Successfully');
      this.getAllAction();
      this.action = {
      };
    });
  }

  update() {
    this.permissionService.updateAction(this.action).subscribe(() => {
      this.alertify.success('Add Action Successfully');
      // this.modalReference.close() ;
      this.getAllAction();
      this.action = {
      };
    });
  }
  delete(id) {
    this.alertify.confirm('Delete action', 'Are you sure you want to delete this action "' + id + '" ?', () => {
      this.permissionService.deleteAction(id).subscribe(() => {
        this.getAllAction();
        this.alertify.success('The action has been deleted');
      }, error => {
        this.alertify.error('Failed to delete the action');
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
        this.action.id = 0;
        this.action.name = args.data.name;
        this.action.code = args.data.code;
        this.create();
      }
      if (args.action === 'edit') {
        this.action.id = args.data.id;
        this.action.name = args.data.name;
        this.action.code = args.data.code;
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
