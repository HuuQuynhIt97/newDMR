import { SubpackageComponent } from '../subpackage/subpackage.component';
import { DispatchEVAUVComponent } from '../dispatchEVAUV/dispatchEVAUV.component';
import { AfterViewInit, Component, HostListener, OnDestroy, OnInit, QueryList, TemplateRef, ViewChild, ViewChildren } from '@angular/core';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { GridComponent, PageSettingsModel, RowDataBoundEventArgs } from '@syncfusion/ej2-angular-grids';
import { Subscription } from 'rxjs';
import { IBuilding } from 'src/app/_core/_model/building';
import { IRole } from 'src/app/_core/_model/role';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { BuildingService } from 'src/app/_core/_service/building.service';
import { PlanService } from 'src/app/_core/_service/plan.service';
import { EmitType } from '@syncfusion/ej2-base/';
import { Query } from '@syncfusion/ej2-data/';
import { FilteringEventArgs } from '@syncfusion/ej2-angular-dropdowns';
import * as signalr from '../../../../../assets/js/ec-client.js';

import { DataService } from 'src/app/_core/_service/data.service';
import { TodolistService } from 'src/app/_core/_service/todolist.service';
import { IToDoList, IToDoListForCancel } from 'src/app/_core/_model/IToDoList';
import { ClickEventArgs, ToolbarComponent } from '@syncfusion/ej2-angular-navigations';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxSpinnerService } from 'ngx-spinner';
import { DatePipe } from '@angular/common';
import { HubConnectionState } from '@microsoft/signalr';
import { BottomFactoryService } from 'src/app/_core/_service/bottom.factory.service';
import { Kind } from 'src/app/_core/_model/kind';
import { AuthService } from 'src/app/_core/_service/auth.service';
import { PrintGlueComponent } from '../print-glue/print-glue.component.js';
import { DispatchDoneListComponent } from '../dispatch-done-list/dispatch-done-list.component.js';
import { PrintGlueDispatchListComponent } from '../print-glue-dispatch-list/print-glue-dispatch-list.component.js';
import { DispatchComponent } from '../dispatch/dispatch.component';
import { AuthenticationService } from 'src/app/_core/_service/authentication.service';
import { map } from 'rxjs/operators';

