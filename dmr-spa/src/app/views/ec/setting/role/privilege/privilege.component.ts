import { PermissionService } from 'src/app/_core/_service/permission.service';
import { Component, OnInit, QueryList, ViewChildren } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { RoleService } from 'src/app/_core/_service/role.service';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { TreeViewComponent } from '@syncfusion/ej2-angular-navigations';

@Component({
  selector: 'app-privilege',
  templateUrl: './privilege.component.html',
  styleUrls: ['./privilege.component.scss']
})
export class PrivilegeComponent implements OnInit {
  id: number;
  modules: any;
  roleIDs: any[];
  checkedData: any[] = [];
  @ViewChildren('treeview')
  public tree: QueryList<TreeViewComponent>;
  checkedList: any[];
  data: any[];

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private roleService: RoleService,
    private alertify: AlertifyService,
    private permissionService: PermissionService

  ) { }
  ngOnInit() {
    this.id = +this.route.snapshot.params.id || 0;
    this.roleIDs = [this.id];
    this.getAllRole();
    this.getScreenFunctionAndAction();
  }
  getAllRole() {
    this.roleService.getAll().subscribe(res => {
      this.data = res;
    });
  }
  changeRole(args, item) {
    if (args.checked === true) {
      this.roleIDs.push(item.id);
    } else {
      const filterd = this.roleIDs.filter((el) => {
        return el !== item.id;
      });
      this.roleIDs = filterd;
      if (this.roleIDs.length === 0) {
        this.roleIDs.push(this.id);
      }
    }
    this.getScreenFunctionAndAction();
  }

  nodeCheck(args: any): void {
    const checkedNode: any = [args.node];
    for (const tree of this.tree) {
      if (args.event.target.classList.contains('e-fullrow') || args.event.key === "Enter") {
        const getNodeDetails: any = tree.getNode(args.node);
        if (getNodeDetails.isChecked === 'true') {
          tree.uncheckAll(checkedNode);
        } else {
          tree.checkAll(checkedNode);
        }
      }
    }
  }
  nodeChecked(args): void {
    console.log(args);
  }
  back() {
    this.router.navigate(['/ec/setting/role']);
  }
  created(args) {
  }

  getAllChecked() {
    const checkedData = [];
    const permissions = [];
    const treeData = [];

    for (const tree of this.tree) {
      const result = [...tree.getAllCheckedNodes()];
      checkedData.push(...result);
      const result2 = [...tree.getTreeData()];
      treeData.push(...result2);
    }

    const childrenTemp = treeData.map(el => {
      return el.childrens;
    });

    const childs = [].concat.apply([], childrenTemp);

    const distinctCheckedData = checkedData.filter((value, index, self) => {
      return self.indexOf(value) === index;
    });
    for (const item of distinctCheckedData) {
      const childData = childs.filter((el: any) => {
        return el.id === item;
      });

      const parentData = treeData.filter((el: any) => {
        return el.id === item;
      });
      permissions.push(...childData);
      permissions.push(...parentData);
    }
    this.checkedList = permissions;

  }

  getScreenFunctionAndAction() {
    this.roleService.getScreenFunctionAndAction(this.roleIDs === [] ? [0] : this.roleIDs).subscribe((data: any) => {
      this.modules = data;
    });
  }
  checkedAll() {
    for (const tree of this.tree) {
      tree.checkAll();
    }
  }
  uncheckedAll() {
    for (const tree of this.tree) {
      tree.uncheckAll();
    }
  }
  collapseAll() {
    for (const tree of this.tree) {
      tree.collapseAll();
    }
  }
  expandAll() {
    for (const tree of this.tree) {
      tree.expandAll();
    }
  }
  save() {
    this.getAllChecked();
    this.putPermissionByRoleId();
  }
  putPermissionByRoleId() {
    const request = {
      permissions: this.checkedList
    };
    this.permissionService.putPermissionByRoleId(this.id, request).subscribe(() => {
      this.alertify.success('Successfully');
      this.roleIDs = [this.id];
      this.getScreenFunctionAndAction();
    });
  }
}
