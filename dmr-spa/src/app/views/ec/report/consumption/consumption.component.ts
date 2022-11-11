import { BaseComponent } from 'src/app/_core/_component/base.component';
import { PlanService } from '../../../../_core/_service/plan.service';
import { Plan } from '../../../../_core/_model/plan';
import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { PageSettingsModel, GridComponent } from '@syncfusion/ej2-angular-grids';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { ActivatedRoute } from '@angular/router';
import { DatePipe } from '@angular/common';
import { FormGroup } from '@angular/forms';
import { BPFCEstablishService } from 'src/app/_core/_service/bpfc-establish.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { IBuilding } from 'src/app/_core/_model/building';
import { BuildingService } from 'src/app/_core/_service/building.service';
import { FilteringEventArgs } from '@syncfusion/ej2-angular-dropdowns';
import { Query } from '@syncfusion/ej2-data/';
import { EmitType } from '@syncfusion/ej2-base';
import { ActionConstant } from 'src/app/_core/_constants';

const WORKER = 4;
const BUILDING_LEVEL = 2;

@Component({
  selector: 'app-consumption',
  templateUrl: './consumption.component.html',
  styleUrls: ['./consumption.component.css']
})
export class ConsumptionComponent extends BaseComponent implements OnInit {
  @ViewChild('cloneModal') public cloneModal: TemplateRef<any>;
  @ViewChild('planForm')
  public orderForm: FormGroup;
  public pageSettings: PageSettingsModel;
  startDate = new Date();
  endDate = new Date();
  bpfcID: number;
  level: number;
  hasWorker: boolean;
  sortSettings = { columns: [{ field: 'dueDate', direction: 'Ascending' }] };
  public bpfcData: object;
  public plansSelected: any;
  public date = new Date();
  public editparams: object;
  public role = JSON.parse(localStorage.getItem('level'));
  public building = JSON.parse(localStorage.getItem('building'));
  @ViewChild('grid')
  public grid: GridComponent;
  dueDate: any;
  modalReference: NgbModalRef;
  public data: object[];
  searchSettings: any = { hierarchyMode: 'Parent' };
  modalPlan: Plan = {
    id: 0,
    buildingID: 0,
    BPFCEstablishID: 0,
    BPFCName: '',
    hourlyOutput: 0,
    workingHour: 0,
    dueDate: new Date(),
    startWorkingTime: new Date(),
    finishWorkingTime: new Date(),
    startTime: {
      hour: 7,
      minute: 0
    },
    endTime: {
      hour: 16,
      minute: 30
    }

  };
  public textLine = 'Select a line name';
  public fieldsGlue: object = { text: 'name', value: 'name' };
  public fieldsLine: object = { text: 'name', value: 'name' };
  public fieldsBPFC: object = { text: 'name', value: 'name' };
  public fieldsBuildings: object = { text: 'name', value: 'id' };

  public buildingName: object[];
  public modelName: object[];
  buildingNameEdit: any;
  workHour: number;
  hourlyOutput: number;
  BPFCs: any;
  bpfcEdit: number;
  glueDetails: any;
  setFocus: any;
  buildings: IBuilding[];
  buildingID = 0;
  constructor(
    private alertify: AlertifyService,
    public modalService: NgbModal,
    private planService: PlanService,
    private bPFCEstablishService: BPFCEstablishService,
    public datePipe: DatePipe,
    private buildingService: BuildingService,
    private spinner: NgxSpinnerService,
    private route: ActivatedRoute,
  ) {
    super();
    this.buildingID = +localStorage.getItem('buildingID');
  }

  ngOnInit() {
    this.Permission(this.route);
    const now = new Date();
    this.endDate = new Date();
    this.level = JSON.parse(localStorage.getItem('level')).level;
    this.pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
    this.editparams = { params: { popupHeight: '300px' } };

    this.editSettings = { showDeleteConfirmDialog: false, allowEditing: false, allowAdding: false, allowDeleting: false, mode: 'Normal' };

    this.getAll(this.startDate, this.endDate);
    this.getAllBPFC();
    const buildingID = JSON.parse(localStorage.getItem('level')).id;
    this.getAllLine(buildingID);
    this.getBuilding();
    this.ClearForm();
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
        return [
                { text: 'Report', tooltipText: 'Report', prefixIcon: 'e-excelexport', id: 'report' },
                { text: 'Report(new)', tooltipText: 'Report(new)', prefixIcon: 'e-excelexport', id: 'newReport' },
              ];
      default:
        return [undefined];
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
    this.getAll(this.startDate, this.endDate);
  }

  private getBuilding(): void {
    this.buildingService.getBuildings().subscribe(async (buildingData) => {
      this.buildings = buildingData.filter(item => item.level === BUILDING_LEVEL);
    });
  }
  count(index) {
    return Number(index) + 1;
  }

