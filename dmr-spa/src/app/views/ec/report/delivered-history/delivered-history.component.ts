import { BaseComponent } from 'src/app/_core/_component/base.component';
import { Component, OnInit, ViewChild } from '@angular/core';
import { MakeGlueService } from 'src/app/_core/_service/make-glue.service';
import { BuildingUserService } from 'src/app/_core/_service/building.user.service';
import { GridComponent, FilterService, FilterType } from '@syncfusion/ej2-angular-grids';
import { rejects } from 'assert';
import { ActivatedRoute } from '@angular/router';
import { ActionConstant } from 'src/app/_core/_constants';
@Component({
  selector: 'app-delivered-history',
  templateUrl: './delivered-history.component.html',
  styleUrls: ['./delivered-history.component.css'],
  providers: [FilterService]
})
export class DeliveredHistoryComponent extends BaseComponent implements OnInit {
  data: any;
  users: { ID: any; EmployeeID: any; Email: any; }[];
  public filterSettings: object;
  pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10};
  @ViewChild('grid') public grid: GridComponent;
  toolbarOptions: string[];
  constructor(
    private makeGlueService: MakeGlueService,
    private buildingUserService: BuildingUserService,
    private route: ActivatedRoute,
  ) {
    super();
  }

  ngOnInit() {
    this.Permission(this.route);
    this.filterSettings = { type: 'Excel' };
    this.loadData();
  }
  Permission(route: ActivatedRoute) {
    const functionCode = route.snapshot.data.functionCode;
    this.functions = JSON.parse(localStorage.getItem('functions')).filter(x => x.functionCode === functionCode) || [];
    for (const item of this.functions) {
      const toolbarOptions = [];
      for (const action of item.childrens) {
        const optionItem = this.makeAction(action.code);
        toolbarOptions.push(...optionItem.filter(Boolean));
      }
      toolbarOptions.push(...['Search']);
      const uniqueOptionItem = toolbarOptions.filter((elem, index, self) => {
        return index === self.indexOf(elem);
      });
      this.toolbarOptions = uniqueOptionItem;
    }
  }
  makeAction(input: string): any[] {
    switch (input) {
      case ActionConstant.EXCEL_EXPORT:
        return ['ExcelExport'];
      default:
        return [undefined];
    }
  }
  toolbarClick(args): void {
    switch (args.item.id) {
      /* tslint:disable */
      case 'grid_excelexport':
        this.grid.excelExport();
        break;
    }
  }
  getUsers() {
    return new Promise((resolve, reject) => {
      this.buildingUserService.getAllUsers(1, 1000).subscribe(res => {
        const data = res.result.map((i: any) => {
          return {
            ID: i.ID,
            EmployeeID: i.EmployeeID,
            Email: i.Email
          };
        });
        this.users = data;
        resolve(true);
      }, err => {
        reject(false);
      });
    });
  }
  username(id) {
    return (this.users.find(item => item.ID === id) as any)?.EmployeeID || '#N/A';
  }
  async loadData() {
    try {
      const result = await this.getUsers();
      if (result) {
        this.deliveredHistory();
      }
    } catch (error) {
    }
  }
  deliveredHistory() {
    this.makeGlueService.deliveredHistory()
      .subscribe((res: any) => {
       this.data = res.map( (item: any) => {
         return {
           glueName: item.glueName,
           buildingName: item.buildingName,
           qty: item.qty,
           deliveredBy: this.username(item.createdBy),
           createdDate: new Date(item.createdDate)
         };
       });
      }, () => {
        console.log('500 (Internal Server Error)')
      });
  }
  NO(index) {
    return (this.grid.pageSettings.currentPage - 1) * this.grid.pageSettings.pageSize + Number(index) + 1;
  }
}
