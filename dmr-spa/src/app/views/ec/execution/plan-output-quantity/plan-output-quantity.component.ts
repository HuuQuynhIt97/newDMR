import { PlanService } from '../../../../_core/_service/plan.service';
import { Plan } from '../../../../_core/_model/plan';
import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { PageSettingsModel, GridComponent } from '@syncfusion/ej2-angular-grids';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { DatePipe } from '@angular/common';
import { FormGroup } from '@angular/forms';
import { BPFCEstablishService } from 'src/app/_core/_service/bpfc-establish.service';
import { NgxSpinnerService } from 'ngx-spinner';
import { IBuilding } from 'src/app/_core/_model/building';
import { IRole } from 'src/app/_core/_model/role';
import { AuthService } from 'src/app/_core/_service/auth.service';
import { DataService } from 'src/app/_core/_service/data.service';
import { Subscription } from 'rxjs';
import { FilteringEventArgs } from '@syncfusion/ej2-angular-dropdowns';
import { Query } from '@syncfusion/ej2-data/';
import { EmitType } from '@syncfusion/ej2-base';
import { BuildingService } from 'src/app/_core/_service/building.service';
const WORKER = 4;
declare var $;
const ADMIN = 1;
const BUILDING_LEVEL = 2;
const SUPERVISOR = 2;
const STAFF = 3;
const DISPATCHER = 6;
@Component({
  selector: 'app-plan-output-quantity',
  templateUrl: './plan-output-quantity.component.html',
  styleUrls: ['./plan-output-quantity.component.scss']
})
export class PlanOutputQuantityComponent implements OnInit {
  role: IRole;
  building: IBuilding[];
  @ViewChild('cloneModal') public cloneModal: TemplateRef<any>;
  @ViewChild('planForm')
  public orderForm: FormGroup;
  public pageSettings: PageSettingsModel;
  public toolbarOptions: object;
  public editSettings: object;
  sortSettings: object;
  startDate = new Date();
  endDate = new Date();
  bpfcID: number;
  level: number;
  hasWorker: boolean;
  public bpfcData: object;
  public plansSelected: any;
  public date = new Date();
  public editparams: object;
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
  fieldsBuildings: object = { text: 'name', value: 'id' };

  public buildingName: object[];
  public modelName: object[];
  buildingNameEdit: any;
  workHour: number;
  hourlyOutput: number;
  BPFCs: any;
  bpfcEdit: number;
  glueDetails: any;
  setFocus: any;
  locale: string;
  subscription: Subscription[] = [];
  buildingID: number;
  buildings: IBuilding[];
  IsAdmin: boolean;
  constructor(
    private alertify: AlertifyService,
    public modalService: NgbModal,
    private planService: PlanService,
    private bPFCEstablishService: BPFCEstablishService,
    public authService: AuthService,
    public datePipe: DatePipe,
    private buildingService: BuildingService,
    private dataService: DataService,
    private spinner: NgxSpinnerService
  ) { }

  ngOnInit(): void {
    this.date = new Date();
    this.endDate = new Date();
    this.endDate = new Date();
    const BUIDLING: IBuilding[] = JSON.parse(localStorage.getItem('building'));
    const ROLE: IRole = JSON.parse(localStorage.getItem('level'));
    this.role = ROLE;
    this.building = BUIDLING;
    this.gridConfig();
    this.checkRole();
    this.getAllBPFC();
    this.ClearForm();
  }