  getAllLine(buildingID) {
    this.planService.getLines(buildingID).subscribe((res: any) => {
      this.buildingName = res;
    });
  }
  getReport(obj: { startDate: Date, endDate: Date }) {
    const days = Math.floor((obj.endDate.getTime() - obj.startDate.getTime()) / (1000 * 60 * 60 * 24));
    if (days > 31) {
      const error = 'Chỉ được xuất dữ liệu báo cáo trong 30 ngày!!!<br>The report data can only be exported for 30 days!!!';
      this.alertify.error(error, true);
      return;
    }
    this.spinner.show();
    this.planService.getReport(obj).subscribe((data: any) => {
      const blob = new Blob([data],
        { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });

      const downloadURL = window.URL.createObjectURL(data);
      const link = document.createElement('a');
      link.href = downloadURL;
      const ct = new Date();
      const startDateFormat = this.datePipe.transform(this.startDate, "YYYYMMdd");
      const endDateFormat = this.datePipe.transform(this.endDate, "YYYYMMdd");
      link.download = `${startDateFormat}-${endDateFormat}_Cost.xlsx`;
      link.click();
      this.spinner.hide();
    });
  }
  getReportByBuilding() {
    const obj = { startDate: this.startDate, endDate: this.endDate, buildingID: this.buildingID };
    const days = Math.floor((obj.endDate.getTime() - obj.startDate.getTime()) / (1000 * 60 * 60 * 24));
    if (days > 31) {
      const error = 'Chỉ được xuất dữ liệu báo cáo trong 30 ngày!!!<br>The report data can only be exported for 30 days!!!';
      this.alertify.error(error, true);
      return;
    }
    this.spinner.show();
    this.planService.getReportByBuilding(obj).subscribe((data: any) => {
      const blob = new Blob([data],
        { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });

      const downloadURL = window.URL.createObjectURL(data);
      const link = document.createElement('a');
      link.href = downloadURL;
      const ct = new Date();
      const startDateFormat = this.datePipe.transform(this.startDate, "YYYYMMdd");
      const endDateFormat = this.datePipe.transform(this.endDate, "YYYYMMdd");
      link.download = `${startDateFormat}-${endDateFormat}_Cost.xlsx`;
      link.click();
      this.spinner.hide();
    });
  }
  // Method is use to download file.
  // param data - Array Buffer data
  // param type - type of the document.
  downLoadFile(data: any, type: string) {
    const blob = new Blob([data], { type });
    const url = window.URL.createObjectURL(blob);
    const pwa = window.open(url);
    if (!pwa || pwa.closed || typeof pwa.closed === 'undefined') {
      this.alertify.error('Please disable your Pop-up blocker and try again.');
    }
    this.spinner.hide();
  }
  onChangeBuildingNameEdit(args) {
    this.buildingNameEdit = args.itemData.id;
  }
  onChangeDueDateEdit(args) {
    this.dueDate = (args.value as Date).toDateString();
  }

  onChangeDueDateClone(args) {
    this.date = (args.value as Date);
  }

  onChangeBPFCEdit(args) {
    this.bpfcEdit = args.itemData.id;
  }

  actionComplete(e) {
    if (e.requestType === 'beginEdit') {
      e.form.elements.namedItem(this.setFocus.field).focus(); // Set focus to the Target element
    }
  }
  onDoubleClick(args: any): void {
    this.setFocus = args.column; // Get the column from Double click event
  }
  actionBegin(args) {
    if (args.requestType === 'cancel') {
      this.ClearForm();
    }

    if (args.requestType === 'save') {
      if (args.action === 'edit') {
        const planId = args.data.id || 0;
        const quantity = args.data.quantity;
        this.planService.editQuantity(planId, quantity).subscribe(res => {
          this.alertify.success('Updated succeeded!');
          this.ClearForm();
          this.getAll(this.startDate, this.endDate);
        });
      }
    }
  }

  private ClearForm() {
    this.bpfcEdit = 0;
    this.hourlyOutput = 0;
    this.workHour = 0;
    this.dueDate = new Date();
  }

  private validForm(): boolean {
    const array = [this.bpfcEdit];
    return array.every(item => item > 0);
  }

  onChangeWorkingHour(args) {
    this.workHour = args;
  }

  onChangeHourlyOutput(args) {

    this.hourlyOutput = args;
  }

  rowSelected(args) {
  }

  openaddModalPlan(addModalPlan) {
    this.modalReference = this.modalService.open(addModalPlan);
  }

  getAllBPFC() {
    this.bPFCEstablishService.filterByApprovedStatus().subscribe((res: any) => {
      this.BPFCs = res.map((item) => {
        return {
          id: item.id,
          name: `${item.modelName} - ${item.modelNo} - ${item.articleNo} - ${item.artProcess}`,
        };
      });
    });
  }

