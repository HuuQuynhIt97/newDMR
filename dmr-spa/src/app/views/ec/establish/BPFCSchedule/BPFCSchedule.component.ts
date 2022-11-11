import { BaseComponent } from 'src/app/_core/_component/base.component';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import {
  Component,
  OnInit,
  ViewChild,
  TemplateRef,
  ViewChildren,
  QueryList,
  OnDestroy,
  Input,
} from '@angular/core';
import { ModalNameService } from 'src/app/_core/_service/modal-name.service';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import {
  GridComponent,
  ExcelExportProperties,
  Column,
  SearchSettingsModel,
} from '@syncfusion/ej2-angular-grids';
import { BuildingUserService } from 'src/app/_core/_service/building.user.service';
import { environment } from '../../../../../environments/environment';
import { BPFCEstablishService } from 'src/app/_core/_service/bpfc-establish.service';
import { DatePipe } from '@angular/common';
import { UserService } from 'src/app/_core/_service/user.service';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxSpinnerService } from 'ngx-spinner';
import { EmitType } from '@syncfusion/ej2-base/';
import { FilteringEventArgs } from '@syncfusion/ej2-dropdowns';
import { Query } from '@syncfusion/ej2-data/';
import { ArticleNoService } from 'src/app/_core/_service/articleNoService.service';
import { DataService } from 'src/app/_core/_service/data.service';
import { Subscription, SubscriptionLike } from 'rxjs';
import { Button } from '@syncfusion/ej2-angular-buttons';
import { IRole } from 'src/app/_core/_model/role';
import { DatePicker } from '@syncfusion/ej2-angular-calendars';
import { ActionConstant } from 'src/app/_core/_constants';
const HttpUploadOptions = {
  headers: new HttpHeaders({ Accept: 'application/json' }),
};
const ROLE_ADMIN = 1;
const DONE = 'done';
const UNDONE = 'undone';
const ALL = 'all';
@Component({
  // tslint:disable-next-line:component-selector
  selector: 'app-BPFCSchedule',
  templateUrl: './BPFCSchedule.component.html',
  styleUrls: ['./BPFCSchedule.component.css'],
  providers: [DatePipe],
})
export class BPFCScheduleComponent extends BaseComponent implements OnInit, OnDestroy {
  @ViewChildren('tooltip') tooltip: QueryList<any>;

  pageSettings = {
    pageCount: 20,
    pageSizes: [50, 100, 150, 200, 'All'],
    pageSize: 50,
  };
  data: any[];
  file: any;

  @ViewChild('grid')
  public gridObj: GridComponent;
  modalReference: NgbModalRef;
  @ViewChild('importModal', { static: true })
  importModal: TemplateRef<any>;
  excelDownloadUrl: string;
  users: any[] = [];
  filterSettings: { type: string };
  modelName: string = null;
  modelNo: string = null;
  articleNo: string = null;
  articleNoNew: string = null;
  articleNoDefault: string = null;
  artProcess: string = null;
  value: string = null;
  modelNameID = 0;
  modelNoID = 0;
  articleNoID = 0;
  artProcessID = 0;
  BPFCID = 0;
  articleNosData: any;
  modelNOsDataClone: any;
  baseUrl = environment.apiUrlEC;
  filetest: any;
  textSearch: string = null;
  textSearchs: string = null;
  dataText: any;
  public fieldsBPFCs: object = { text: 'name', value: 'id' };
  sortSettings = { columns: [{ field: 'articleNo', direction: 'Ascending' }] };
  public searchOptions: SearchSettingsModel;
  subscriptions: Subscription = new Subscription();
  checkCode = false;
  percentageOfDone: Button;
  doneBtn: Button;
  undoneBtn: Button;
  allBtn: Button;
  percentageOfDoneData: any;
  undoneText: string;
  doneText: string;
  isDone: boolean;
  styleBtn = {
    background: '#6c757d',
    boxShadow: '0 0 0 3px rgba(130, 138, 145, 0.5)',
    border: '1px solid #6c757d',
    margin: '0',
    borderColor: '#6c757d',
    borderRadius: '4px',
    color: '#fff',
  };
  role: IRole;
  dueDate: Date;
  toolbar = [
    'Search',
    {
      text: 'Undone',
      tooltipText: 'Undone',
      prefixIcon: 'fa fa-remove',
      id: 'Undone',
    },
    {
      text: 'Done',
      tooltipText: 'Done',
      prefixIcon: 'fa fa-check',
      id: 'Done',
    },
    {
      text: 'All',
      tooltipText: 'All',
      prefixIcon: 'fa fa-list',
      id: 'All',
    },
    {
      text: '% of done',
      tooltipText: '% of done',
      prefixIcon: '',
      id: 'percentageOfDone',
    },
  ];
  keySearch: string;
  tab: string;
  constructor(
    private modalNameService: ModalNameService,
    private alertify: AlertifyService,
    private userService: UserService,
    private bPFCEstablishService: BPFCEstablishService,
    public modalService: NgbModal,
    private router: Router,
    private datePipe: DatePipe,
    private spinner: NgxSpinnerService,
    private articleNoService: ArticleNoService,
    private dataService: DataService,
    private http: HttpClient,
    private route: ActivatedRoute,
  ) {
    super();
    this.Permission(this.route);
  }

