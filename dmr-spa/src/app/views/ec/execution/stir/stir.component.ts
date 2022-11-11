import { Component, OnInit, ViewEncapsulation, ViewChild, OnDestroy } from '@angular/core';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { IngredientService } from 'src/app/_core/_service/ingredient.service';
import { DatePipe } from '@angular/common';
import { SettingService } from 'src/app/_core/_service/setting.service';
import { StirService } from 'src/app/_core/_service/stir.service';
import { GridComponent } from '@syncfusion/ej2-angular-grids';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { TodolistService } from 'src/app/_core/_service/todolist.service';
import { IStir, IStirForAdd, IStirForUpdate } from 'src/app/_core/_model/stir';
import { of, Subject, Subscription } from 'rxjs';
import { IScanner } from 'src/app/_core/_model/IToDoList';
import { debounceTime, delay, tap } from 'rxjs/operators';

declare const $: any;

@Component({
  selector: 'app-stir',
  templateUrl: './stir.component.html',
  styleUrls: ['./stir.component.css'],
  encapsulation: ViewEncapsulation.None,
  providers: [
    DatePipe
  ]
})
export class StirComponent implements OnInit, OnDestroy {
  public ADMIN = 1;
  public SUPERVISOR = 2;
  STIRRED = 1;
  NOT_STIRRED_YET = 0;
  NA = 2;
  modalReference: NgbModalRef;
  public filterSettings = { type: 'Excel' };
  pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
  @ViewChild('grid') public grid: GridComponent;
  toolbarOptions = ['ExcelExport', 'Search'];
  public value: any = '';
  public dateValue: any = '';
  public Envalue: any = '';
  public interval = 1;
  public customFormat = 'HH:mm:ss a';
  public ingredients: any = [];
  public building: any;
  public role: any;
  qrCode: string;
  timeStir = 0;
  glueID: number;
  settingID: number;
  settingData = [];
  glues: any;
  glueName: any;
  rpm = 0;
  minutes = 0;
  totalMinutes = 0;
  status: boolean;
  stir: any;
  finishStiringTime: any;
  startStiringTime: any;
  mixingInfoID: number;
  stirData: IStir[] = [];
  standardRPM = 0;
  duration = 0;
  startScanTime = null;
  remainingTime: Date;
  leftTime: any;
  subject = new Subject<IScanner>();
  subscription: Subscription[] = [];
  tab: any;
  buildingID: number;
  private timer: Subscription;
  constructor(
    private route: ActivatedRoute,
    public modalService: NgbModal,
    private alertify: AlertifyService,
    private datePipe: DatePipe,
    public ingredientService: IngredientService,
    public settingService: SettingService,
    public stirService: StirService,
    private router: Router,
    public todolistService: TodolistService,
    private translate: TranslateService
  ) { }
  ngOnDestroy(): void {
    this.subscription.forEach(item => item.unsubscribe());
  }
  public ngOnInit(): void {
    this.building = JSON.parse(localStorage.getItem('building'));
    this.buildingID = +JSON.parse(localStorage.getItem('buildingID'));
    this.role = JSON.parse(localStorage.getItem('level'));
    this.startStiringTime = new Date();
    this.getAllSetting();
    this.onRouteChange();
    this.checkQRCode();

  }
  private checkQRCode() {
    this.subscription.push(this.subject
      .pipe(debounceTime(500)).subscribe(async (res) => {
        try {
          const check = this.stirData.every(x => x.actualDuration > 0);
          // if (check === true) {
          //   this.alertify.warning('The glue has been stired <br> Keo này đã được khuấy', true);
          //   return;
          // }
          const qrCode = res.QRCode;
          const setting = await this.scanQrCode(qrCode) as any;
          this.qrCode = qrCode;
          const stirModel: IStirForAdd = {
            id: 0,
            glueName: this.glueName,
            settingID: setting.id,
            mixingInfoID: this.mixingInfoID,
            buildingID: this.buildingID,
            startScanTime: this.datePipe.transform(new Date(), 'yyyy-MM-dd HH:mm:ss'),
            startStiringTime: this.datePipe.transform(new Date(), 'yyyy-MM-dd HH:mm:ss'),
          };
          if (this.stirData.length === 0) {
            const create = await this.create(stirModel);
          } else {
            const status = await this.updateStartScanTime(this.mixingInfoID);
          }
          const data = await this.getStirByMixingInfoID(this.mixingInfoID);
          this.stirData = data;
          this.getStandardRPMAndDuration();
        } catch (error) {
          this.alertify.error('QR Code invalid!', true);
        }
      }));
  }
  onRouteChange() {
    this.route.data.subscribe(data => {
      this.glueName = this.route.snapshot.params.glueName;
      this.mixingInfoID = this.route.snapshot.params.mixingInfoID;
      this.tab = this.route.snapshot.params.tab;
      this.loadData();
    });
  }
  toolbarClick(args): void {
    switch (args.item.text) {
      /* tslint:disable */
      case 'ExcelExport' || 'Xuất Excel':
        this.grid.excelExport();
        break;
      /* tslint:enable */
      case 'PDF Export':
        break;
    }
  }
  getStirInfo(glueName): Promise<any> {
    return this.stirService.getStirInfo(glueName).toPromise();
  }
  getRPM(stirID): Promise<any> {
    return this.stirService.getRPM(stirID).toPromise();
  }
  async loadStir() {
    try {
      const result = await this.getStirInfo(this.glueName);
      const res = result.map((item: any) => {
        return {
          id: item.id,
          stirID: item.stirID,
          glueName: item.glueName,
          // tslint:disable-next-line:max-line-length
          qty: item.qty,
          createdTime: item.createdTime,
          mixingStatus: item.mixingStatus,
          startTime: item.startTime,
          endTime: item.endTime,
          settingID: item.settingID,
          status: item.status,
          totalMinutes: item.totalMinutes,
          rpm: item.rpm,
          setting: item.setting,
          glueType: item.glueType,
          machineType: item.machineType
        };
      });
      this.glues = res;
    } catch (error) {
      this.alertify.error(error + '');
    }
  }
  async loadRPM(stirID) {
    try {
      const obj = await this.getRPM(stirID);
      if (obj.rpm === 0) {
        this.alertify.warning('Không tìm thấy rpm trong khoản thời gian này!', true);
        this.modalReference.close();
        return;
      }
      if (this.stir.stirID > 0 && this.stir.rpm > 0) {
        this.rpm = this.stir.rpm;
        this.minutes = this.stir.totalMinutes;
        this.totalMinutes = this.stir.totalMinutes;
      } else {
        this.rpm = obj.rpm;
        this.minutes = obj.minutes;
        this.totalMinutes = obj.totalMinutes;
      }
      this.status = this.rpm >= this.stir.setting.minRPM && this.rpm <= this.stir.setting.maxRPM ? true : false;
    } catch (error) {
      this.alertify.error(error + '');
    }
  }

