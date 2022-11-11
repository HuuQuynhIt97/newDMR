import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { GridComponent } from '@syncfusion/ej2-angular-grids';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { VersionService } from 'src/app/_core/_service/version.service';

@Component({
  selector: 'app-version',
  templateUrl: './version.component.html',
  styleUrls: ['./version.component.scss']
})
export class VersionComponent implements OnInit {
  version: any;
  data: any;
  editSettings = { showDeleteConfirmDialog: false, allowEditing: false, allowAdding: true, allowDeleting: true, mode: 'Normal' };
  toolbarOptions = ['Add', 'Delete', 'Search'];
  @ViewChild('grid') grid: GridComponent;
  pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
  fieldsVersionType: object = { text: 'name', value: 'name' };
  filterSettings = { type: 'Excel' };
  versionTypeData: object;
  versionTypeID: any;
  constructor(
    private versionService: VersionService,
    private alertify: AlertifyService,
    private router: Router,
  ) { }

  ngOnInit() {
    this.version = {
    };
    this.getAllVersion();
  }
  // api

  getAllVersion() {
    this.versionService.getAllVersion().subscribe(res => {
      this.data = res;
    });
  }

  create() {
    this.versionService.create(this.version).subscribe(() => {
      this.alertify.success('Add Version Successfully');
      this.getAllVersion();
      this.version = {
      };
    });
  }

  update() {
    this.versionService.update(this.version).subscribe(() => {
      this.alertify.success('Add Version Successfully');
      // this.modalReference.close() ;
      this.getAllVersion();
      this.version = {
      };
    });
  }
  delete(id) {
    this.alertify.confirm('Delete version', 'Are you sure you want to delete this version "' + id + '" ?', () => {
      this.versionService.delete(id).subscribe(() => {
        this.getAllVersion();
        this.alertify.success('The version has been deleted');
      }, error => {
        this.alertify.error('Failed to delete the version');
      });
    });
  }
  // end api
  edit(data) {
    this.router.navigate(['/ec/system/version/edit/' + data.id]);
  }
  // grid event
  toolbarClick(args): void {
    switch (args.item.text) {
      /* tslint:disable */
      case 'Add':
        this.router.navigate(['/ec/system/version/add']);
        break;
      /* tslint:enable */
      default:
        break;
    }
  }
  actionBegin(args) {
    if (args.requestType === 'save') {
      if (args.version === 'add') {
        this.version.id = 0;
        this.version.name = args.data.name;
        this.version.description = this.data;
        this.create();
      }
      if (args.version === 'edit') {
        this.version.id = args.data.id;
        this.version.name = args.data.name;
        this.version.description = this.data;
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
  onChangeVersionType(args) {
    this.versionTypeID = args.itemData.id;
  }
}
