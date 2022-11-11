import { BaseComponent } from 'src/app/_core/_component/base.component';
import { Component, OnInit, ViewChild } from '@angular/core';
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
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-account',
  templateUrl: './account.component.html',
  styleUrls: ['./account.component.css'],
  providers: [ToolbarService, EditService, PageService]
})
export class AccountComponent extends BaseComponent implements OnInit {
  userData: any;
  buildings: IBuilding[];
  buildingDatas: IBuilding[];
  fieldsBuilding: object = { text: 'name', value: 'name' };
  fieldsRole: object = { text: 'name', value: 'name' };
  buildingUsers: [];
  user: any;
  password = '';
  userID: number;
  buildingID: number;
  modalReference: NgbModalRef;
  toolbarOptions = ['Search'];
  passwordFake = `aRlG8BBHDYjrood3UqjzRl3FubHFI99nEPCahGtZl9jvkexwlJ`;
  pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
  @ViewChild('grid') public grid: GridComponent;
  roles: IRole[];
  roleID: any;
  userCreate: IUserCreate;
  userUpdate: IUserUpdate;
  setFocus: any;
  locale = localStorage.getItem('lang');
  lines: IBuilding[];
  constructor(
    private accountService: AccountService,
    private roleService: RoleService,
    public modalService: NgbModal,
    private userService: UserService,
    private alertify: AlertifyService,
    private route: ActivatedRoute,
  ) { super(); }