  ngOnInit() {
    this.getAllUsers();
    this.tab = this.route.snapshot.params.tab || UNDONE;
    // this.router.navigate([`/ec/establish/bpfc-schedule/${this.tab}`]);
    this.onRouteChange();
    const ROLE: IRole = JSON.parse(localStorage.getItem('level'));
    this.role = ROLE;
    this.excelDownloadUrl = `${environment.apiUrlEC}ModelName/ExcelExport`;
    this.filterSettings = { type: 'Excel' };
    this.dataText = this.dataService.currentMessages.subscribe((res: any) => {
      this.textSearch = res;
      if (res !== 0) {
        this.searchOptions = {
          fields: [
            'modelName',
            'modelNo',
            'articleNo',
            'approvedBy',
            'artProcess',
          ],
          operator: 'contains',
          key: res || '',
          ignoreCase: true,
        };
      }
    });
  }

  onRouteChange() {
    this.route.data.subscribe(data => {
      if (this.route.snapshot.params.keySearch !== undefined) {
        this.keySearch = this.route.snapshot.params.keySearch;
        const tab = this.route.snapshot.params.tab;
        this.tab = tab;
        this.loadData();
      }
    });
  }
  ngOnDestroy() {
    this.dataText.unsubscribe();
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
      toolbarOptions.push(...this.toolbar);
      const uniqueOptionItem = toolbarOptions.filter((elem, index, self) => {
        return index === self.indexOf(elem);
      });
      this.toolbarOptions = uniqueOptionItem;
    }
  }
  makeAction(input: string): string[] {
    switch (input) {
      case ActionConstant.EXCEL_EXPORT:
        return ['ExcelExport'];
      case ActionConstant.EXCEL_IMPORT:
        return ['Excel Import'];
      default:
        return [undefined];
    }
  }
  detail(data) {
    this.dataService.changeMessage(this.textSearch);
    return this.router.navigate([
      `/ec/establish/bpfc-schedule/${this.tab}/detailv2/${data.id}`,
    ]);
  }
  onChangeArticleNo(args) {
    if (args.isInteracted) {
      this.articleNoID = args.itemData.id;
    }
  }
  onChangeDueDate(args) {
    this.dueDate = args.value as Date;
  }
  createdSearch(args) {
    this.loadData();
    const gridElement = this.gridObj.element;
    const span = document.createElement('span');
    span.className = 'e-clear-icon';
    span.id = gridElement.id + 'clear';
    span.onclick = this.cancelBtnClick.bind(this);
    gridElement
      .querySelector('.e-toolbar-item .e-input-group')
      .appendChild(span);
    this.percentageOfDone = new Button(
      {
        cssClass: `e-tbar-btn e-tbtn-txt e-control e-lib`,
        iconCss: '',
      });
    this.percentageOfDone.appendTo('#percentageOfDone');
    if (this.keySearch) {
      this.gridObj.searchSettings.key = this.keySearch;
    }

  }

  public cancelBtnClick(args) {
    this.gridObj.searchSettings.key = '';
    (this.gridObj.element.querySelector(
      '.e-input-group.e-search .e-input'
    ) as any).value = '';
  }

  getSearchText() {
    // this.textSearch = this.textSearch ;
    // this.dataText = this.dataService.currentMessage.subscribe((res: any) => {
    //   if (res !== 0) {
    //     this.searchOptions = {
    //       fields: [
    //         "modelName",
    //         "modelNo",
    //         "articleNo",
    //         "approvedBy",
    //         "artProcess",
    //       ],
    //       operator: "contains",
    //       key: res || "",
    //       ignoreCase: true,
    //     };
    //   }
    // });
  }

  public onFilteringArticleNO: EmitType<FilteringEventArgs> = (
    e: FilteringEventArgs
  ) => {
    let query: Query = new Query();
    // frame the query based on search string with filter type.
    query =
      e.text !== '' ? query.where('name', 'contains', e.text, true) : query;
    // pass the filter data source, filter query to updateData method.
    e.updateData(this.articleNosData, query);
  }

  public onFilteringModelNOClone: EmitType<FilteringEventArgs> = (
    e: FilteringEventArgs
  ) => {
    let query: Query = new Query();
    // frame the query based on search string with filter type.
    query =
      e.text !== '' ? query.where('name', 'contains', e.text, true) : query;
    // pass the filter data source, filter query to updateData method.
    e.updateData(this.modelNOsDataClone, query);
  }

  getArticleNoByModelNoID(modelNoID, articleNo) {
    this.articleNoService
      .getArticleNoByModelNoID(modelNoID)
      .subscribe((res: any) => {
        this.articleNosData = res.filter(item => item.name !== articleNo);
      });
  }

  openPopupDropdownlist() { }

  onBeforeRender(args, data, i) {
    const t = this.tooltip.filter((item, index) => index === +i)[0];
    t.content = 'Loading...';
    t.dataBind();
    this.bPFCEstablishService.getGlueByBPFCID(data.id).subscribe((res: any) => {
      t.content = res.join('<br>');
      t.dataBind();
    });
  }

  onClickCloneNewVersion() {
    if (this.value !== null) {
      this.articleNoNew = this.articleNoDefault + this.value;
    }
    const clone = {
      modelNameID: this.modelNameID,
      modelNOID: this.modelNoID,
      articleNOID: this.articleNoID,
      artProcessID: Number(this.artProcessID),
      bpfcID: this.BPFCID,
      name: this.articleNoNew,
      cloneBy: JSON.parse(localStorage.getItem('user')).user.id,
    };
    this.cloneNewVersion(clone);
  }

  cloneNewVersion(clone) {
    this.findArticleNo(this.articleNoNew);
    if (this.value !== null) {
      if (this.checkCode) {
        this.modalNameService.cloneBPFC(clone).subscribe((res: any) => {
          if (res.status === true) {
            this.alertify.success('Đã sao chép thành công! <br> Copy succeeded!');
            this.modalService.dismissAll();
            this.getUndone();
            this.gridObj.search(this.articleNoNew);
            this.value = null;
          } else {
            this.alertify.error('The BPFC exists!');
            this.value = null;
          }
        });
      } else {
        this.alertify.error('The BPFC exists!');
        this.value = null;
      }
    } else {
      this.alertify.error('Version can not be Empty!');
      this.value = null;
    }
  }

  findArticleNo(code) {
    for (const item of this.data) {
      if (item.articleNo === code) {
        this.checkCode = false;
        break;
      } else {
        this.checkCode = true;
      }
    }
  }

  onClickClone() {
    const clone = {
      modelNameID: this.modelNameID,
      modelNOID: this.modelNoID,
      articleNOID: this.articleNoID,
      artProcessID: Number(this.artProcessID),
      bpfcID: this.BPFCID,
      cloneBy: JSON.parse(localStorage.getItem('user')).user.id,
    };
    this.clone(clone);
  }

  clone(clone) {
    this.modalNameService.clone(clone).subscribe((res: any) => {
      if (res.status === true) {
        this.alertify.success('Đã sao chép thành công! <br> Copy succeeded!');
        this.getUndone();
        this.gridObj.search(this.articleNoNew);
        this.modalService.dismissAll();
      } else {
        this.alertify.error('The BPFC exists!');
      }
    });
  }

  openModal(ref, data) {
    this.modalReference = this.modalService.open(ref, { size: 'md', backdrop: 'static', keyboard: false });
    this.BPFCID = data.id;
    this.modelName = data.modelName;
    this.modelNo = data.modelNo;
    this.articleNo = data.articleNo;
    this.artProcess = data.artProcess;
    this.articleNoNew = data.articleNo.split('-')[0] + '-T';
    this.articleNoDefault = data.articleNo.split('-')[0] + '-T';
    this.modelNameID = data.modelNameID;
    this.modelNoID = data.modelNoID;
    this.articleNoID = 0;

    if (data.artProcess === 'ASY') {
      this.artProcessID = 1;
    } else {
      this.artProcessID = 2;
    }
  }

  openModalClone(ref, data) {
    this.modalReference = this.modalService.open(ref, { size: 'md', backdrop: 'static', keyboard: false });
    this.BPFCID = data.id;
    this.modelName = data.modelName;
    this.modelNo = data.modelNo;
    this.artProcess = data.artProcess;
    this.articleNo = data.articleNo;
    this.modelNameID = data.modelNameID;
    this.modelNoID = data.modelNoID;
    this.articleNoID = 0;
    // this.artProcessID = data.artProcessID;
    this.getArticleNoByModelNoID(this.modelNoID, data.articleNo);
    if (data.artProcess === 'ASY') {
      this.artProcessID = 1;
    } else {
      this.artProcessID = 2;
    }
  }



  actionBegin(args) {
    if (args.requestType === 'searching') {
      this.textSearch = args.searchString;
    }
    if (args.requestType === 'save') {
      const entity = {
        id: args.data.id,
        season: args.data.season,
      };
      const previousSeason = args.previousData.season;
      if (previousSeason !== null || previousSeason !== '' && previousSeason !== args.data.season) {
        this.bPFCEstablishService.updateSeason(entity).subscribe(() => {
          this.alertify.success('Update season successfully');
          this.getAllUsers();
        });
      }
      const dueDateEntity = {
        id: args.data.id,
        dueDate: this.dueDate,
      };
      const previousDueDate = args.previousData.dueDate;
      if (args.data.artProcess === 'STF' && this.dueDate.toDateString() !== previousDueDate && previousDueDate != null) {
        this.bPFCEstablishService.updateDueDate(dueDateEntity).subscribe(() => {
          this.getAllUsers();
          this.alertify.success('Update due date successfully');
        });
      }
    }
  }

  dataBound(args) {
    // console.log('databound', args);
    // (this.gridObj.columns[0] as any).isPrimaryKey = 'true';
    //  this.gridObj.autoFitColumns();
  }

  toolbarClick(args) {
    switch (args.item.id) {
      case 'grid_Excel Import':
        this.showModal(this.importModal);
        break;
      case 'Done':
        this.gridObj.searchSettings.key = '';
        this.tab = DONE;
        this.router.navigate([`/ec/establish/bpfc-schedule/${this.tab}`]);
        this.getDone();
        break;
      case 'Undone':
        this.gridObj.searchSettings.key = '';
        this.tab = UNDONE;
        this.router.navigate([`/ec/establish/bpfc-schedule/${this.tab}`]);
        this.getUndone();
        break;
      case 'All':
        this.gridObj.searchSettings.key = '';
        this.tab = ALL;
        this.router.navigate([`/ec/establish/bpfc-schedule/${this.tab}`]);
        this.getAll();
        break;
      case 'grid_Excel Export':
        const data = this.data.map((item) => {
          return {
            approvedBy: item.approvedBy,
            approvalStatus: item.approvalStatus,
            createdBy: item.createdBy,
            articleNo: item.articleNo,
            createdDate: this.datePipe.transform(
              item.createdDate,
              'd MMM, yyyy HH:mm'
            ),
            artProcess: item.artProcess,
            finishedStatus: item.finishedStatus === true ? 'Yes' : 'No',
            modelName: item.modelName,
            modelNo: item.modelNo,
            season: item.season,
          };
        });
        const exportProperties = {
          dataSource: data,
        };
        this.gridObj.excelExport(exportProperties);
        break;
    }
  }

  fileProgress(event) {
    this.file = event.target.files[0];
  }

  uploadFile() {
    const createdBy = JSON.parse(localStorage.getItem('user')).user.id;
    this.bPFCEstablishService
      .import(this.file, createdBy)
      .subscribe((res: any) => {
        this.getAll();
        this.modalReference.close();
        this.alertify.success('The excel has been imported into system!');
      });
  }
  loadData() {
    switch (this.tab) {
      case UNDONE:
        this.getUndone();
        break;
      case DONE:
        this.getDone();
        break;
      case ALL:
        this.getAll();
        break;
    }
  }
  getAllUsers() {
    this.userService.getAllUserInfo().subscribe((res: any) => {
      this.users = res;
      console.log('getAllUsers', this.users);
    });
  }
  delete(id: number) {
    this.alertify.confirm('Delete BPFC', 'Are you sure you want to delete this BPFC "' + id + '" ?', () => {
      this.bPFCEstablishService.delete(id).subscribe((res: any) => {
        this.alertify.success('Success');
        this.getUndone();
      }, err => { this.alertify.warning(err); });
    });
  }
  rowDB(args) {
    const data = args.data as any;
    if (data.dueDateStatus) {
      args.row.classList.add('bgcolor');
    }
  }
  editTextOfBtn() {
    const allBtn = document.getElementById('All') as HTMLButtonElement;
    const undoneBtn = document.getElementById('Undone') as HTMLButtonElement;
    const doneBtn = document.getElementById('Done') as HTMLButtonElement;
    if (this.isDone === true) {
      this.setAttrBtn(doneBtn);
      // reset color
      this.resetAttrBtn(undoneBtn);
      this.resetAttrBtn(allBtn);
    } else if (this.isDone === false) {

      this.setAttrBtn(undoneBtn);
      // reset color
      this.resetAttrBtn(doneBtn);
      this.resetAttrBtn(allBtn);
    } else if (this.isDone === null) {

      this.setAttrBtn(allBtn);
      // reset color
      this.resetAttrBtn(doneBtn);
      this.resetAttrBtn(undoneBtn);
    }
  }
  getAll() {
    this.spinner.show();
    this.bPFCEstablishService.getAll().subscribe((res: any) => {
      this.data = res.map((item: any) => {
        return {
          id: item.id,
          modelNameID: item.modelNameID,
          modelNoID: item.modelNoID,
          articleNoID: item.articleNoID,
          artProcessID: item.artProcessID,
          modelName: item.modelName,
          modelNo: item.modelNo,
          createdDate: new Date(item.createdDate),
          articleNo: item.articleNo,
          approvalStatus: item.approvalStatus,
          finishedStatus: item.finishedStatus,
          dueDate: item.dueDate,
          dueDateStatus: item.dueDateStatus,
          approvedBy: this.users.filter((a) => a.id === item.approvalBy)[0]
            ?.username,
          createdBy: item.createdBy,
          artProcess: item.artProcess,
          season: item.season,
        };
      });
      this.spinner.hide();
      this.isDone = null;
      this.editTextOfBtn();
    });
  }

  getDone() {
    this.spinner.show();
    this.bPFCEstablishService.getDoneBPFC().subscribe((res: any) => {
      this.data = res.data.map((item: any) => {
        return {
          id: item.id,
          modelNameID: item.modelNameID,
          modelNoID: item.modelNoID,
          articleNoID: item.articleNoID,
          artProcessID: item.artProcessID,
          modelName: item.modelName,
          modelNo: item.modelNo,
          createdDate: new Date(item.createdDate),
          articleNo: item.articleNo,
          dueDate: item.dueDate,
          dueDateStatus: item.dueDateStatus,
          approvalStatus: item.approvalStatus,
          finishedStatus: item.finishedStatus,
          approvedBy: this.users.filter((a) => a.id === item.approvalBy)[0]
            ?.username,
          createdBy: item.createdBy,
          artProcess: item.artProcess,
          season: item.season,
        };
      });
      this.percentageOfDoneData = res.percentageOfDone;
      this.percentageOfDone.element.innerHTML = this.percentageOfDoneData;
      this.doneText = `Done (${res.doneTotal})`;
      this.undoneText = `Undone (${res.undoneTotal})`;
      this.isDone = true;
      this.editTextOfBtn();
      this.spinner.hide();
    });
  }

  getUndone() {
    this.spinner.show();
    this.bPFCEstablishService.getUndoneBPFC().subscribe((res: any) => {
      this.data = res.data.map((item: any) => {
        return {
          id: item.id,
          modelNameID: item.modelNameID,
          modelNoID: item.modelNoID,
          articleNoID: item.articleNoID,
          artProcessID: item.artProcessID,
          modelName: item.modelName,
          modelNo: item.modelNo,
          createdDate: new Date(item.createdDate),
          articleNo: item.articleNo,
          dueDate: item.dueDate,
          dueDateStatus: item.dueDateStatus,
          approvalStatus: item.approvalStatus,
          finishedStatus: item.finishedStatus,
          approvedBy: this.users.filter((a) => a.id === item.approvalBy)[0]
            ?.username,
          createdBy: item.createdBy,
          artProcess: item.artProcess,
          season: item.season,
        };
      });
      this.percentageOfDoneData = res.percentageOfDone;
      this.percentageOfDone.element.innerHTML = this.percentageOfDoneData;
      this.doneText = `Done (${res.doneTotal})`;
      this.undoneText = `Undone (${res.undoneTotal})`;
      this.isDone = false;
      this.editTextOfBtn();
      this.spinner.hide();
    });
  }

  showModal(importModal) {
    this.modalReference = this.modalService.open(importModal, { size: 'xl' });
  }

  NO(index) {
    return (
      (this.gridObj.pageSettings.currentPage - 1) *
      this.gridObj.pageSettings.pageSize +
      Number(index) +
      1
    );
  }
  // helper function

  setAttrBtn(btn: HTMLButtonElement, styleBtn = {
    background: '#6c757d',
    boxShadow: '0 0 0 3px rgba(130, 138, 145, 0.5)',
    border: '1px solid #6c757d',
    margin: '0',
    borderColor: '#6c757d',
    borderRadius: '4px',
    color: '#fff',
  }) {
    btn.style.background = styleBtn.background;
    btn.style.boxShadow = styleBtn.boxShadow;
    btn.style.border = styleBtn.border;
    btn.style.margin = styleBtn.margin;
    btn.style.borderColor = styleBtn.borderColor;
    btn.style.borderRadius = styleBtn.borderRadius;
    btn.style.color = styleBtn.color;
  }
  resetAttrBtn(btn: HTMLButtonElement, styleBtn = {
    background: '#f8f9fa',
    boxShadow: 'none',
    border: 'none',
    margin: '0',
    borderColor: '#f8f9fa',
    borderRadius: 'none',
    color: '#495057',
  }) {
    btn.style.background = styleBtn.background;
    btn.style.boxShadow = styleBtn.boxShadow;
    btn.style.border = styleBtn.border;
    btn.style.margin = styleBtn.margin;
    btn.style.borderColor = styleBtn.borderColor;
    btn.style.borderRadius = styleBtn.borderRadius;
    btn.style.color = styleBtn.color;
  }
}
