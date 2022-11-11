import { BaseComponent } from 'src/app/_core/_component/base.component';
import { PlanService } from '../../../../_core/_service/plan.service';
import { Consumtion } from '../../../../_core/_model/plan';
import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { PageSettingsModel, GridComponent } from '@syncfusion/ej2-angular-grids';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { ActivatedRoute } from '@angular/router';
import { DatePipe } from '@angular/common';
import { FormGroup } from '@angular/forms';
import { NgxSpinnerService } from 'ngx-spinner';
import { QueryCellInfoEventArgs } from '@syncfusion/ej2-angular-grids';
import { EmitType } from '@syncfusion/ej2-base';

import { IBuilding } from 'src/app/_core/_model/building';
import { BuildingService } from 'src/app/_core/_service/building.service';
import { FilteringEventArgs } from '@syncfusion/ej2-angular-dropdowns';
import { Query } from '@syncfusion/ej2-data/';
import { Tooltip } from '@syncfusion/ej2-angular-popups';
import { IRole } from 'src/app/_core/_model/role';
import { ActionConstant } from 'src/app/_core/_constants';
const BUILDING_LEVEL = 2;
const WORKER = 4;
const ROLE: IRole = JSON.parse(localStorage.getItem('level'));
@Component({
  selector: 'app-consumption-1',
  templateUrl: './consumption-1.component.html',
  styleUrls: ['./consumption-1.component.scss']
})
export class Consumption1Component extends BaseComponent implements OnInit {
  @ViewChild('cloneModal') public cloneModal: TemplateRef<any>;
  @ViewChild('planForm')
  public orderForm: FormGroup;
  public pageSettings: PageSettingsModel;
  level: number;
  public role: IRole;
  public building: IBuilding[];
  @ViewChild('grid')
  public grid: GridComponent;
  modalReference: NgbModalRef;
  public data: Consumtion[];
  searchSettings: any = { hierarchyMode: 'Parent' };
  startDate: Date;
  endDate: Date;
  buildingID: number;
  buildings: IBuilding[];
  public fieldsBuildings: object = { text: 'name', value: 'id' };
  sortSettings = { columns: [{ field: 'percentage', direction: 'Descending' }] };
  public dataBound(): void {
    this.grid.autoFitColumns();
  }
  public queryCellInfoEvent: EmitType<QueryCellInfoEventArgs> = (args: QueryCellInfoEventArgs) => {
    const data = args.data as Consumtion;
    const fields = ['modelName', 'modelNo', 'articleNo', 'process', 'glue', 'std'];
    // if (fields.includes(args.column.field)) {
    //   args.rowSpan = this.data.filter(
    //     item => item.id === data.id &&
    //       item.modelName === data.modelName &&
    //       item.modelNo === data.modelNo &&
    //       item.articleNo === data.articleNo &&
    //       item.process === data.process &&
    //       item.glue === data.glue &&
    //       item.dueDate === data.dueDate &&
    //       item.std === data.std
    //   ).length;
    // }
    if (data.percentage > 0) {
      (args.cell as any).style.backgroundColor = '#FFFF66';
    }
    // if (!fields.includes(args.column.field)) {
    //   if (data.percentage > 0) {
    //     (args.cell as any).style.backgroundColor = '#FFFF66';
    //   }
    // }
  }
  constructor(
    private alertify: AlertifyService,
    public modalService: NgbModal,
    private planService: PlanService,
    public datePipe: DatePipe,
    private spinner: NgxSpinnerService,
    private buildingService: BuildingService,
    private route: ActivatedRoute,
  ) {
    super();
    this.buildingID = +localStorage.getItem('buildingID');
  }