  updateStir() {
    const model = {
      id: this.stir.stirID,
      rpm: this.rpm,
      totalMinutes: this.totalMinutes,
      status: this.status,
      glueName: this.stir.glueName,
      settingID: this.stir.settingID,
      mixingInfoID: this.stir.mixingInfoID,
      buildingID: this.buildingID,
      startTime: this.stir.startTime,
      endTime: this.stir.endTime
    };
    this.settingService.updateStir(model).subscribe((res) => {
      this.alertify.success('Success');
      this.modalReference.close();
      this.loadStir();
    });
  }

  getAllSetting() {
    const item = {
      building: null,
      buildingID: this.buildingID,
      id: 0,
      machineCode: '',
      machineType: 'X',
      maxRPM: 350,
      minRPM: 250,
      name: null
    };
    this.settingService.getSettingByBuilding(this.buildingID).subscribe((res: []) => {
      this.settingData = res;
      this.settingData.push(item);
    });
  }
  NO(index) {
    return (this.grid.pageSettings.currentPage - 1) * this.grid.pageSettings.pageSize + Number(index) + 1;
  }
  // --------------------------------------------------------------------------------------
  async onNgModelChangeScanQRCode(args) {
    const scanner: IScanner = {
      QRCode: args,
      ingredient: null
    };
    this.subject.next(scanner);
  }
  async onNgModelChangeScanQRCode2(qrCode) {
    if (qrCode.length !== 3) {
      this.alertify.warning('The QR Code is incorrect format <br> Mã QR sai định dạng của hệ thống!', true);
      return;
    }
    try {
      const check = this.stirData.every(x => x.actualDuration > 0);
      // if (check === true) {
      //   this.alertify.warning('The glue has been stired <br> Keo này đã được khuấy', true);
      //   return;
      // }
      const setting = await this.scanQrCode(qrCode) as any;
      this.qrCode = qrCode;
      const stirModel: IStirForAdd = {
        id: 0,
        glueName: this.glueName,
        settingID: setting.id,
        buildingID: this.buildingID,
        mixingInfoID: this.mixingInfoID,
        startScanTime: this.datePipe.transform(new Date(), 'yyyy-MM-dd HH:mm:ss'),
        startStiringTime: this.datePipe.transform(new Date(), 'yyyy-MM-dd HH:mm:ss'),
      };
      if (this.stirData.length === 0) {
        const create = await this.create(stirModel);
      } else {
        const status = await this.updateStartScanTime(this.mixingInfoID);
      }
      const data = await this.getStirByMixingInfoID(this.mixingInfoID);
      this.stirData = data;
      this.getStandardRPMAndDuration();
    } catch (error) {
      this.alertify.error('QR Code invalid!', true);
    }
  }
  async confirmData(item) {

    if (item.startScanTime as any === '0001-01-01T00:00:00') {
      this.alertify.error('Please scan the machine QR Code first! <br> Hãy quét mã QR của máy trước!', true);
      return;
    } else {
      const currentTime = new Date().getTime();
      const second = item.standardDuration > 0 ? item.standardDuration : item.glueType.minutes * 60;
      const startScanTime = new Date(item.startScanTime as any);
      const endStart = startScanTime.setSeconds(startScanTime.getSeconds() + second);
      if (currentTime < endStart) {
        this.alertify.error('The glue is stiring! <br> Máy đang khuấy keo bạn ơi!', true);
        return;
      }
    }
    try {
      const stirModel: IStirForUpdate = {
        id: item.id,
        glueName: item.glueName,
        settingID: item.settingID,
        mixingInfoID: this.mixingInfoID,
        buildingID: this.buildingID,
        glueType: item.glueType,
        startScanTime: this.datePipe.transform(new Date(), 'yyyy-MM-dd HH:mm:ss'),
        finishStiringTime: this.datePipe.transform(new Date(), 'yyyy-MM-dd HH:mm:ss'),
      };
      const update = await this.update(stirModel as IStirForUpdate) as IStir;
      const data = await this.getStirByMixingInfoID(this.mixingInfoID);
      this.stirData = data;
      this.getStandardRPMAndDuration();
      if (update.status === true) {
        this.goToTodolist();
      }
    } catch (error) {
      this.alertify.error(error.toString());
    }
  }