declare var $: any;
const ADMIN = 1;
const SUPERVISOR = 2;
const BUILDING_LEVEL = 2;
const STAFF = 3;
const WORKER = 4;
const DISPATCHER = 6;
@Component({
  selector: 'app-todolist',
  templateUrl: './todolist.component.html',
  styleUrls: ['./todolist.component.css'],
  providers: [DatePipe]
})
export class TodolistComponent implements OnInit, OnDestroy, AfterViewInit {
  // Start Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
  Glue: any = [];
  dataAddition: any = [];
  dataAdditionDispatch: any = [];
  dataHistoryMixed: any = []; // Thêm bởi Quỳnh (Leo 2/2/2021 11:46)
  @ViewChild('historyMixed', { static: true }) historyMixed: TemplateRef<any>;
  @ViewChild('addition', { static: true }) addition: TemplateRef<any>;
  @ViewChild('additionDispatch', { static: true }) additionDispatch: TemplateRef<any>;
  modalReference: NgbModalRef;
  public fieldsBPFC: object = { text: 'name', value: 'name' };
  AddGlueNameID = 0;
  AddGlueID = 0;
  AddEstimatedStartTime = new Date();
  AddEstimatedFinishTime = new Date();
  startWorkingTime = new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDay(), 7, 0, 0);
  finishWorkingTime = new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDay(), 16, 30, 0);
  toolbarAddition = ['Add', 'Cancel'];
  toolbarAdditionDispatch = ['Add', 'Cancel'];
  // tslint:disable-next-line:variable-name
  total_amount: string = null; // Thêm bởi Quỳnh (Leo 2/2/2021 11:46)
  // tslint:disable-next-line:variable-name
  glueMix_name: string = null; // Thêm bởi Quỳnh (Leo 2/2/2021 11:46)
  // End Thêm bởi Quỳnh (Leo 1/28/2021 11:46)

  @ViewChild('toolbar') toolbar: ToolbarComponent;
  @ViewChild('toolbarTodo') toolbarTodo: ToolbarComponent;
  @ViewChild('toolbarDone') toolbarDone: ToolbarComponent;
  @ViewChild('toolbarDelay') toolbarDelay: ToolbarComponent;
  @ViewChild('toolbarDispatch') toolbarDispatch: ToolbarComponent;
  @ViewChild('toolbarDispatchDelay') toolbarDispatchDelay: ToolbarComponent;

  @ViewChild('gridEVAUV') gridEVAUV: GridComponent;
  @ViewChild('gridDone') gridDone: GridComponent;
  @ViewChild('gridAdditionDispatch') gridAdditionDispatch: GridComponent;
  @ViewChildren('tooltip') tooltip: QueryList<any>;

  @ViewChild('gridTodo') gridTodo: GridComponent;
  @ViewChild('gridDelay') gridDelay: GridComponent;
  @ViewChild('gridDispatch') gridDispatch: GridComponent;
  @ViewChild('gridDispatchDelay') gridDispatchDelay: GridComponent;
  focusDone: string;
  @ViewChild('fullScreen') divRef;
  sortSettings: object;
  pageSettings: PageSettingsModel;
  toolbarOptions: object;
  editSettings: object;
  searchSettings: any = { hierarchyMode: 'Parent' };
  fieldsBuildings: object = { text: 'name', value: 'id' };
  setFocus: any;
  data: IToDoList[];
  EVAUVData: any;
  doneData: IToDoList[];
  building: IBuilding[];
  role: IRole;
  buildingID: number;
  isShowTab: string;
  subscription: Subscription[] = [];
  IsAdmin: boolean;
  buildings: IBuilding[];
  buildingName: any;
  glueName = '';
  todoTotal = 0;
  doneTotal = 0;
  total = 0;
  percentageOfDone = 0;

  doneTotalDispatch = 0;

  dispatchTotal = 0;
  todoDispatchTotal = 0;
  delayDispatchTotal = 0;
  percentageOfDoneDispatch = 0;

  hasCloseScreen: boolean;
  hasShowFullScreen = true;
  public fullscreenBtn: any;
  delayTotal = 0;
  RUBBER = Kind.RUBBER as number;
  TODO = 'todo';
  DONE = 'done';
  DELAY = 'delay';
  DISPATCH = 'dispatch';
  DISPATCH_DELAY = 'dispatchDelay';
  EVA_UV = 'EVA_UV';
  tabs = [this.TODO, this.DONE, this.DELAY, this.DISPATCH, this.DISPATCH_DELAY, this.EVA_UV];
  dispatchData: any;
  // tslint:disable-next-line:variable-name
  current_Date_Time = this.datePipe.transform(new Date(), 'HH:mm');
  doneDispatchTotal = 0;
  glueDispatchList: {
    id: number; planID: number;
    name: string; glueID: number; glueNameID: number; supplier: string; estimatedStartTime: Date; estimatedFinishTime: Date;
  }[];
  glueNameIDDispatch: any;
  isSTF: boolean;
  @HostListener('fullscreenchange', ['$event']) fullscreenchange(e) {
    // if (document.fullscreenElement) {
    //   this.fullscreenBtn.iconCss = 'fas fa-compress-arrows-alt';
    //   this.fullscreenBtn.content = 'CloseScreen';
    // } else {
    //   this.fullscreenBtn.iconCss = 'fa fa-expand-arrows-alt';
    //   this.fullscreenBtn.content = 'FullScreen';
    // }
  }
  constructor(
    private planService: PlanService,
    private buildingService: BuildingService,
    private bottomFactoryService: BottomFactoryService,
    private alertify: AlertifyService,
    public modalService: NgbModal,
    public dataService: DataService,
    private route: ActivatedRoute,
    private router: Router,
    private datePipe: DatePipe,
    private authService: AuthService,
    private spinner: NgxSpinnerService,
    public todolistService: TodolistService
  ) {
    const ROLE: IRole = JSON.parse(localStorage.getItem('level'));
    this.role = ROLE;
    this.building = JSON.parse(localStorage.getItem('building'));
    this.buildingID = +localStorage.getItem('buildingID');
  }
  ngOnDestroy() {
    this.subscription.forEach(subscription => subscription.unsubscribe());
    if (signalr.CONNECTION_HUB.state === HubConnectionState.Connected) {
      signalr.CONNECTION_HUB.stop().then().catch(err => console.log(err));
    }
  }
  ngAfterViewInit() {
  }
  ngOnInit() {
    this.focusDone = this.TODO;
    this.isShowTab = this.TODO;
    this.onRouteChange();
    this.hasCloseScreen = false;
    this.IsAdmin = false;
    this.checkRole();
    this.onEventHub();
    this.gridConfig();
    this.subscription.push(this.todolistService.getValue().subscribe(status => {
      if (status !== null) {
        this.buildingID = this.building[0].id;
        this.loadData();
      }
    }));
  }
  onEventHub() {
    if (signalr.CONNECTION_HUB.state === HubConnectionState.Connected) {
      signalr.CONNECTION_HUB.on('ReloadTodo', () => {
        if (this.isShowTab === this.TODO) {
          this.loadData();
          console.log('Reload Todo', '');
        }
      });

      signalr.CONNECTION_HUB.on('ReloadDispatch', () => {
        if (this.isShowTab === this.DISPATCH) {
          this.loadData();
          console.log('Reload dispatch', '');
        }
      });
    }

  }
  onRouteChange() {
    this.route.data.subscribe(data => {
      if (this.route.snapshot.params.glueName !== undefined) {
        this.glueName = this.route.snapshot.params.glueName.includes('%2B')
          ? this.route.snapshot.params.glueName.replace(/\%2B/g, '+')
          : this.route.snapshot.params.glueName;
      }
      if (this.buildingID === 0) {
        this.glueName = '';
      }
      const tab = this.route.snapshot.params.tab;
      switch (tab) {
        case this.TODO:
          this.isShowTab = tab;
          this.focusDone = tab;
          this.loadData();
          break;
        case this.DELAY:
          this.isShowTab = tab;
          this.focusDone = tab;
          this.loadData();
          break;
        case this.DONE:
          this.isShowTab = tab;
          this.focusDone = tab;
          this.loadData();
          break;
        case this.DISPATCH:
          this.isShowTab = tab;
          this.focusDone = tab;
          this.loadData();
          break;
        case this.DISPATCH_DELAY:
          this.isShowTab = tab;
          this.focusDone = tab;
          this.loadData();
          break;
        case this.EVA_UV:
          this.isShowTab = tab;
          this.focusDone = tab;
          this.loadData();
          break;
      }
    });
  }
  getBuilding(callback): void {
    this.buildingService.getBuildings()
    .pipe(
      map( data => {
        return data.filter(item => item.level === BUILDING_LEVEL);
      })
    )
    .subscribe((buildingData) => {
      this.buildings = buildingData;
      callback();
    });
    const userID = +JSON.parse(localStorage.getItem('user')).user.id;
    this.authService.getBuildingUserByUserID(userID).subscribe((res) => {
      this.buildings = res.data;
      callback();
    });
  }
  cancelRange(): void {
    const data = this.gridTodo.getSelectedRecords() as IToDoList[];
    const model: IToDoListForCancel[] = data.map(item => {
      const todo: IToDoListForCancel = {
        id: item.id,
        lineNames: item.lineNames
      };
      return todo;
    });
    this.todolistService.cancelRange(model).subscribe((res) => {
      this.alertify.success('Xóa thành công! <br> Success!');
    });
  }
  cancel(todo: IToDoList): void {
    this.alertify.confirm('Cancel', 'Bạn có chắc chắn muốn hủy keo này không? Are you sure you want to get rid of this data?', () => {
      const model: IToDoListForCancel = {
        id: todo.id,
        lineNames: todo.lineNames
      };
      this.todolistService.cancel(model).subscribe((res) => {
        this.todo();
        this.alertify.success('Xóa thành công! <br> Success!');
      });

    });
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
  onChangeBuilding(args) {
    this.buildingID = args.itemData.id;
    this.buildingName = args.itemData.name;
    const building = JSON.stringify([args.itemData]);
    localStorage.setItem('buildingID', args.itemData.id);
    localStorage.setItem('building', building);
    localStorage.setItem('isSTF', args.itemData.isSTF);
    this.isSTF = args.itemData.isSTF;
    this.building = [args.itemData];
    this.loadData();
  }
  onSelectBuildingToDo(args: any): void {
    this.buildingID = args.itemData.id;
    this.buildingName = args.itemData.name;
    const building = JSON.stringify([args.itemData]);
    localStorage.setItem('buildingID', args.itemData.id);
    localStorage.setItem('building', building);
    this.building = [args.itemData];
    localStorage.setItem('isSTF', args.itemData.isSTF);
    this.isSTF = args.itemData.isSTF;
    this.loadData();
  }
  onSelectBuildingDelay(args: any): void {
    this.buildingID = args.itemData.id;
    this.buildingName = args.itemData.name;
    localStorage.setItem('buildingID', args.itemData.id);
    const building = JSON.stringify([args.itemData]);
    localStorage.setItem('building', building);
    this.building = [args.itemData];
    this.loadData();
  }
  onSelectBuildingDone(args: any): void {
    this.buildingID = args.itemData.id;
    this.buildingName = args.itemData.name;
    localStorage.setItem('buildingID', args.itemData.id);
    const building = JSON.stringify([args.itemData]);
    localStorage.setItem('building', building);
    this.building = [args.itemData];
    this.loadData();
  }
  loadData() {
    this.isSTF = JSON.parse(localStorage.getItem('isSTF')) as boolean;
    if (this.isSTF === true) {
      switch (this.isShowTab) {
        case this.TODO:
          this.todoSTF();
          break;
        case this.DELAY:
          this.undoneSTF();
          break;
        case this.DONE:
          this.doneSTF();
          break;
        case this.DISPATCH_DELAY:
          this.dispatchListDelaySTF();
          break;
        case this.DISPATCH:
          this.dispatchListSTF();
          break;
        case this.EVA_UV:
          this.EVA_UVList();
          break;
      }
    } else {
      switch (this.isShowTab) {
        case this.TODO:
          this.todo();
          break;
        case this.DELAY:
          this.delay();
          break;
        case this.DONE:
          this.done();
          break;
        case this.DISPATCH_DELAY:
          this.dispatchListDelay();
          break;
        case this.DISPATCH:
          this.dispatchList();
          break;
        case this.EVA_UV:
          this.EVA_UVList();
          break;
      }
    }
  }

  checkRole(): void {
    const buildingId = +localStorage.getItem('buildingID');
    this.building = JSON.parse(localStorage.getItem('building'));
    if (buildingId === 0) {
      this.getBuilding(() => {
        this.alertify.message('Please select a building! <br> Vui lòng chọn 1 tòa nhà!', true);
      });
    } else {
      this.getBuilding(() => {
        this.buildingID = buildingId;
        this.isShowTab = this.TODO;
        this.focusDone = this.TODO;
        this.loadData();
        this.todoAddition();
        this.dispatchAddition();
      });
    }
  }
  EVA_UVList() {
    this.bottomFactoryService.EVAUVList(this.buildingID).subscribe((res: any) => {
      this.EVAUVData = res.data;
      this.todoTotal = res.todoTotal;
      this.doneTotal = res.doneTotal;
      this.total = res.total;
      this.delayTotal = res.delayTotal;
      this.percentageOfDone = res.percentageOfDone;

      this.dispatchTotal = res.dispatchTotal;
      this.todoDispatchTotal = res.todoDispatchTotal;
      this.delayDispatchTotal = res.delayDispatchTotal;
      this.doneDispatchTotal = res.doneDispatchTotal;
      this.percentageOfDoneDispatch = res.percentageOfDoneDispatch;
    });
  }
  dispatchList() {
    this.todolistService.dispatchList(this.buildingID).subscribe((res: any) => {
      this.dispatchData = res.data;
      this.todoTotal = res.todoTotal;
      this.doneTotal = res.doneTotal;
      this.total = res.total;
      this.delayTotal = res.delayTotal;
      this.percentageOfDone = res.percentageOfDone;

      this.dispatchTotal = res.dispatchTotal;
      this.todoDispatchTotal = res.todoDispatchTotal;
      this.delayDispatchTotal = res.delayDispatchTotal;
      this.doneDispatchTotal = res.doneDispatchTotal;
      this.percentageOfDoneDispatch = res.percentageOfDoneDispatch;
    });
  }
  dispatchListDelay() {
    this.todolistService.dispatchListDelay(this.buildingID).subscribe((res: any) => {
      this.dispatchData = res.data;
      this.todoTotal = res.todoTotal;
      this.doneTotal = res.doneTotal;
      this.total = res.total;
      this.delayTotal = res.delayTotal;
      this.percentageOfDone = res.percentageOfDone;

      this.dispatchTotal = res.dispatchTotal;
      this.todoDispatchTotal = res.todoDispatchTotal;
      this.delayDispatchTotal = res.delayDispatchTotal;
      this.doneDispatchTotal = res.doneDispatchTotal;
      this.percentageOfDoneDispatch = res.percentageOfDoneDispatch;
    });
  }
  todo() {
    this.todolistService.todo(this.buildingID).subscribe(res => {
      this.data = res.data;
      this.todoTotal = res.todoTotal;
      this.doneTotal = res.doneTotal;
      this.total = res.total;
      this.delayTotal = res.delayTotal;
      this.percentageOfDone = res.percentageOfDone;

      this.dispatchTotal = res.dispatchTotal;
      this.todoDispatchTotal = res.todoDispatchTotal;
      this.delayDispatchTotal = res.delayDispatchTotal;
      this.doneDispatchTotal = res.doneDispatchTotal;
      this.percentageOfDoneDispatch = res.percentageOfDoneDispatch;
    });
  }
  todoAddition() {
    this.todolistService.todoAddition(this.buildingID).subscribe(res => {
      //  Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
      this.Glue = res.data.map((item) => {
        if (item.abnormalStatus === false) {
          return {
            id: item.id,
            planID: item.planID,
            name: `${item.glueName} | ${item.lineNames.join(',')}`,
            glueID: item.glueID,
            glueNameID: item.glueNameID,
            supplier: item.supplier,
            estimatedStartTime: item.estimatedStartTime,
            estimatedFinishTime: item.estimatedFinishTime
          };
        }
      });
    });
  }
  dispatchAddition() {
    this.todolistService.dispatchAddition(this.buildingID).subscribe(res => {
      //  Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
      this.glueDispatchList = res.data.map((item) => {
        return {
          id: item.id,
          planID: item.planID,
          name: `${item.glueName} | ${item.lineNames.join(',')}`,
          glueID: item.glueID,
          glueNameID: item.glueNameID,
          supplier: item.supplier,
          estimatedStartTime: item.estimatedStartTime,
          estimatedFinishTime: item.estimatedFinishTime
        };
      });
    });
  }
  delay() {
    this.todolistService.delay(this.buildingID).subscribe(res => {
      this.data = res.data;
      this.todoTotal = res.todoTotal;
      this.doneTotal = res.doneTotal;
      this.delayTotal = res.delayTotal;
      this.total = res.total;
      this.percentageOfDone = res.percentageOfDone;

      this.dispatchTotal = res.dispatchTotal;
      this.todoDispatchTotal = res.todoDispatchTotal;
      this.delayDispatchTotal = res.delayDispatchTotal;

      this.doneDispatchTotal = res.doneDispatchTotal;
      this.percentageOfDoneDispatch = res.percentageOfDoneDispatch;
    });
  }
  done() {
    this.todolistService.done(this.buildingID).subscribe(res => {
      this.doneData = res.data;
      this.todoTotal = res.todoTotal;
      this.doneTotal = res.doneTotal;
      this.delayTotal = res.delayTotal;
      this.total = res.total;
      this.percentageOfDone = res.percentageOfDone;

      this.dispatchTotal = res.dispatchTotal;
      this.todoDispatchTotal = res.todoDispatchTotal;
      this.doneDispatchTotal = res.doneDispatchTotal;
      this.delayDispatchTotal = res.delayDispatchTotal;
      this.percentageOfDoneDispatch = res.percentageOfDoneDispatch;
    });
  }
  gridConfig(): void {
    this.pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
    this.sortSettings = {};
    this.editSettings = { showDeleteConfirmDialog: false, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Normal' };
    this.toolbarOptions = ['Search'];
  }
  dataBoundDone() {
    this.gridDone.autoFitColumns();
  }
  dataBound() {
  }
  createdDispatchGrid() {
    if (this.glueName !== undefined) {
      this.gridDispatch.search(this.glueName);
    }
  }
  createdTodoGrid() {
    if (this.glueName !== undefined) {
      this.gridTodo.search(this.glueName);
    }
  }
  public cancelBtnGridTodoClick(args) {
    this.glueName = '';
    switch (this.isShowTab) {
      case this.TODO:
      case this.DELAY:
        this.gridTodo.searchSettings.key = '';
        break;
      case this.DISPATCH:
      case this.DISPATCH_DELAY: // Chỉ hiện todolist
        this.gridDispatch.searchSettings.key = '';
        break;
      case this.DONE: // Chỉ hiện dispatchlist
        this.gridDone.searchSettings.key = '';
        break;
    }
  }
  createdToolbar() {
    // this.fullscreenBtn = new Button(
    //   {
    //     cssClass: `e-tbar-btn e-tbtn-txt e-control e-btn e-lib`,
    //     iconCss: 'fa fa-expand-arrows-alt',
    //     isToggle: true
    //   });
    // this.fullscreenBtn.appendTo('#screenToolbar');
    // this.fullscreenBtn.element.onclick = (): void => {
    //   if (this.fullscreenBtn.content === 'CloseScreen') {
    //     this.fullscreenBtn.iconCss = 'fa fa-expand-arrows-alt';
    //     this.fullscreenBtn.content = 'FullScreen';
    //     this.closeFullscreen();
    //   } else {
    //     this.openFullscreen();
    //     this.fullscreenBtn.iconCss = 'fas fa-compress-arrows-alt';
    //     this.fullscreenBtn.content = 'CloseScreen';
    //   }
    // };
  }
  createdTodo() {
    const tab = window.location.pathname;
    for (const item of this.tabs) {
      if (tab.includes(`-2/${item}`)) {
        this.focusDone = item;
        this.isShowTab = item;
        break;
      } else {
        this.focusDone = this.TODO;
        this.isShowTab = this.TODO;
      }
    }
    console.log('oninit', this.isShowTab, this.focusDone);
    const toolbarTodo = this.toolbarTodo.element;
    const span = document.createElement('span');
    span.className = 'e-clear-icon';
    span.id = toolbarTodo.id + 'clear';
    span.onclick = this.cancelBtnGridTodoClick.bind(this);
    toolbarTodo
      .querySelector('.e-toolbar-item .e-input-group')
      .appendChild(span);
    const todoButton: HTMLElement = (this.toolbarTodo.element as HTMLElement).querySelector('#todo');
    const delayButton: HTMLElement = (this.toolbarTodo.element as HTMLElement).querySelector('#delay');
    const delayDispatchButton: HTMLElement = (this.toolbarTodo.element as HTMLElement).querySelector('#delayDispatchList');
    const dispatchButton: HTMLElement = (this.toolbarTodo.element as HTMLElement).querySelector('#dispatch');
    const doneButton: HTMLElement = (this.toolbarTodo.element as HTMLElement).querySelector('#done');
    const EVAUVButton: HTMLElement = (this.toolbarTodo.element as HTMLElement).querySelector('#eva_uv');
    switch (this.isShowTab) {
      case this.TODO:
        todoButton?.classList.add('todo');

        delayButton?.classList.remove('todo');
        delayDispatchButton?.classList.remove('todo');
        dispatchButton?.classList.remove('todo');
        doneButton?.classList.remove('todo');
        EVAUVButton?.classList.remove('todo');
        break;
      case this.DELAY:
        delayButton?.classList.add('todo');

        todoButton?.classList.remove('todo');
        todoButton?.classList.remove('todo');
        doneButton?.classList.remove('todo');
        dispatchButton?.classList.remove('todo');
        EVAUVButton?.classList.remove('todo');
        break;
      case this.DISPATCH:
        dispatchButton?.classList.add('todo');

        todoButton?.classList.remove('todo');
        delayButton?.classList.remove('todo');
        doneButton?.classList.remove('todo');
        delayDispatchButton?.classList.remove('todo');
        EVAUVButton?.classList.remove('todo');
        break;
      case this.DISPATCH_DELAY:
        delayDispatchButton?.classList.add('todo');

        todoButton?.classList.remove('todo');
        delayButton?.classList.remove('todo');
        doneButton?.classList.remove('todo');
        dispatchButton?.classList.remove('todo');
        EVAUVButton?.classList.remove('todo');
        break;
      case this.DONE:
        doneButton?.classList.add('todo');

        todoButton?.classList.remove('todo');
        delayButton?.classList.remove('todo');
        dispatchButton?.classList.remove('todo');
        delayDispatchButton?.classList.remove('todo');
        EVAUVButton?.classList.remove('todo');
        break;
      case this.EVA_UV:
        EVAUVButton?.classList.add('todo');

        todoButton?.classList.remove('todo');
        delayButton?.classList.remove('todo');
        dispatchButton?.classList.remove('todo');
        delayDispatchButton?.classList.remove('todo');
        doneButton?.classList.remove('todo');
        break;
    }
    if (todoButton !== null) {
        todoButton.onclick = (): void => {
        todoButton?.classList.add('todo');

        delayButton?.classList.remove('todo');
        delayDispatchButton?.classList.remove('todo');
        doneButton?.classList.remove('todo');
        dispatchButton?.classList.remove('todo');
        EVAUVButton?.classList.remove('todo');
      };
    }
    if (delayButton !== null) {
        delayButton.onclick = (): void => {
        delayButton?.classList.add('todo');

        todoButton?.classList.remove('todo');
        todoButton?.classList.remove('todo');
        doneButton?.classList.remove('todo');
        dispatchButton?.classList.remove('todo');
        EVAUVButton?.classList.remove('todo');
      };
    }
    if (dispatchButton !== null) {
        dispatchButton.onclick = (): void => {
        dispatchButton?.classList.add('todo');
        todoButton?.classList.remove('todo');
        delayButton?.classList.remove('todo');
        doneButton?.classList.remove('todo');
        delayDispatchButton?.classList.remove('todo');
        EVAUVButton?.classList.remove('todo');
      };
    }
    if (delayDispatchButton !== null) {
        delayDispatchButton.onclick = (): void => {
        delayDispatchButton?.classList.add('todo');

        todoButton?.classList.remove('todo');
        delayButton?.classList.remove('todo');
        doneButton?.classList.remove('todo');
        dispatchButton?.classList.remove('todo');
        EVAUVButton?.classList.remove('todo');
      };
    }
    if (doneButton !== null) {
        doneButton.onclick = (): void => {
        doneButton?.classList.add('todo');

        todoButton?.classList.remove('todo');
        delayButton?.classList.remove('todo');
        dispatchButton?.classList.remove('todo');
        delayDispatchButton?.classList.remove('todo');
        EVAUVButton?.classList.remove('todo');
      };
    }
    if (EVAUVButton !== null) {
        EVAUVButton.onclick = (): void => {
        EVAUVButton?.classList.add('todo');

        todoButton?.classList.remove('todo');
        delayButton?.classList.remove('todo');
        delayDispatchButton?.classList.remove('todo');
        doneButton?.classList.remove('todo');
        dispatchButton?.classList.remove('todo');
      };
    }
  }
  searchDone(args) {
    if (this.focusDone === this.DONE) {
      this.gridDone.search(this.glueName);
    } else if (this.focusDone === this.TODO) {
      this.gridTodo.search(this.glueName);
    } else if (this.focusDone === this.DELAY) {
      this.gridTodo.search(this.glueName);
    } else if (this.focusDone === this.DISPATCH) {
      this.gridDispatch.search(this.glueName);
    } else if (this.focusDone === this.DISPATCH_DELAY) {
      this.gridDispatch.search(this.glueName);
    } else if (this.focusDone === this.EVA_UV) {
      this.gridEVAUV.search(this.glueName);
    }
  }
  onClickToolbarTop(args: ClickEventArgs): void {
    // debugger;
    const target: HTMLElement = (args.originalEvent.target as HTMLElement).closest('button'); // find clicked button
    this.glueName = '';
    switch (target?.id) {
      case 'excelExport':
        this.spinner.show();
        this.todolistService.exportExcel(this.buildingID).subscribe((data: any) => {
          const blob = new Blob([data],
            { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });

          const downloadURL = window.URL.createObjectURL(data);
          const link = document.createElement('a');
          link.href = downloadURL;
          const ct = new Date();
          link.download = `${ct.getFullYear()}${ct.getMonth()}${ct.getDay()}_DoneList.xlsx`;
          link.click();
          this.spinner.hide();
        });
        break;
      case 'excelExport2':
        this.spinner.show();
        this.todolistService.exportExcel2(this.buildingID).subscribe((data: any) => {
          const blob = new Blob([data],
            { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
          const downloadURL = window.URL.createObjectURL(data);
          const link = document.createElement('a');
          link.href = downloadURL;
          const ct = new Date();
          link.download = `${ct.getFullYear()}${ct.getMonth()}${ct.getDay()}_DoneList(new).xlsx`;
          link.click();
          this.spinner.hide();
        });
        break;
      default:
        break;
    }
  }
  onClickToolbar(args: ClickEventArgs): void {
    // debugger;
    const target: HTMLElement = (args.originalEvent.target as HTMLElement).closest('button'); // find clicked button
    this.glueName = '';
    switch (target?.id) {
      case 'addition':
        this.openAddition();
        break;
      case 'done':
        this.isShowTab = this.DONE;
        this.focusDone = this.DONE;
        // this.glueName = '';
        this.done();
        this.router.navigate([
          `/ec/execution/todolist-2/${this.DONE}/${this.glueName}`,
        ]);
        // target.focus();
        break;
      case 'todo':
        this.isShowTab = this.TODO;
        this.focusDone = this.TODO;
        // this.glueName = '';
        this.todo();
        this.router.navigate([
          `/ec/execution/todolist-2/${this.TODO}/${this.glueName}`,
        ]);
        // target.focus();
        break;
      case 'delay':
        this.isShowTab = this.DELAY;
        this.focusDone = this.DELAY;
        // this.glueName = '';
        this.delay();
        this.router.navigate([
          `/ec/execution/todolist-2/${this.DELAY}/${this.glueName}`,
        ]);
        // target.focus();
        break;
      case 'delayDispatchList':
        this.isShowTab = this.DISPATCH_DELAY;
        this.focusDone = this.DISPATCH_DELAY;
        // this.glueName = '';
        this.dispatchListDelay();
        this.router.navigate([
          `/ec/execution/todolist-2/${this.DISPATCH_DELAY}/${this.glueName}`,
        ]);
        // target.focus();
        break;
      case 'dispatch':
        this.isShowTab = this.DISPATCH;
        this.focusDone = this.DISPATCH;
        // this.glueName = '';
        this.dispatchList();
        this.router.navigate([
          `/ec/execution/todolist-2/${this.DISPATCH}/${this.glueName}`,
        ]);
        // target.focus();
        break;
      case 'eva_uv':
        this.isShowTab = this.EVA_UV;
        this.focusDone = this.EVA_UV;
        // this.glueName = '';
        this.EVA_UVList();
        this.router.navigate([
          `/ec/execution/todolist-2/${this.EVA_UV}/${this.glueName}`,
        ]);
        // target.focus();
        break;
      case 'excelExport':
        this.spinner.show();
        this.todolistService.exportExcel(this.buildingID).subscribe((data: any) => {
          const blob = new Blob([data],
            { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });

          const downloadURL = window.URL.createObjectURL(data);
          const link = document.createElement('a');
          link.href = downloadURL;
          link.download = 'doneListReport.xlsx';
          link.click();
          this.spinner.hide();
        });
        break;
      case 'excelExport2':
        this.spinner.show();
        this.todolistService.exportExcel2(this.buildingID).subscribe((data: any) => {
          const blob = new Blob([data],
            { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
          const downloadURL = window.URL.createObjectURL(data);
          const link = document.createElement('a');
          link.href = downloadURL;
          link.download = 'report(new).xlsx';
          link.click();
          this.spinner.hide();
        });
        break;
      default:
        break;
    }
  }
  toolbarClick(args: any): void {
    switch (args.item.id) {
      case 'Done':
        this.isShowTab = this.DONE;
        this.done();
        break;
      case 'Undone':
        this.isShowTab = this.TODO;
        this.todo();
        break;
      case 'Delay':
        this.isShowTab = this.DELAY;
        this.todo();
        break;
      case 'Dispatch':
        this.isShowTab = this.DISPATCH;
        this.dispatchList();
        break;
      default:
        break;
    }
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
  rowDB(args: RowDataBoundEventArgs): void {
    const data = args.data as any;
    if (data.abnormalStatus === true) {
      args.row.classList.add('bgcolor');
    }
  }
  rowDBDispatch(args: RowDataBoundEventArgs): void {
    const data = args.data as any;
    const colorCode = `color-code-${data.colorCode}`;
    if (data.abnormalStatus === true) {
      const abnormalColorCode = `color-code-abnormal`;
      args.row.classList.add(abnormalColorCode);
    } else {
      args.row.classList.add(colorCode);
    }
  }
  rowDBDispatchDelay(args: RowDataBoundEventArgs): void {
    const data = args.data as any;
    const colorCode = `color-code-${data.colorCode}`;
    if (data.abnormalStatus === true) {
      const abnormalColorCode = `color-code-abnormal`;
      args.row.classList.add(abnormalColorCode);
    } else {
      args.row.classList.add(colorCode);
    }
  }
  onDoubleClickDone(args: any): void {
    if (args.column.field === 'deliveredAmount') {
      const value = args.rowData as IToDoList;
      this.openDispatchModalDoneList(value);
    }

    if (args.column.field === 'mixedConsumption') {
      const value = args.rowData as IToDoList;
      this.openMixHistory(value); // <!--Thêm bởi Quỳnh (Leo 2/2/2021 11:46)-->
    }
  }
  // Thêm bởi Quỳnh (Leo 2/2/2021 11:46)
  openMixHistory(value) {
    this.modalReference = this.modalService.open(this.historyMixed, { size: 'lg', backdrop: 'static', keyboard: false });
    this.todolistService.MixedHistory(value.mixingInfoID).subscribe((res: any) => {
      this.glueMix_name = value.glueName;
      this.total_amount = value.mixedConsumption;
      if (res.status) {
        this.dataHistoryMixed = res.result;
      }
    });
  }
  // End Thêm bởi Quỳnh (Leo 2/2/2021 11:46)
  actionBegin(args) {
    if (args.requestType === 'cancel') {
    }

    if (args.requestType === 'save') {
      if (args.action === 'edit') {
        const previousData = args.previousData;
        const data = args.data;
        if (data.quantity !== previousData.quantity) {
          const planId = args.data.id || 0;
          const quantity = args.data.quantity;
        } else { args.cancel = true; }
      }
    }
  }
  onBeforeRender(args, data, i) {
    const t = this.tooltip.filter((item, index) => index === +i)[0];
    t.content = 'Loading...';
    t.dataBind();
    this.planService
      .getBPFCByGlue(data.glueName)
      .subscribe((res: any) => {
        t.content = res.join('<br>');
        t.dataBind();
      });
  }

  // modal
  goToStir(data: IToDoList) {
    if (data.finishMixingTime === null) {
      this.alertify.warning('Hãy thực hiện bước trộn keo trước!', true);
      return;
    }
    this.router.navigate([`/ec/execution/todolist-2/stir/${this.isShowTab}/${data.mixingInfoID}/${data.glueName}`]);
  }
  goToShake(data: IToDoList) {
    // if (data.finishMixingTime === null) {
    //   this.alertify.warning('Hãy thực hiện bước trộn keo trước!', true);
    //   return;
    // }
    return this.router.navigate([`/ec/execution/todolist-2/shake/${this.isShowTab}/${data.mixingInfoID}`]);

  }
  goToMixing(data: IToDoList) {
    return [`/ec/execution/todolist-2/mixing/${this.isShowTab}/${data.glueID}/${data.estimatedStartTime}/${data.estimatedFinishTime}/${data.standardConsumption}`];
  }
  openDispatchModal(value: any) {
    if (value.printTime === null && value.glueName.includes(' + ')) {
      this.alertify.warning('Hãy thực hiện bước in keo trước!', true);
      return;
    }
    if (value.mixingInfoID === 0 && !value.glueName.includes(' + ')) { this.alertify.warning('Hãy thực hiện bước in keo trước!<br> Please perform glue printing first!', true); return; }
    const modalRef = this.modalService.open(DispatchComponent, { size: 'xl', backdrop: 'static', keyboard: false });
    modalRef.componentInstance.value = value;
    modalRef.result.then((result) => {
    }, (reason) => {
      this.todolistService.setValue(true);
    });
  }
  openDispatchModalDoneList(value: any) {
    if (value.printTime === null && value.glueName.includes(' + ')) {
      this.alertify.warning('Hãy thực hiện bước in keo trước!', true);
      return;
    }
    if (value.mixingInfoID === 0 && !value.glueName.includes(' + ')) { this.alertify.warning('Hãy thực hiện bước in keo trước!<br> Please perform glue printing first!', true); return; }
    const modalRef = this.modalService.open(DispatchDoneListComponent, { size: 'xl', backdrop: 'static', keyboard: false });
    modalRef.componentInstance.value = value;
    modalRef.result.then((result) => {
    }, (reason) => {
      this.todolistService.setValue(true);
    });
  }
  openPrintModal(value: IToDoList) {
    // if (value.finishStirTime === null && value.glueName.includes(' + ')) {
    //   this.alertify.warning('Hãy thực hiện bước khuấy keo trước!', true);
    //   return;
    // }
    this.todolistService.findPrintGlue(value.mixingInfoID).subscribe(data => {
      if (data?.id === 0 && value.glueName.includes(' + ')) {
        this.alertify.error('Please mixing this glue first!', true);
        return;
      }
      const modalRef = this.modalService.open(PrintGlueComponent, { size: 'md', backdrop: 'static', keyboard: false });
      modalRef.componentInstance.data = data;
      modalRef.componentInstance.value = value;
      modalRef.result.then((result) => {
      }, (reason) => {
      });
    });
  }
  openPrintModalDispatchList(value: IToDoList) {
    const modalRef = this.modalService.open(PrintGlueDispatchListComponent, { size: 'md', backdrop: 'static', keyboard: false });
    modalRef.componentInstance.value = value;
    modalRef.result.then((result) => {
    }, (reason) => {
    });
  }
  lockDispatch(data: any): string {
    const classList = ''; // loai bo khong dung nua nhe
    // if (data.deliveredAmount > 0) {
    //   classList = 'disabled cursor-pointer';
    // }
    return classList;
  }
  // config screen
  openFullscreen() {
    // Use this.divRef.nativeElement here to request fullscreen
    const elem = this.divRef.nativeElement;
    if (elem.requestFullscreen) {
      elem.requestFullscreen();
    } else if (elem.msRequestFullscreen) {
      elem.msRequestFullscreen();
    } else if (elem.mozRequestFullScreen) {
      elem.mozRequestFullScreen();
    } else if (elem.webkitRequestFullscreen) {
      elem.webkitRequestFullscreen();
    }
    this.hasCloseScreen = true;
    this.hasShowFullScreen = false;
  }
  closeFullscreen() {
    if (document.exitFullscreen) {
      document.exitFullscreen();
    } else if ((document as any).mozCancelFullScreen) {
      (document as any).mozCancelFullScreen();
    } else if ((document as any).webkitExitFullscreen) {
      (document as any).webkitExitFullscreen();
    } else if ((document as any).msExitFullscreen) {
      (window.top.document as any).msExitFullscreen();
    }
    this.hasCloseScreen = false;
    this.hasShowFullScreen = true;

  }
  reloadPage() {
    this.router.navigateByUrl('/TodolistComponent', { skipLocationChange: true }).then(() => {
      this.router.navigate(['/ec/execution/todolist-2/' + this.isShowTab]);
    });
    // window.location.reload();
  }
  // Start Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
  onChangeGlue(args) {
    this.AddGlueNameID = args.itemData.glueNameID,
      this.AddGlueID = args.itemData.glueID,
      this.AddEstimatedStartTime = args.itemData.estimatedStartTime,
      this.AddEstimatedFinishTime = args.itemData.estimatedFinishTime;
    this.startWorkingTime = new Date(args.itemData.estimatedStartTime);
    this.finishWorkingTime = new Date(args.itemData.estimatedFinishTime);

  }
  onChangeGlueDispatch(args) {
    this.glueNameIDDispatch = args.itemData.glueNameID;
  }
  actionBeginAddition(args) {
    if (args.requestType === 'save') {
      this.todolistService.addition(this.AddGlueNameID, this.buildingID, this.AddEstimatedStartTime, this.AddEstimatedFinishTime)
        .subscribe((res: any) => {
          if (res) {
            this.alertify.success(res.message);
            this.modalReference.dismiss();
            this.loadData();
          }
          else {
            this.alertify.error(res.message);
            args.cancel = true;
          }
        });
    }
  }
  actionBeginAdditionDispatch(args) {
    if (args.requestType === 'save') {
      this.todolistService.additionDispatch(this.glueNameIDDispatch)
        .subscribe(() => {
          this.alertify.success('Success!');
          this.modalReference.dismiss();
          this.dataAdditionDispatch = [];
          this.loadData();
        }, err => {
          this.alertify.error(err);
          args.cancel = true;
          this.dataAdditionDispatch = [];
        });
    }
  }
  public onFiltering: EmitType<FilteringEventArgs> = (
    e: FilteringEventArgs
  ) => {
    let query: Query = new Query();
    // frame the query based on search string with filter type.
    query =
      e.text !== '' ? query.where('name', 'contains', e.text, true) : query;
    // pass the filter data source, filter query to updateData method.
    // e.updateData(this.BPFCs, query);
  }
  public onFilteringGlueDispatch: EmitType<FilteringEventArgs> = (
    e: FilteringEventArgs
  ) => {
    let query: Query = new Query();
    // frame the query based on search string with filter type.
    query =
      e.text !== '' ? query.where('name', 'contains', e.text, true) : query;
    // pass the filter data source, filter query to updateData method.
    // e.updateData(this.BPFCs, query);
  }
  openAddition() {
    this.modalReference = this.modalService.open(this.addition, { size: 'lg', backdrop: 'static', keyboard: false });
  }
  openAdditionDispatch() {
    this.modalReference = this.modalService.open(this.additionDispatch, { size: 'lg', backdrop: 'static', keyboard: false });
  }
  // End Thêm bởi Quỳnh (Leo 1/28/2021 11:46)

  // Bebin STF
  openDispatchEVAUV(data) {
    const modalRef = this.modalService.open(DispatchEVAUVComponent, { size: 'lg', backdrop: 'static', keyboard: false });
    modalRef.componentInstance.data = data;
    modalRef.result.then((result) => {
    }, (reason) => {
    });
  }
  openSubpackage(data) {
    const modalRef = this.modalService.open(SubpackageComponent, { size: 'lg', backdrop: 'static', keyboard: false });
    modalRef.componentInstance.data = data;
    modalRef.result.then((result) => {
    }, (reason) => {
    });
  }
  doneSTF() {
    this.bottomFactoryService.done(this.buildingID).subscribe(res => {
      this.doneData = res.data;
      this.todoTotal = res.todoTotal;
      this.doneTotal = res.doneTotal;
      this.delayTotal = res.delayTotal;
      this.total = res.total;
      this.percentageOfDone = res.percentageOfDone;

      this.dispatchTotal = res.dispatchTotal;
      this.todoDispatchTotal = res.todoDispatchTotal;
      this.doneDispatchTotal = res.doneDispatchTotal;
      this.delayDispatchTotal = res.delayDispatchTotal;
      this.percentageOfDoneDispatch = res.percentageOfDoneDispatch;
    });
  }
  todoSTF() {
    this.bottomFactoryService.todo(this.buildingID).subscribe(res => {
      this.data = res.data;
      this.todoTotal = res.todoTotal;
      this.doneTotal = res.doneTotal;
      this.total = res.total;
      this.delayTotal = res.delayTotal;
      this.percentageOfDone = res.percentageOfDone;

      this.dispatchTotal = res.dispatchTotal;
      this.todoDispatchTotal = res.todoDispatchTotal;
      this.delayDispatchTotal = res.delayDispatchTotal;
      this.doneDispatchTotal = res.doneDispatchTotal;
      this.percentageOfDoneDispatch = res.percentageOfDoneDispatch;
    });
  }
  undoneSTF() {
    this.bottomFactoryService.delay(this.buildingID).subscribe(res => {
      this.data = res.data;
      this.todoTotal = res.todoTotal;
      this.doneTotal = res.doneTotal;
      this.delayTotal = res.delayTotal;
      this.total = res.total;
      this.percentageOfDone = res.percentageOfDone;

      this.dispatchTotal = res.dispatchTotal;
      this.todoDispatchTotal = res.todoDispatchTotal;
      this.delayDispatchTotal = res.delayDispatchTotal;

      this.doneDispatchTotal = res.doneDispatchTotal;
      this.percentageOfDoneDispatch = res.percentageOfDoneDispatch;
    });
  }
  dispatchListSTF() {
    this.bottomFactoryService.dispatchList(this.buildingID).subscribe((res: any) => {
      this.dispatchData = res.data;
      this.todoTotal = res.todoTotal;
      this.doneTotal = res.doneTotal;
      this.total = res.total;
      this.delayTotal = res.delayTotal;
      this.percentageOfDone = res.percentageOfDone;

      this.dispatchTotal = res.dispatchTotal;
      this.todoDispatchTotal = res.todoDispatchTotal;
      this.delayDispatchTotal = res.delayDispatchTotal;
      this.doneDispatchTotal = res.doneDispatchTotal;
      this.percentageOfDoneDispatch = res.percentageOfDoneDispatch;
    });
  }
  dispatchListDelaySTF() {
    this.bottomFactoryService.dispatchListDelay(this.buildingID).subscribe((res: any) => {
      this.dispatchData = res.data;
      this.todoTotal = res.todoTotal;
      this.doneTotal = res.doneTotal;
      this.total = res.total;
      this.delayTotal = res.delayTotal;
      this.percentageOfDone = res.percentageOfDone;

      this.dispatchTotal = res.dispatchTotal;
      this.todoDispatchTotal = res.todoDispatchTotal;
      this.delayDispatchTotal = res.delayDispatchTotal;
      this.doneDispatchTotal = res.doneDispatchTotal;
      this.percentageOfDoneDispatch = res.percentageOfDoneDispatch;
    });
  }
  // End STF
}