  getAll(startDate, endDate) {
    this.planService.search(this.buildingID, startDate.toDateString(), endDate.toDateString()).subscribe((res: any) => {
      this.data = res.map(item => {
        return {
          id: item.id,
          bpfcName: `${item.modelName} - ${item.modelNoName} - ${item.articleName} - ${item.processName}`,
          dueDate: item.dueDate,
          createdDate: item.createdDate,
          workingHour: item.workingHour,
          hourlyOutput: item.hourlyOutput,
          buildingName: item.buildingName,
          buildingID: item.buildingID,
          quantity: item.quantity,
          bpfcEstablishID: item.bpfcEstablishID,
          glues: item.glues || []
        };
      });
    });
  }
  deleteRange(plans) {
    this.alertify.confirm('Delete Plan', 'Are you sure you want to delete this Plans ?', () => {
      this.planService.deleteRange(plans).subscribe(() => {
        this.getAll(this.startDate, this.endDate);
        this.alertify.success('Plans has been deleted');
      }, error => {
        this.alertify.error('Failed to delete the Modal Name');
      });
    });
  }

  /// Begin API
  openModal(ref) {
    const selectedRecords = this.grid.getSelectedRecords();
    if (selectedRecords.length !== 0) {
      this.plansSelected = selectedRecords.map((item: any) => {
        return {
          id: 0,
          bpfcEstablishID: item.bpfcEstablishID,
          workingHour: item.workingHour,
          hourlyOutput: item.hourlyOutput,
          dueDate: item.dueDate,
          buildingID: item.buildingID
        };
      });
      this.modalReference = this.modalService.open(ref);
    } else {
      this.alertify.warning('Please select the plan');
    }
  }

  toolbarClick(args: any): void {
    switch (args.item.id) {
      case 'grid_excelexport':
        this.getReport({ startDate: this.startDate, endDate: this.endDate });
        break;
      case 'Clone':
        this.openModal(this.cloneModal);
        break;
      case 'Delete Range':
        if (this.grid.getSelectedRecords().length === 0) {
          this.alertify.warning('Please select the plans!!');
        } else {
          const selectedRecords = this.grid.getSelectedRecords().map((item: any) => {
            return item.id;
          });
          this.deleteRange(selectedRecords);
        }
        break;
      case 'report':
        this.getReportByBuilding();
        break;
      case 'newReport':
        this.getNewReportByBuilding();
        break;
      default:
        break;
    }
  }
  getNewReportByBuilding() {
    const obj = { startDate: this.startDate, endDate: this.endDate, buildingID: this.buildingID };
    const days = Math.floor((obj.endDate.getTime() - obj.startDate.getTime()) / (1000 * 60 * 60 * 24));
    if (days > 31) {
      const error = 'Chỉ được xuất dữ liệu báo cáo trong 30 ngày!!!<br>The report data can only be exported for 30 days!!!';
      this.alertify.error(error, true);
      return;
    }
    this.spinner.show();
    this.planService.getNewReportByBuilding(obj).subscribe((data: any) => {
      const blob = new Blob([data],
        { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });

      const downloadURL = window.URL.createObjectURL(data);
      const link = document.createElement('a');
      link.href = downloadURL;
      const startDateFormat = this.datePipe.transform(this.startDate, "YYYYMMdd");
      const endDateFormat = this.datePipe.transform(this.endDate, "YYYYMMdd");
      link.download = `${startDateFormat}-${endDateFormat}_Cost(new).xlsx`;
      link.click();
      this.spinner.hide();
    });
  }
  onClickClone() {
    this.plansSelected.map(item => {
      item.dueDate = this.date;
    });

    this.planService.clonePlan(this.plansSelected).subscribe((res: any) => {
      if (res) {
        this.alertify.success('Successfully!');
        this.startDate = this.date;
        this.endDate = this.date;
        this.getAll(this.date, this.date);
        this.modalService.dismissAll();
      } else {
        this.alertify.warning('the plans have already existed!');
        this.modalService.dismissAll();
      }
    });
  }

  search(startDate, endDate) {
    this.planService.search(this.building.id, startDate.toDateString(), endDate.toDateString()).subscribe((res: any) => {
      this.data = res.map(item => {
        return {
          id: item.id,
          bpfcName: `${item.modelName} - ${item.modelNoName} - ${item.articleName} - ${item.processName}`,
          dueDate: item.dueDate,
          createdDate: item.createdDate,
          workingHour: item.workingHour,
          hourlyOutput: item.hourlyOutput,
          buildingName: item.buildingName,
          buildingID: item.buildingID,
          bpfcEstablishID: item.bpfcEstablishID,
          quantity: item.quantity,
          glues: item.glues || [],
        };
      });
    });
  }


  onClickDefault() {
    this.startDate = new Date();
    this.endDate = new Date();
    this.getAll(this.startDate, this.endDate);
  }
  startDateOnchange(args) {
    this.startDate = (args.value as Date);
    this.getAll(this.startDate, this.endDate);
  }
  endDateOnchange(args) {
    this.endDate = (args.value as Date);
    this.getAll(this.startDate, this.endDate);
  }
  tooltip(data) {
    if (data) {
      return data.join('<br>');
    } else {
      return '';
    }
  }
  editQuantity(id: number, qty: number) {
    this.planService.editQuantity(id, qty);
  }
  onClickFilter() {
    this.getAll(this.startDate, this.endDate);
  }

  // End API
}
