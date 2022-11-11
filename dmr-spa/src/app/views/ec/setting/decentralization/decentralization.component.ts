import { Component, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { AccountService } from 'src/app/_core/_service/account.service';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { EditService, ToolbarService, PageService, PageSettingsModel, ToolbarItems, GridComponent, QueryCellInfoEventArgs } from '@syncfusion/ej2-angular-grids';
import { RoleService } from 'src/app/_core/_service/role.service';
import { IRole, IUserRole } from 'src/app/_core/_model/role';
import { IUserCreate, IUserUpdate } from 'src/app/_core/_model/user';
import { UserService } from 'src/app/_core/_service/user.service';
import { environment } from 'src/environments/environment';
import { Tooltip } from '@syncfusion/ej2-angular-popups';
import { IBuilding } from 'src/app/_core/_model/building';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { CheckBoxSelectionService, MultiSelectComponent } from '@syncfusion/ej2-angular-dropdowns';
const DISPATCHER = 6;
@Component({
  selector: 'app-decentralization',
  templateUrl: './decentralization.component.html',
  styleUrls: ['./decentralization.component.css'],
  providers: [ToolbarService, EditService, PageService, CheckBoxSelectionService]
})
export class DecentralizationComponent implements OnInit {

  userData: any;
  public mode: string;
  buildings: IBuilding[];
  buildingDatas: IBuilding[];
  fieldsBuilding: object = { text: 'name', value: 'id' };
  fieldsLine: object = { text: 'name', value: 'id' };
  fieldsRole: object = { text: 'name', value: 'name' };
  editSettings = { showDeleteConfirmDialog: false, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Normal' };
  buildingUsers: [];
  user: any;
  password = '';
  userID: number;
  buildingID: number;
  modalReference: NgbModalRef;
  toolbarOptions = ['Search'];
  toolbar = ['Add', 'Edit', 'Delete', 'Update', 'Cancel', 'ExcelExport', 'Search'];
  passwordFake = `aRlG8BBHDYjrood3UqjzRl3FubHFI99nEPCahGtZl9jvkexwlJ`;
  pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
  @ViewChild('grid') public grid: GridComponent;
  @ViewChildren('multiSelectBuildings') public multiSelectBuildings: QueryList<MultiSelectComponent>;
  @ViewChildren('multiSelectLines') public multiSelectLines: QueryList<MultiSelectComponent>;
  roles: IRole[];
  roleID: any;
  userCreate: IUserCreate;
  userUpdate: IUserUpdate;
  setFocus: any;
  locale = localStorage.getItem('lang');
  lines: IBuilding[];
  public fields: object = { text: 'name', value: 'id' };
  lineList = [];
  lineRemovingList = [];
  buildingList = [];
  buildingRemovingList = [];
  checkAll = [];
  public selectAllText = '';
  constructor(
    private accountService: AccountService,
    public modalService: NgbModal,
    private userService: UserService,
    private alertify: AlertifyService,
  ) { }

  ngOnInit() {
    this.selectAllText = 'Select All';
    this.mode = 'CheckBox';
    this.roleID = 0;
    this.buildingID = 0;
    this.getBuildings();
    this.getAllUserInfo();
  }
  rowSelected(args) {
    if (args.isInteracted === true) {
      // this.userID = args.data.id;
      // this.getBuildingByUserID();
      // this.getLineByUserID();
    }
  }
  // api
  onChangeBuilding(args, data) {
    this.userID = data.id;
    this.buildingID = args.itemData.id;
  }
  onChangeRole(args, data) {
    this.userID = data.id;
    this.roleID = args.itemData.id;
  }
  getBuildings() {
    this.accountService.getBuildings().subscribe((result: any) => {
      this.buildingDatas = result || [];
      const data = this.buildingDatas.filter((item: any) => item.level === 2);
      // this.lines = this.buildingDatas.filter((item: any) => item.level === 3);
      // this.buildings = this.buildingDatas.filter((item: any) => item.level === 2);
      this.buildingDatas = data;
    }, error => {
      this.alertify.error(error);
    });
  }
  getLineByUserID() {
    this.accountService.getLineByUserID(this.userID, this.buildingID).subscribe(result => {
      const lines = result.data;
      this.lines = lines;
    }, error => {
      this.alertify.error(error);
    });
  }

  getBuildingByUserID() {
    this.accountService.getBuildingByUserID(this.userID).subscribe(result => {
      const buildings = result.data;
      this.buildings = buildings;
    }, error => {
      this.alertify.error(error);
    });
  }
  mapBuildingUser(userid, buildingid) {
    if (userid !== undefined && buildingid !== undefined) {
      this.accountService.mapBuildingUser(userid, buildingid).subscribe((res: any) => {
        if (res.status) {
          this.alertify.success(res.message);
          this.getAllUserInfo();
          this.buildingID = 0;
        } else {
          this.alertify.success(res.message);
        }
      });
    }
  }
  getAllUserInfo() {
    this.userService.getAllUserInfoRoles().subscribe((users: any) => {
      this.userData = users;
    });
  }
  removing(args, data) {
    this.lineList = this.lineList.filter(item => item !== args.itemData.id);
    this.lineRemovingList.push(args.itemData.id);
    this.userID = data.id;
    const obj = {
      userID: this.userID, buildings: [args.itemData.id]
    };
    this.accountService.removeLineUser(obj).subscribe((res: any) => {
      this.alertify.success('Successfully! Thành công!');
      // this.getLineByUserID();
    }, err => this.alertify.error(err));
  }
  onSelect(args, data) {
    this.lineList.push(args.itemData.id);
    this.userID = data.id;
    const obj = {
      userID: this.userID, buildings: [args.itemData.id]
    };
    this.accountService.mapLineUser(obj).subscribe((res: any) => {
      if (res.status) {
        this.alertify.success(res.message);
        // this.getLineByUserID();
      } else {
        this.alertify.success(res.message);
      }
    });
  }

  removingBuilding(args, data) {
    this.buildingList = this.buildingList.filter(item => item !== args.itemData.id);
    this.userID = data.id;
    this.buildingRemovingList.push(args.itemData.id);
    const obj = {
      userID: this.userID, buildings: [args.itemData.id]
    };
    this.accountService.removeMultipleBuildingUser(obj).subscribe((res: any) => {
      this.alertify.success('Successfully! Thành công!');
      // this.getLineByUserID();
    }, err => this.alertify.error(err));
  }
  onSelectBuilding(args, data) {
    this.buildingList.push(args.itemData.id);
    this.userID = data.id;
    const obj = {
      userID: this.userID, buildings: [args.itemData.id]
    };
    this.accountService.mapMultipleBuildingUser(obj).subscribe((res: any) => {
      if (res.status) {
        this.alertify.success(res.message);
        // this.getLineByUserID();
      } else {
        this.alertify.success(res.message);
      }
    });
  }
  closeLine(index) {
    // const item = this.multiSelectLines.toArray()[+index];
    // item.refresh();
    this.getAllUserInfo();
  }
  closeBuilding(index) {
    this.getAllUserInfo();
    // const item = this.multiSelectBuildings.toArray()[+index];
    // item.refresh();

  }
  // end api
  NO(index) {
    return (this.grid.pageSettings.currentPage - 1) * this.pageSettings.pageSize + Number(index) + 1;
  }
  selectedAll(args) {
    console.log(args);
  }
}
