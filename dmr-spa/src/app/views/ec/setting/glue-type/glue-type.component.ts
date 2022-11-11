import { BaseComponent } from 'src/app/_core/_component/base.component';
import { Component, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { GlueTypeService } from 'src/app/_core/_service/glue-type.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ToolbarItems, TreeGridComponent } from '@syncfusion/ej2-angular-treegrid';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { SortService, FilterService, ReorderService, ITreeData } from '@syncfusion/ej2-angular-treegrid';
import { HierarchyNode, IGlueType } from 'src/app/_core/_model/glue-type';
import { GlueTypeModalComponent } from './glue-type-modal/glue-type-modal.component';
import { RowDataBoundEventArgs } from '@syncfusion/ej2-angular-grids';
import { BeforeOpenCloseEventArgs } from '@syncfusion/ej2-inputs';
import { ActivatedRoute } from '@angular/router';
@Component({
  selector: 'app-glue-type',
  templateUrl: './glue-type.component.html',
  styleUrls: ['./glue-type.component.css'],
  providers: [FilterService, SortService, ReorderService],
  encapsulation: ViewEncapsulation.None
})
export class GlueTypeComponent extends BaseComponent implements OnInit {
  toolbarOptions: ToolbarItems[];
  data: HierarchyNode<IGlueType>[];
  editing: any;
  methods =  [{ text: 'Stir', value: 'Stir' }, { text: 'Shaking', value: 'Shaking' }];
  pageSettings: any;
  editparams: { params: { format: string; }; };
  @ViewChild('treegrid')
  public treeGridObj: TreeGridComponent;
  @ViewChild('buildingModal')
  buildingModal: any;
  glueType: IGlueType;
  edit: IGlueType;
  method: any;
  fields: object = { text: 'text', value: 'value' };
  constructor(
    private buildingService: GlueTypeService,
    private modalService: NgbModal,
    private alertify: AlertifyService,
    private route: ActivatedRoute,
  ) { super(); }

  ngOnInit() {
    this.PermissionForTreeGrid(this.route);
    this.editing = { allowDeleting: true, allowEditing: true, mode: 'Row' };
    // this.toolbarOptions = ['Add', 'Delete', 'Search', 'Update', 'Cancel'];
    this.optionTreeGrid();
    this.onService();
    this.getGlueTypesAsTreeView();
  }
  onChangeMethod(args) {
    this.method = args.itemData.value;
  }
  optionTreeGrid() {
    this.pageSettings = { pageSize: 20 };
    this.editparams = { params: { format: 'n' } };
  }
  created() {
    this.getGlueTypesAsTreeView();
  }
  onService() {
    this.buildingService.currentMessage
      .subscribe(arg => {
        if (arg === 200) {
          this.getGlueTypesAsTreeView();
        }
      });
  }
  toolbarClick(args) {
    switch (args.item.text) {
      case 'Add':
        args.cancel = true;
        this.openMainModal();
        break;
      case 'PDF Export':
        this.treeGridObj.pdfExport({ hierarchyExportMode: 'All' });
        break;
      case 'Excel Export':
        this.treeGridObj.excelExport({ hierarchyExportMode: 'All' });
        break;
      default:
        break;
    }
  }
  contextMenuOpen(arg): void {
    const data = arg.rowInfo.rowData.entity as IGlueType;
    if (data.level === 2) {
      arg.cancel = true;
    }
  }
  contextMenuClick(args) {
    const data = args.rowInfo.rowData.entity as IGlueType;
    switch (args.item.id) {
      case 'DeleteOC':
        this.delete(data.id);
        break;
      case 'Add-Sub-Item':
        this.openSubModal();
        break;
      default:
        break;
    }
  }
  delete(id) {
    this.alertify.confirm(
      'Delete Glue Type',
      'Are you sure you want to delete this GlueTypeID "' + id + '" ?',
      () => {
        this.buildingService.delete(id).subscribe(res => {
          this.getGlueTypesAsTreeView();
          this.alertify.success('The glue-type has been deleted!!!');
        },
          error => {
            this.alertify.error('Failed to delete the glue-type!!!');
          });
      }
    );
  }
  actionBegin(args) {
    if (args.requestType === 'save' && args.action === 'edit') {
      const data = args.data.entity as IGlueType;
      this.edit.title = data.title;
      this.edit.minutes = data.minutes;
      this.edit.rpm = data.rpm;
      this.edit.level = data.level;
      this.edit.id = data.id;
      this.edit.parentID = data.parentID;
      this.edit.method = this.method;
      this.rename();
    }
    if (args.requestType === 'delete') {
      const data = args.data[0].entity as IGlueType;
      this.delete(data.id);
    }
  }
  rowSelected(args) {
    const data = args.data.entity as IGlueType;

    this.edit = {
      id: data.id,
      title: data.title,
      level: data.level,
      parentID: data.parentID,
      rpm: data.rpm,
      minutes: data.minutes,
      method: this.method
    };
    this.glueType = {
      id: 0,
      title: '',
      parentID: data.id,
      level: data.parentID === null ? data.level + 1 : 1,
      rpm: data.rpm,
      minutes: data.minutes,
      method: this.method
    };
  }
  getGlueTypesAsTreeView() {
    this.buildingService.getGlueTypesAsTreeView().subscribe(res => {
      this.data = res;
    });
  }
  rowDB(args: RowDataBoundEventArgs) {
    const data = args.data as IGlueType;
    if (data.level === 1) {
      // args.row.classList.add('bgcolor');
    }
  }
  clearFrom() {
    this.method = '';
    this.glueType = {
      id: 0,
      title: '',
      parentID: 0,
      level: 0,
      rpm: 0,
      minutes: 0,
      method: this.method
    };
  }
  queryCellInfo(args) {
    if (args.column.field === 'entity.title') {
      if (args.data.entity.level === 1) {
        args.colSpan = 4;
        // merging 2 columns of same row using colSpan property
      }
    }
  }
  rename() {
    this.buildingService.update(this.edit).subscribe(res => {
      this.getGlueTypesAsTreeView();
      this.clearFrom();
      this.alertify.success('The glue-type has been changed!!!');
    });
  }
  openMainModal() {
    this.clearFrom();
    const modalRef = this.modalService.open(GlueTypeModalComponent, { size: 'lg' });
    modalRef.componentInstance.title = 'Add Glue Type Parent';
    modalRef.componentInstance.glueType = this.glueType;
    modalRef.result.then((result) => {
    }, (reason) => {
    });
  }
  openSubModal() {
    const modalRef = this.modalService.open(GlueTypeModalComponent, { size: 'lg' });
    modalRef.componentInstance.title = 'Add Glue Type Child';
    modalRef.componentInstance.glueType = this.glueType;
    modalRef.result.then((result) => {
    }, (reason) => {
    });
  }
}
