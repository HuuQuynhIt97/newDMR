import { BaseComponent } from 'src/app/_core/_component/base.component';
import { Query } from '@syncfusion/ej2-data/';
import { DataManager, WebApiAdaptor } from '@syncfusion/ej2-data';
import { Component, OnInit, ViewChild } from '@angular/core';
import { GridComponent } from '@syncfusion/ej2-angular-grids';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { PermissionService } from 'src/app/_core/_service/permission.service';
import { environment } from 'src/environments/environment';
import { TreeGridComponent } from '@syncfusion/ej2-angular-treegrid';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-function',
  templateUrl: './function.component.html',
  styleUrls: ['./function.component.css']
})
export class FunctionComponent extends BaseComponent implements OnInit {
  function: any;
  data: any;
  data2: object;
  @ViewChild('grid') grid: GridComponent;
  @ViewChild('treeGrid') treeGrid: TreeGridComponent;
  pageSettings = { pageCount: 20, pageSizes: true, pageSize: 20 };
  fieldsModule: object = { text: 'name', value: 'name' };
  filterSettings = { type: 'Excel' };
  permissionTypeData: object;
  permissionTypeID: any;
  moduleID: any;
  moduleData: object;
  parentID: number;
  level: number;
  parentItem: any;
  constructor(
    private permissionService: PermissionService,
    private alertify: AlertifyService,
    private route: ActivatedRoute,
  ) { super(); }

  ngOnInit() {
    this.PermissionForTreeGrid(this.route);
    this.function = {
    };
  }
  created() {
    this.getFunctionsAsTreeView();
    this.getAllModule();
  }

  // api
  getFunctionsAsTreeView() {
    this.permissionService.getFunctionsAsTreeView().subscribe(res => {
      this.data = res;
    });
  }
  getAllModule() {
    this.permissionService.getAllModule().subscribe(res => {
      this.moduleData = res;
    });
  }
  create() {
    this.permissionService.createFunction(this.function).subscribe(() => {
      this.alertify.success('Add Function Successfully');
      this.getFunctionsAsTreeView();
      this.function = {
      };
      this.moduleID = 0;
      this.parentID = null;
      this.level = 0;
      this.parentItem = {};
    });
  }

  update() {
    this.permissionService.updateFunction(this.function).subscribe(() => {
      this.alertify.success('Add Function Successfully');
      this.getFunctionsAsTreeView();
      this.function = {
      };
      this.moduleID = 0;
      this.parentID = null;
      this.level = 0;
      this.parentItem = {};
    });
  }
  delete(id) {
    this.alertify.confirm2('Delete function', 'Are you sure you want to delete this function "' + id + '" ?', () => {
      this.permissionService.deleteFunction(id).subscribe(() => {
        this.getFunctionsAsTreeView();
        this.parentID = null;
        this.level = null;
        this.parentItem = {};
        this.alertify.success('The function has been deleted');
      }, error => {
        this.alertify.error('Failed to delete the function');
      });
    }, () => {
      this.parentID = null;
      this.level = null;
      this.parentItem = {};
      this.getFunctionsAsTreeView();
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
  rowSelected(args) {
    if (args.isInteracted) {
      const item = args.data.entity;
      this.parentID = +item?.id;
      this.level = Number(item.level) + 1;
      this.moduleID = item?.moduleID;
      this.parentItem = item;
    }
  }
  actionComplete(args) {
  }
  actionBegin(args) {
    if (args.requestType === 'beginEdit') {
      const item = args.rowData.entity;
      this.moduleID = item?.moduleID;
      this.parentID = item?.parentID;
      this.level = item?.level;
    }
    if (args.requestType === 'add' && args.type === "actionBegin") {
      // this.moduleID = args.rowData?.moduleID;
      // this.parentID = args.rowData?.parentID;
      // this.level = args.rowData?.level;
      args.data.entity.module = this.parentItem.module;
    }
    if (args.requestType === 'save' && args.action === 'add') {
      const item = args.data.entity;
      this.function.id = 0;
      this.function.name = item.name;
      this.function.code = item.code;
      this.function.url = item.url;
      this.function.level = this.level || 1;
      this.function.sequence = item.sequence || 0;
      this.function.moduleID = this.moduleID || null;
      this.function.parentID = this.parentID || null;
      console.log('Add item: ', this.function);
      this.create();
    }
    if (args.requestType === 'save' && args.action === 'edit') {
      const item = args.data.entity;
      this.function.id = item.id;
      this.function.name = item.name;
      this.function.code = item.code;
      this.function.url = item.url;
      this.function.level = this.level;
      this.function.parentID = this.parentID;
      this.function.sequence = item.sequence;
      this.function.moduleID = this.moduleID || null;
      // this.function.moduleID = this.moduleID;.
      console.log('update item: ', this.function);
      this.update();
    }
    if (args.requestType === 'delete') {
      this.delete(this.parentID);
    }
  }

  // end event
  NO(index) {
    return (this.treeGrid.pageSettings.currentPage - 1) * this.pageSettings.pageSize + Number(index) + 1;
  }
  onChangeModule(args) {
    this.moduleID = args.itemData.id;
  }
}