  ngOnInit() {
    this.Permission(this.route);
    this.roleID = 0;
    this.buildingID = 0;
    this.getRoles();
    this.getBuildings();
    this.getAllUserInfo();
  }
  // life cycle ejs-grid
  createdUsers() {
  }
  onDoubleClick(args: any): void {
    this.setFocus = args.column; // Get the column from Double click event
  }
  actionBegin(args) {
    if (args.requestType === 'save' && args.action === 'add') {
      this.userCreate = {
        id: 0,
        username: args.data.username ,
        password: args.data.password,
        email: args.data.email,
        roleid: 2,
        employeeID: args.data.employeeID,
        isLeader: false,
        systemCode: environment.systemCode
      };
      if (args.data.employeeID === undefined) {
        this.alertify.error('Please key in a account! <br> Vui lòng nhập tài khoản đăng nhập!');
        args.cancel = true;
        return;
      }
      if (args.data.password === undefined) {
        this.alertify.error('Please key in a password! <br> Vui lòng nhập mật khẩu!');
        args.cancel = true;
        return;
      }
      if (this.roleID > 0) {
        this.create();
      } else {
        args.cancel = true;
        this.alertify.error('Please select a role! <br> Vui lòng chọn 1 quyền!');
        return;
      }
    }
    if (args.requestType === 'save' && args.action === 'edit') {
      this.userUpdate = {
        id: args.data.id,
        username: args.data.username,
        password: args.data.password || '',
        email: args.data.email || '',
        roleid: 2,
        employeeID: args.data.employeeID,
        isLeader: false
      };
      this.update();
    }
    if (args.requestType === 'delete') {
      this.delete(args.data[0].id);
    }
  }
  tooltip(args: QueryCellInfoEventArgs) {
    if (args.column.field !== 'ID' && args.column.field !== 'password' && args.column.field !== 'option') {
      const tooltip: Tooltip = new Tooltip({
        content: args.data[args.column.field] + ''
      }, args.cell as HTMLTableCellElement);
    }
  }
  toolbarClick(args) {
    switch (args.item.id) {
      case 'grid_excelexport':
        this.grid.excelExport({ hierarchyExportMode: 'All' });
        break;
      default:
        break;
    }
  }
  actionComplete(args) {
    if (args.requestType === 'beginEdit' ) {
      if (this.setFocus.field !== 'role' && this.setFocus.field !== 'building') {
        args.form.elements.namedItem(this.setFocus.field).focus(); // Set focus to the Target element
      }
    }
    if (args.requestType === 'add') {
      args.form.elements.namedItem('employeeID').focus(); // Set focus to the Target element
    }
  }
  dataBound() {
    // document.querySelectorAll('button[aria-label=Update] > span.e-tbar-btn-text')[0].innerHTML = 'Save';
  }
  // end life cycle ejs-grid

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
  getRoles() {
    this.roleService.getAll().subscribe(result => {
      this.roles = result;
    }, error => {
      this.alertify.error(error);
    });
  }
  mappingUserRole(userRole: IUserRole) {
    this.roleService.mappingUserRole(userRole).subscribe(result => {
      this.alertify.success('Successfully!');
      this.roleID = 0;
    }, error => {
      this.alertify.error(error);
    });
  }
  lock(data) {
    const obj: IUserRole = {
      userID: data.id,
      roleID: data.userRoleID,
      isLock: !data.isLock
    };
    this.lockAPI(obj);
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

  mapUserRole(userID: number, roleID: number) {
    this.roleService.mapUserRole(userID, roleID).subscribe((res: any) => {
      if (res.status) {
        this.alertify.success(res.message);
        this.getAllUserInfo();
        this.roleID = 0;
      } else {
        this.alertify.success(res.message);
      }
    });
  }
  lockAPI(userRole: IUserRole) {
    this.roleService.lock(userRole).subscribe((res: any) => {
      if (res) {
        this.alertify.success('Success!');
        this.getAllUserInfo();
      } else {
        this.alertify.error('Failed!');
      }
    });
  }
  getAllUserInfo() {
    this.userService.getAllUserInfo().subscribe((users: any) => {
      this.userData = users;
    });
  }
  delete(id) {
    this.accountService.deleteUser(id).subscribe(res => {
      this.alertify.success('The user has been deleted! <br> Xóa thành công!');
      this.getAllUserInfo();
    });
  }
  create() {
    this.accountService.createUser(this.userCreate).subscribe((res: number) => {
      this.alertify.success('The user has been created! <br> Tạo thành công!');
      if (res > 0) {
        // if (res > 0) {
        //   this.mapBuildingUser(res, this.buildingID);
        // }
        if (res > 0) {
          this.mapUserRole(res, this.roleID);
        }
        this.getAllUserInfo();
        this.password = '';
      }
    });
  }
  update() {
    this.accountService.updateUser(this.userUpdate).subscribe(res => {
      this.alertify.success('The user has been updated! <br> Chỉnh sửa thành công!');
      if (this.userID && this.buildingID) {
        this.mapBuildingUser(this.userID, this.buildingID);
      }
      if (this.userID > 0 && this.roleID > 0) {
        this.mapUserRole(this.userID, this.roleID);
      }
      this.getAllUserInfo();
      this.password = '';
    });
  }
  // end api
  NO(index) {
    return (this.grid.pageSettings.currentPage - 1) * this.pageSettings.pageSize + Number(index) + 1;
  }
  checkboxChange(args, data) {
    // console.log('checkboxChange', args);
    // const checked = args.checked;
    // if (checked === true) {
    //   this.accountService.mapLineUser(this.userID, data.id).subscribe(res => {
    //     if (res.status) {
    //       this.alertify.success(res.message);
    //       this.getLineByUserID();
    //     } else {
    //       this.alertify.warning(res.message);
    //     }
    //   }, err => this.alertify.error(err));
    // } else {
    //   const obj = {
    //     userID: this.userID, buildingID: data.id
    //   };
    //   this.accountService.removeLineUser(obj).subscribe(res => {
    //       this.alertify.success('Thành công!');
    //       this.getLineByUserID();
    //   }, err => this.alertify.error(err));
    // }
  }
  checkboxChangeBuilding(args, data) {
    // const checked = args.checked;
    // if (checked === true) {
    //   this.accountService.mapMultipleBuildingUser(this.userID, data.id).subscribe(res => {
    //     if (res.status) {
    //       this.alertify.success(res.message);
    //       this.getBuildingByUserID();
    //     } else {
    //       this.alertify.warning(res.message);
    //     }
    //   }, err => this.alertify.error(err));
    // } else {
    //   const obj = {
    //     userID: this.userID, buildingID: data.id
    //   };
    //   this.accountService.removeMultipleBuildingUser(obj).subscribe(res => {
    //     this.alertify.success('Thành công!');
    //     this.getLineByUserID();
    //   }, err => this.alertify.error(err));
    // }
  }
  showLineModal(name, value) {
    this.userID = value.id;
    this.buildingID = this.buildingDatas.filter(item => item.name  === value.building)[0].id;
    this.getLineByUserID();
    this.modalReference = this.modalService.open(name, { size: 'md' });
  }
  showBuildingModal(name, value) {
    this.userID = value.id;
    this.getBuildingByUserID();
    this.modalReference = this.modalService.open(name, { size: 'md' });
  }
}
