import { BaseComponent } from 'src/app/_core/_component/base.component';
import { LogLevel } from '@microsoft/signalr';
import { EmitType } from '@syncfusion/ej2-base';
import { Component, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { BuildingService } from 'src/app/_core/_service/building.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { BuildingModalComponent } from './building-modal/building-modal.component';
import { TreeGridComponent } from '@syncfusion/ej2-angular-treegrid/';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { SortService, FilterService, ReorderService, ITreeData } from '@syncfusion/ej2-angular-treegrid/';
import { HierarchyNode, IBuilding } from 'src/app/_core/_model/building';
import { KindService } from 'src/app/_core/_service/kind.service';
import { QueryCellInfoEventArgs } from '@syncfusion/ej2-angular-grids';
import { SystemConstant } from 'src/app/_core/_constants';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-building',
  templateUrl: './building.component.html',
  styleUrls: ['./building.component.css'],
  providers: [FilterService, SortService, ReorderService],
  encapsulation: ViewEncapsulation.None
})
export class BuildingComponent extends BaseComponent implements OnInit {
  toolbar: object;
  data: Array<HierarchyNode<IBuilding>>;
  editing: any;
  contextMenuItems: any;
  pageSettings: any;
  editparams: { params: { format: string; }; };
  @ViewChild('treegrid')
  treeGridObj: TreeGridComponent;
  @ViewChild('buildingModal')
  buildingModal: any;
  building: IBuilding = {} as IBuilding;
  parent: IBuilding = {} as IBuilding;
  edit: IBuilding = {} as IBuilding;
  kindID: any;
  kinds: object;
  buildingTypeData: any;
  buildingTypeID: any;
  fieldsKindEdit: object = { text: 'name', value: 'id' };
  fieldsBuildingType: object = { text: 'name', value: 'id' };
  public queryCellInfoEvent: EmitType<QueryCellInfoEventArgs> = (args: QueryCellInfoEventArgs) => {
    const entity = 'entity';
    const data: IBuilding = args.data[entity];
    switch (data.level) {
      case SystemConstant.ROOT_LEVEL:
        if (args.column.field === 'entity.buildingType.name' || args.column.field === 'entity.kindName') {
          args.colSpan = 2;
        }
        break;
      case SystemConstant.BUILDING_LEVEL:
        if (args.column.field === 'entity.buildingType.name' || args.column.field === 'entity.kindName') {
          args.colSpan = 3;
        }
        break;
      case SystemConstant.LINE_LEVEL:
        if (args.column.field === 'entity.name' ) {
          args.colSpan = 2;
        }
        break;
    }
  }

  constructor(
    private buildingService: BuildingService,
    private modalService: NgbModal,
    private kindService: KindService,

    private alertify: AlertifyService,
    private route: ActivatedRoute,
  ) { super(); }

  ngOnInit() {
    this.PermissionForTreeGrid(this.route);
    this.editing = { allowDeleting: true, allowEditing: true, mode: 'Row' };
    this.toolbar = ['Add', 'Delete', 'Search', 'Update', 'Cancel'];
    this.optionTreeGrid();
    this.onService();
    this.getAllKind();
    this.getAllBuildingType();
    this.getBuildingsAsTreeView();
  }
  optionTreeGrid() {
    this.pageSettings = { pageSize: 20 };
    this.editparams = { params: { format: 'n' } };
  }
  created() {
    this.getBuildingsAsTreeView();
  }
  onService() {
    this.buildingService.currentMessage
      .subscribe(arg => {
        if (arg === 200) {
          this.getBuildingsAsTreeView();
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
  contextMenuClick(args) {
    const data = args.rowInfo.rowData.entity;
    this.building = {} as IBuilding;
    switch (args.item.id) {
      case 'DeleteOC':
        this.delete(args.rowInfo.rowData.entity.id);
        break;
      case 'Add-Sub-Item':
        this.building.parentID = data.id; // Gán ID hiện tại là cha của record đc tạo
        this.building.level = data.level; // Gán ID hiện tại là cha của record đc tạo
        this.parent = data;
        this.openSubModal();
        break;
      default:
        break;
    }
  }
  delete(id) {
    this.alertify.confirm(
      'Delete Project',
      'Are you sure you want to delete this BuildingID "' + id + '" ?',
      () => {
        this.buildingService.delete(id).subscribe(res => {
          this.getBuildingsAsTreeView();
          this.treeGridObj.refresh();
          this.alertify.success('The building has been deleted!!!');
        },
        error => {
          this.alertify.error('Failed to delete the building!!!');
        });
      }
    );
   }
  actionComplete(args) {
    // if (args.requestType === 'save') {
    //   this.edit = { ...args.data.entity } ;
    //   this.edit.kindID = this.kindID;
    //   this.edit.buildingTypeID = this.buildingTypeID;
    //   this.rename();
    // }
   }
  actionBegin(args) {
    if (args.requestType === 'beginEdit') {
      const item = args.rowData;
      this.buildingTypeID = item?.buildingType?.id;
    }
    if (args.requestType === 'save' && args.action === 'edit') {
      this.edit = { ...args.data.entity };
      this.edit.kindID = this.kindID;
      this.edit.buildingTypeID = this.buildingTypeID;
      this.rename();
    }
  }
  getBuildingsAsTreeView() {
    this.buildingService.getBuildingsAsTreeView().subscribe(res => {
      this.data = res;
    });
  }
  clearFrom() {
    this.building = {} as IBuilding;
  }
  rename() {
    this.buildingService.rename(this.edit).subscribe(res => {
      this.getBuildingsAsTreeView();
      this.edit = {} as IBuilding;
      this.kindID = 0;
      this.buildingTypeID = 0;
      this.alertify.success('The building has been changed!!!');
    });
  }
  openMainModal() {
    this.clearFrom();
    const modalRef = this.modalService.open(BuildingModalComponent, { size: 'lg' });
    modalRef.componentInstance.title = 'Add Root';
    modalRef.componentInstance.building = this.building;
    modalRef.result.then((result) => {
    }, (reason) => {
    });
  }
  openSubModal() {
    const modalRef = this.modalService.open(BuildingModalComponent, { size: 'lg' });
    modalRef.componentInstance.title = this.building.level === 1 ? 'Add building' : 'Add Line';
    modalRef.componentInstance.building = this.building;
    modalRef.componentInstance.parent = this.parent;
    modalRef.result.then((result) => {
    }, (reason) => {
    });
  }
  onChangeKindEdit(args) {
    this.kindID = args.itemData.id;
  }
  getAllKind() {
    this.kindService.getAllKind().subscribe((res) => {
      this.kinds = res;
    });
  }

  onChangeBuildingType(args) {
    this.buildingTypeID = args.itemData.id;
  }
  getAllBuildingType() {
    this.buildingService.getAllBuildingType().subscribe((res) => {
      this.buildingTypeData = res;
    });
  }
}
