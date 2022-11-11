import { Component, OnInit, ViewChild } from '@angular/core';
import { GridComponent } from '@syncfusion/ej2-angular-grids';
import { KindService } from 'src/app/_core/_service/kind.service';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { BaseComponent } from 'src/app/_core/_component/base.component';

@Component({
  selector: 'app-kind',
  templateUrl: './kind.component.html',
  styleUrls: ['./kind.component.css']
})
export class KindComponent extends BaseComponent implements OnInit {
  kind: any;
  data: any;
  @ViewChild('grid') grid: GridComponent;
  pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
  fieldsKindType: object = { text: 'name', value: 'name' };
  filterSettings = { type: 'Excel' };
  kindTypeData: object;
  kindTypeID: any;
  constructor(
    private kindService: KindService,
    private alertify: AlertifyService,
    private route: ActivatedRoute
  ) {
    super();
  }

  ngOnInit() {
    this.Permission(this.route);
    this.kind = {
      id: 0,
      name: '',
      kindTypeID: null
    };
    this.getAllKind();
    this.getAllKindType();
  }
  // api
  getAllKind() {
    this.kindService.getAllKind().subscribe(res => {
      this.data = res;
    });
  }
  getAllKindType() {
    this.kindService.getAllKindType().subscribe(res => {
      this.kindTypeData = res;
    });
  }
  create() {
    this.kindService.create(this.kind).subscribe(() => {
      this.alertify.success('Add Kind Successfully');
      this.getAllKind();
      this.kind = {
        id: 0,
        name: '',
        kindTypeID: null
      };
      this.kindTypeID = 0;
    });
  }

  update() {
    this.kindService.update(this.kind).subscribe(() => {
      this.alertify.success('Add Kind Successfully');
      // this.modalReference.close() ;
      this.getAllKind();
      this.kind = {
        id: 0,
        name: '',
        kindTypeID: null
      };
      this.kindTypeID = 0;
    });
  }
  delete(id) {
    this.alertify.confirm('Delete Kind', 'Are you sure you want to delete this Kind "' + id + '" ?', () => {
      this.kindService.delete(id).subscribe(() => {
        this.getAllKind();
        this.alertify.success('The kind has been deleted');
      }, error => {
        this.alertify.error('Failed to delete the kind');
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
    this.grid.toolbarModule.getToolbar();
    if (args.requestType === 'beginEdit') {
      const item = args.rowData;
      this.kindTypeID = item.kindTypeID;
    }
    if (args.requestType === 'save') {
      if (args.action === 'add') {
        this.kind.id = 0;
        this.kind.name = args.data.name;
        this.kind.kindTypeID = this.kindTypeID;
        this.create();
      }
      if (args.action === 'edit') {
        this.kind.id = args.data.id;
        this.kind.name = args.data.name;
        this.kind.kindTypeID = this.kindTypeID;
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
  onChangeKindType(args) {
    this.kindTypeID = args.itemData.id;
  }
}