  async loadData() {
    try {
      const data = await this.getStirByMixingInfoID(this.mixingInfoID);
      this.stirData = data;
      this.getStandardRPMAndDuration();
    } catch (error) {
    }
  }
  getStandardRPMAndDuration() {
    if (this.stirData.length > 0) {
      this.standardRPM = this.stirData[0].glueType.rpm;
      this.duration = this.stirData[0].glueType.minutes;
      const newStir = this.stirData.filter(item => item.actualDuration === 0)[0];
      if (newStir !== undefined) {
        this.startScanTime = newStir?.startScanTime as any === '0001-01-01T00:00:00' ? null : newStir?.startScanTime;
        console.log('start scan time', this.startScanTime);
        // const currentTime = new Date();
        // this.remainingTime = new Date(this.startScanTime.setMinutes(newStir?.glueType.minutes));
        // this.leftTime = this.remainingTime.getMinutes() - currentTime.getMinutes();
      }
    } else {
      this.standardRPM = 0;
      this.duration = 0;
    }
  }
  // Action
  updateStartScanTime(mixingInfoID: number) {
    return this.stirService.updateStartScanTime(mixingInfoID).toPromise();
  }
  scanQrCode(qrCode) {
    const buildingID = this.buildingID;
    return this.stirService.scanMachine(buildingID, qrCode).toPromise();
  }
  create(model: IStirForAdd) {
    return this.stirService.create(model).toPromise();
  }
  update(model: IStirForUpdate) {
    return this.stirService.update(model).toPromise();
  }
  getStirByMixingInfoID(mixingInfoID: number) {
    return this.stirService.getStirByMixingInfoID(mixingInfoID).toPromise();
  }
  //
  goToTodolist() {
    return this.router.navigate([`/ec/execution/todolist-2/${this.tab}/${this.glueName}`]);
  }
}
