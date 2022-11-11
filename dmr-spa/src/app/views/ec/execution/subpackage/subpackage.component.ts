import { DatePipe } from '@angular/common';
import { Component, ElementRef, Input, OnDestroy, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { DisplayTextModel, QRCodeGeneratorAllModule, QRCodeGeneratorComponent } from '@syncfusion/ej2-angular-barcode-generator';
import { QRCodeComponent } from 'angularx-qrcode';
import { Subject, Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { IScanner } from 'src/app/_core/_model/IToDoList';
import { ResponseDetail } from 'src/app/_core/_model/responseDetail';
import { IGenerateSubpackageParams, IScanParams } from 'src/app/_core/_model/scan-params';
import { AbnormalService } from 'src/app/_core/_service/abnormal.service';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { BottomFactoryService } from 'src/app/_core/_service/bottom.factory.service';
import { TodolistService } from 'src/app/_core/_service/todolist.service';

@Component({
  selector: 'app-subpackage',
  templateUrl: './subpackage.component.html',
  styleUrls: ['./subpackage.component.scss']
})
export class SubpackageComponent implements OnInit, OnDestroy {
  qrcode: QRCodeGeneratorComponent;
  @Input() data: any;
  subpackages: any = [];
  subscription: Subscription[] = [];
  subject = new Subject<IScanner>();
  scanParams: IScanParams;
  generateSubpackageParams: IGenerateSubpackageParams;
  qrCode: string;
  status: boolean;
  partNO: any;
  chemical: any;
  can = '';
  public displayTextMethod: DisplayTextModel = {
    visibility: false
  };
  statusText: string;
  mixingInfoID: number;
  mixingInfoCode: any;
  subpackageCapacity: any;
  batch: string;
  subpackageLatestItem: any;
  focusNumberOfScan: boolean;
  constructor(
    public activeModal: NgbActiveModal,
    public bottomFactoryService: BottomFactoryService,
    public abnormalService: AbnormalService,
    public alertify: AlertifyService,
    public datePipe: DatePipe,
    private todolistService: TodolistService) { }
  ngOnDestroy(): void {
    this.subscription.forEach(item => item.unsubscribe());
  }

  ngOnInit() {
    console.log('oninit', this.data);
    this.focusNumberOfScan = false;
    this.status = null;
    this.statusText = '';
    this.checkQRCodeV110();
  }
  onkeypress(args) {
    if (this.can.match(/^-?(0|[1-9]\d*)?$/) === null) {
      this.alertify.warning('Chỉ được nhập số! <br> Only key in number!', true);
      return;
    }
    this.generateSubpackageParams = {
      amountOfChemical: this.chemical.unit,
      buildingID: this.data.buildingID,
      mixingInfoID: this.mixingInfoID,
      can: +this.can,
      glueName: this.data.glueName,
      glueNameID: this.data.glueNameID,
    };
    console.log('onkeypress', this.generateSubpackageParams);
    this.generateScanByNumber();
  }
  onkeypressV110(args) {
    if (this.can.match(/^-?(0|[1-9]\d*)?$/) === null) {
      this.alertify.warning('Chỉ được nhập số! <br> Only key in number!', true);
      return;
    }
    this.subpackages = [];
    let position = this.subpackageLatestItem == null ? 0 : this.subpackageLatestItem.position;
    for (let i = 0; i <= +this.can - 1; i++) {
      position = position + 1;
      const subpackage =
      {
        code: `${this.mixingInfoCode}_${this.batch}_${position}`,
        glueName: this.data.glueName,
        name: `${this.data.glueName}_${position}`,
        glueNameID: this.data.glueNameID,
        position,
        mixingInfoID: 0,
        expiredTime: new Date(new Date().setHours(this.chemical.expiredTime)),
        createdBy: JSON.parse(localStorage.getItem('user')).user.id,
        createdTime: new Date(),
        amount: this.subpackageCapacity.capacity
      };
      this.subpackages.push(subpackage);
    }
  }
  checkStatus() {
    if (this.status === null) { this.statusText = ''; return ''; }
    else if (this.status === true) { this.statusText = 'OK'; return 'success-scan'; }
    else if (this.status === false) { this.statusText = 'NO'; return 'warning-scan'; }
  }
  checkQRCode() {
    this.subscription.push(this.subject
      .pipe(debounceTime(500)).subscribe(async (input) => {
        const valid = await this.validateQRCode(input);

        if (valid.status === false) { return; }
        this.chemical = valid.ingredient;
        this.subpackages = valid.subpackages;
        this.mixingInfoID = valid.mixingInfoID;
        if (this.subpackages?.length >= 0) {
          this.can = this.subpackages?.length;
        }
      }));
  }
  checkQRCodeV110() {
    this.subscription.push(this.subject
      .pipe(debounceTime(500)).subscribe(async (input) => {
        const valid = await this.validateQRCode(input);
        if (valid.status === false) { return; }
        this.chemical = valid.ingredient;
        this.mixingInfoCode = valid.code;
        this.batch = valid.batch;
        this.focusNumberOfScan = true;
        this.getInfo();
      }));
  }
  async validateQRCode(args: IScanner): Promise<{
    status: boolean; ingredient: any; subpackages: []
      , mixingInfoID: number,
      code: string, batch: string
  }> {
    const input = args.QRCode.split('    ') || [];
    const qrcode = input[2].split(":")[1].trim() + ':' + input[0].split(":")[1].trim();
    const partNO = qrcode;
    this.partNO = partNO;
    this.scanParams = {
      partNO,
      glueID: this.data.glueID,
      glueName: this.data.glueName,
      batchNO: input[4].split(":")[1].trim() + ':' + input[0].split(":")[1].trim(),
      glueNameID: this.data.glueNameID,
      mixingInfoID: this.data.mixingInfoID,
      buildingID: this.data.buildingID,
      estimatedStartTime: this.data.estimatedStartTime,
      estimatedFinishTime: this.data.estimatedFinishTime,
    };
    const result = await this.scanQRCode();
    this.status = result.status;
    this.mixingInfoCode = result.code;
    this.batch = result.batch;
    this.mixingInfoID = result.data.mixingInfoID;
    console.log('scannQRcode', result);
    const checkLock = await this.hasLock(result.name, input[4].split(":")[1].trim() + ':' + input[0].split(":")[1].trim());
    if (result === null) {
      this.alertify.warning('The QR Code is invalid!<br> Mã QR không hợp lệ! Vui lòng thử lại mã khác.', true);
      this.qrCode = '';
      const res = {} as {
        status: boolean; ingredient: any; subpackages: []
        , mixingInfoID: number,
        code: string, batch: string
      };
      res.status = false;
      return res;
    }
    if (checkLock === true) {
      this.alertify.error('This chemical has been locked! <br> Hóa chất này đã bị khóa!', true);
      this.qrCode = '';
      const res = {} as {
        status: boolean; ingredient: any; subpackages: []
        , mixingInfoID: number,
        code: string, batch: string
      };
      res.status = false;
      return res;
    }
    return {
      status: true,
      ingredient: result.data.ingredient,
      code: result.data.code,
      batch: result.data.batch,
      subpackages: result.data.subpackages,
      mixingInfoID: result.data.mixingInfoID
    };
  }
  getInfo() {
    this.bottomFactoryService.getSubpackageCapacity().subscribe(res => {
      this.subpackageCapacity = res;
    });

    this.bottomFactoryService.getSubpackageLatestSequence(this.batch, this.data.glueNameID, this.data.buildingID).subscribe(res => {
      this.subpackageLatestItem = res;
    });
  }
  hasLock(ingredient, batch): Promise<any> {
    const buildingName = this.data.buildingName;
    return new Promise((resolve, reject) => {
      this.abnormalService.hasLock(ingredient, buildingName, batch).subscribe(
        (res) => {
          resolve(res);
        },
        (err) => {
          reject(false);
        }
      );
    });
  }
  async scanQRCode(): Promise<any> {
    return new Promise((resolve, reject) => {
      this.bottomFactoryService.scanQRCode(this.scanParams).subscribe((res) => { resolve(res); }
        , err => {
          reject(err);
        });
    });
  }
  generateScanByNumber() {
    return new Promise((resolve, reject) => {
      this.bottomFactoryService.generateScanByNumber(this.generateSubpackageParams)
        .subscribe((res: any) => {
          this.subpackages = res.data;
          this.focusNumberOfScan = false;
        }
          , err => {
            this.alertify.error(err);
          });
    });
  }
  async onNgModelChangeScanQRCode(args) {
    const scanner: IScanner = {
      QRCode: args,
      ingredient: null
    };
    this.subject.next(scanner);
  }

  async printData() {
    if (this.subpackages === undefined || this.subpackages.length === 0 || this.subpackages === null) {
      this.alertify.warning('Vui lòng quét mã QR và nhập số lượng can cần chia!', true);
      return;
    }
    let html = '';
    for (const item of this.subpackages) {
      const content = document.getElementById(item.code);
      const exp = (item.expiredTime as any) === '0001-01-01T00:00:00' ?
        'N/A' : this.datePipe.transform(new Date(item.expiredTime), 'YYYYMMdd HH:mm');
      html += `
       <div class='content'>
        <div class='qrcode'>
         ${content.innerHTML}
         </div>
          <div class='info'>
          <ul>
          <li class='subInfo'>Name: ${item.name}</li>
          <li class='subInfo'>Code: ${item.code}</li>
          <li class='subInfoNho' >MFG: ${this.datePipe.transform(new Date(item.createdTime), 'YYYYMMdd HH:mm')}</li>
          <li class='subInfoNho' >EXP: ${exp}</li>
          </ul>
         </div>
      </div>
      `;
    }
    this.configurePrint(html);
    const model = {
      subpackages: this.subpackages,
      ingredient: this.chemical,
      partNO: this.partNO,
      glueID: this.data.glueID,
      glueName: this.data.glueName,
      batchNO: 'DEFAULT',
      glueNameID: this.data.glueNameID,
      mixingInfoID: this.data.mixingInfoID,
      mixingInfoCode: this.mixingInfoCode,
      buildingID: this.data.buildingID,
      estimatedStartTime: this.data.estimatedStartTime,
      estimatedFinishTime: this.data.estimatedFinishTime,
    };
    this.bottomFactoryService.saveSubpackage(model).subscribe(() => {
      this.todolistService.setValue(true);
      this.activeModal.close();
    });
  }
  configurePrint(html) {
    const WindowPrt = window.open('', '_blank', 'left=0,top=0,width=1000,height=900,toolbar=0,scrollbars=0,status=0');
    WindowPrt.document.write(`
    <html>
      <head>
      </head>
      <style>
        * {
          box-sizing: border-box;
          -moz-box-sizing: border-box;
        }

        .content {
          page-break-after: always;
          clear: both;
        }

        .content .qrcode {
          float:left;
          width: 100px;
          margin-top: 10px;
          padding: 0;
          margin-left: 0px;
        }

        .content .info {
          float:left;
          list-style: none;
          width: 200px;
        }
        .content .info ul {
          float:left;
          list-style: none;
          padding: 0px;
          margin: 0px;
          margin-top: 20px;
          font-weight: bold;
          word-wrap: break-word;
        }

        @page {
          size: 2.65 1.20 in;
          page-break-after: always;
          margin: 0;
        }
        @media print {
          html, body {
            width: 90mm; // Chi co nhan millimeter
          }
        }
      </style>
      <body onload="window.print(); window.close()">
        ${html}
      </body>
    </html>
    `);
    WindowPrt.document.close();
  }
}