  gridConfig(): void {
    this.pageSettings = { pageCount: 20, pageSizes: true, pageSize: 12 };
    this.sortSettings = { columns: [{ field: 'dueDate', direction: 'Ascending' }] };
    this.editparams = { params: { popupHeight: '300px' } };
    this.hasWorker = true;
    this.editSettings = { showDeleteConfirmDialog: false, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Normal' };
    this.toolbarOptions = ['Cancel', 'Search'];
  }
  count(index) {
    return Number(index) + 1;
  }
  getAllLine(buildingID) {
    this.planService.getLines(buildingID).subscribe((res: any) => {
      this.buildingName = res;
    });
  }
  created() { }
  onChangeBuilding(args) {
    this.buildingID = args.itemData.id;
    this.buildingName = args.itemData.name;
    localStorage.setItem('buildingID', args.itemData.id);
    this.getAll();
  }
  onFilteringBuilding: EmitType<FilteringEventArgs> = (
    e: FilteringEventArgs
  ) => {
    let query: Query = new Query();
    // frame the query based on search string with filter type.
    query =
      e.text !== '' ? query.where('name', 'contains', e.text, true) : query;
    // pass the filter data source, filter query to updateData method.
    e.updateData(this.buildings as any, query);
  }
  getBuilding(callback): void {
    this.buildingService.getBuildings().subscribe( buildingData => {
      this.buildings = buildingData.filter(item => item.level === BUILDING_LEVEL);
      callback();
    });
  }
  onSelectBuilding(args: any): void {
    this.buildingID = args.itemData.id;
    this.buildingName = args.itemData.name;
    localStorage.setItem('buildingID', args.itemData.id);
    this.getAll();
  }
  checkRole(): void {
    const buildingId = +localStorage.getItem('buildingID');
    if (buildingId === 0) {
      this.getBuilding(() => {
        this.alertify.message('Please select a building! <br> Vui lòng chọn 1 tòa nhà!', true);
      });
    } else {
      this.getBuilding(() => {
        this.buildingID = buildingId;
        // this.getAll();
        // this.getStartTimeFromPeriod();
      });
    }
    // // Nếu là admin, suppervisor, staff thì hiện cả todo va dispatch
    // switch (this.role.id) {
    //   case ADMIN:
    //   case SUPERVISOR:
    //   case STAFF:
    //   case WORKER: // Chỉ hiện todolist
    //     this.IsAdmin = true;
    //     const buildingId = +localStorage.getItem('buildingID');
    //     if (buildingId === 0) {
    //       this.getBuilding(() => {
    //         this.alertify.message('Please select a building!', true);
    //       });
    //     } else {
    //       this.getBuilding(() => {
    //         this.buildingID = buildingId;
    //         this.getAll();
    //         this.getAllLine(this.buildingID);
    //       });
    //     }
    //     break;
    //   case DISPATCHER: // Chỉ hiện dispatchlist
    //     this.building = JSON.parse(localStorage.getItem('building'));
    //     this.getBuilding(() => {
    //       this.buildingID = this.building[0].id;
    //       this.getAll();
    //       this.getAllLine(this.buildingID);
    //     });
    //     break;
    // }
  }
  getReport(obj: { startDate: Date, endDate: Date }) {
    this.spinner.show();
    this.planService.getReport(obj).subscribe((data: any) => {
      const blob = new Blob([data],
        { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });

      const downloadURL = window.URL.createObjectURL(data);
      const link = document.createElement('a');
      link.href = downloadURL;
      link.download = 'report.xlsx';
      link.click();
      this.spinner.hide();
    }, err => {
      this.alertify.error(`Chỉ được xuất dữ liệu báo cáo trong 30 ngày!!!<br>
        The report data can only be exported for 30 days !!!`, true);
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
      if (this.setFocus?.field) {
        e.form.elements.namedItem('quantity').focus(); // Set focus to the Target element
      }
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
        const previousData = args.previousData;
        const data = args.data;
        if (data.quantity !== previousData.quantity) {
          const planId = args.data.id || 0;
          const quantity = args.data.quantity;
          this.planService.editQuantity(planId, quantity).subscribe(res => {
            this.alertify.success('Updated succeeded!');
            this.ClearForm();
            this.getAll();
          });
        } else { args.cancel = true; }
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

  getAll() {
    this.planService.search(this.buildingID, this.startDate.toDateString(), this.endDate.toDateString()).subscribe((res: any) => {
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
        this.getAll();
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
    switch (args.item.text) {
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
          console.log('Delete Range', selectedRecords);
          this.deleteRange(selectedRecords);
        }
        break;
      case 'Excel Export':
        this.getReport({ startDate: this.startDate, endDate: this.endDate });
        break;
      default:
        break;
    }
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
        this.getAll();
        this.modalService.dismissAll();
      } else {
        this.alertify.warning('the plans have already existed!');
        this.modalService.dismissAll();
      }
    });
  }

  search() {
    this.planService.search(this.buildingID, this.startDate.toDateString(), this.endDate.toDateString()).subscribe((res: any) => {
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
    this.getAll();
  }
  startDateOnchange(args) {
    this.startDate = (args.value as Date);
    this.search();
  }
  endDateOnchange(args) {
    this.endDate = (args.value as Date);
    this.search();
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
    this.search();
  }

  // End API
}