  ngOnInit() {
    this.Permission(this.route);
    this.startDate = new Date();
    this.endDate = new Date();
    this.role = ROLE;
    this.level = JSON.parse(localStorage.getItem('level')).level;
    this.pageSettings = { pageCount: 20, pageSizes: ['All', 100], pageSize: 100 };
    if (this.buildingID === 0) {
      this.getBuilding(() => {
        this.alertify.message('Please select a building!', true);
      });
    } else {
      this.getBuilding(() => {
        this.consumptionByLineCase1();
      });
    }
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
  headerCellInfo(args) {
    if (args.cell.column.field === 'totalConsumption') {
      const toolcontent = 'Total Consumption (kg)';
      const tooltip: Tooltip = new Tooltip({
        content: toolcontent
      });
      tooltip.appendTo(args.node);
    }
    if (args.cell.column.field === 'realConsumption') {
      const toolcontent = 'Real Consumption (g) pr.';
      const tooltip: Tooltip = new Tooltip({
        content: toolcontent
      });
      tooltip.appendTo(args.node);
    }
    if (args.cell.column.field === 'mixingDate') {
      const toolcontent = 'Mixing Date Of Glue';
      const tooltip: Tooltip = new Tooltip({
        content: toolcontent
      });
      tooltip.appendTo(args.node);
    }
  }
  public onFilteringBuilding: EmitType<FilteringEventArgs> = (
    e: FilteringEventArgs
  ) => {
    let query: Query = new Query();
    // frame the query based on search string with filter type.
    query =
      e.text !== '' ? query.where('name', 'contains', e.text, true) : query;
    // pass the filter data source, filter query to updateData method.
    e.updateData(this.buildings as any, query);
  }
  onChangeBuilding(args) {
    this.buildingID = args.itemData.id;
    localStorage.setItem('buildingID', args.itemData.id);
    this.consumptionByLineCase1();
  }
  onSelectBuilding(args) {
    this.buildingID = args.itemData.id;
    localStorage.setItem('buildingID', args.itemData.id);
    this.consumptionByLineCase1();
  }

  private getBuilding(callback): void {
    this.buildingService.getBuildings().subscribe(async (buildingData) => {
      this.buildings = buildingData.filter(item => item.level === BUILDING_LEVEL);
      callback();
    });
  }
  consumptionByLineCase1() {
    this.planService.consumptionByLineCase1(this.buildingID, this.startDate, this.endDate).subscribe((res: Consumtion[]) => {
      this.data = res;
    });
  }
  startDateOnchange(args) {
    if (args.isInteracted) {
      this.startDate = (args.value as Date);
      this.consumptionByLineCase1();
    }
  }
  endDateOnchange(args) {
    if (args.isInteracted) {
      this.endDate = (args.value as Date);
      this.consumptionByLineCase1();
    }
  }
  onClickDefault() {
    this.startDate = new Date();
    this.endDate = new Date();
    this.consumptionByLineCase1();
  }
  toolbarClick(args: any): void {
    switch (args.item.id) {
      /* tslint:disable */
      case 'grid_excelexport':
        this.reportConsumptionCase1();
        break;
    }
  }
  tooltip(data) {
    if (data) {
      return data.join('<br>');
    } else {
      return '';
    }
  }

  reportConsumptionCase1() {
    const days = Math.floor((this.endDate.getTime() - this.startDate.getTime()) / (1000 * 60 * 60 * 24));
    if (days > 31) {
      const error = 'Chỉ được xuất dữ liệu báo cáo trong 30 ngày!!!<br>The report data can only be exported for 30 days!!!';
      this.alertify.error(error, true);
      return;
    }
    this.spinner.show();
    this.planService.reportConsumptionCase1(this.buildingID, this.startDate, this.endDate).subscribe((data: any) => {
      const blob = new Blob([data],
        { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });

      const downloadURL = window.URL.createObjectURL(data);
      const link = document.createElement('a');
      link.href = downloadURL;
      const startDateFormat = this.datePipe.transform(this.startDate, "YYYYMMdd");
      const endDateFormat = this.datePipe.transform(this.endDate, "YYYYMMdd");
      link.download = `${startDateFormat}-${endDateFormat}_Consumption1.xlsx`;
      link.click();
      this.spinner.hide();
    });
  }
  // End API
}
