import { DatePipe } from '@angular/common';
import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { DisplayTextModel } from '@syncfusion/ej2-angular-barcode-generator';
import { Subject, Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { IBuilding } from 'src/app/_core/_model/building';
import { IMixingInfo, IMixingInfoDetails } from 'src/app/_core/_model/IMixingInfo';
import { IScanner, IToDoList } from 'src/app/_core/_model/IToDoList';
import { IRole } from 'src/app/_core/_model/role';
import { AbnormalService } from 'src/app/_core/_service/abnormal.service';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { IngredientService } from 'src/app/_core/_service/ingredient.service';
import { MakeGlueService } from 'src/app/_core/_service/make-glue.service';
import { TodolistService } from 'src/app/_core/_service/todolist.service';

@Component({
  selector: 'app-print-glue',
  templateUrl: './print-glue.component.html',
  styleUrls: ['./print-glue.component.css']
})
export class PrintGlueComponent implements OnInit, OnDestroy {
  @Input() data: IMixingInfo;
  @Input() value: IToDoList;
  subject = new Subject<IScanner>();
  subscription: Subscription[] = [];
  public displayTextMethod: DisplayTextModel = {
    visibility: false
  };
  qrCode: string;
  role: IRole;
  user: any;
  building: IBuilding;
  isShow: boolean;
  isShowQRCode: boolean;
  chemicalA: IMixingInfoDetails;
  constructor(
    public activeModal: NgbActiveModal,
    public todolistService: TodolistService,
    public alertify: AlertifyService,
    private datePipe: DatePipe,
    private ingredientService: IngredientService,
    private abnormalService: AbnormalService,
    private makeGlueService: MakeGlueService

  ) { }

  ngOnInit() {
    const BUIDLING: IBuilding = JSON.parse(localStorage.getItem('building'));
    const ROLE: IRole = JSON.parse(localStorage.getItem('level'));
    const USER = JSON.parse(localStorage.getItem('user')).user;
    this.role = ROLE;
    this.user = USER;
    this.building = BUIDLING;
    this.checkQRCode();
    this.isShow = this.value.mixingInfoID === 0 && !this.value.glueName.includes(' + ');
    this.isShowQRCode = this.value.mixingInfoID === 0 && !this.value.glueName.includes(' + ');
    this.chemicalA = this.data?.mixingInfoDetails.filter(x => x.position === 'A')[0];
  }
  ngOnDestroy(): void {
    this.subscription.forEach(item => item.unsubscribe());
  }
  checkQRCode() {
    this.subscription.push(this.subject
      .pipe(debounceTime(500)).subscribe(async (res) => {
        const valid = await this.validateQRCode(res);
        if (valid.status === false) { return; }

        const input = res.QRCode.split('    ') || [];
        const mixing = {
          glueName: this.value.glueName,
          glueID: this.value.glueID,
          buildingID: this.building.id,
          mixBy: JSON.parse(localStorage.getItem('user')).user.id,
          estimatedStartTime: this.value.estimatedStartTime,
          estimatedFinishTime: this.value.estimatedFinishTime,
          details: [{
            amount: this.value.standardConsumption,
            ingredientID: valid.ingredient.id,
            batch: input[4].split(":")[1].trim() + ':' + input[0].split(":")[1].trim(),
            mixingInfoID: 0,
            position: 'A'
          }]
        };
        this.makeGlueService.add(mixing).subscribe((item: IMixingInfo) => {
          this.alertify.success('Success!');
          this.isShow = false;
          this.data = item;
        }, error => this.alertify.error(error));
      }));
    }
  async validateQRCode(args: IScanner): Promise<{ status: boolean; ingredient: any; }> {
    const input = args.QRCode.split('    ') || [];
    const qrcode = input[2].split(":")[1].trim() + ':' + input[0].split(":")[1].trim();
    this.qrCode = qrcode;
    const result = await this.scanQRCode();
    if (result === null) {
      this.alertify.warning('The QR Code is invalid!<br> Mã QR không hợp lệ! Vui lòng thử lại mã khác.', true);
      return {
        status: false,
        ingredient: null
      };
    }
    if (result.name !== this.value.glueName) {
      this.alertify.warning(`Please you should look for the chemical name "${this.value.glueName}"<br> Vui lòng quét đúng hóa chất "${this.value.glueName}"!`, true);
      this.qrCode = '';
      return {
        status: false,
        ingredient: null
      };
    }
    const checkLock = await this.hasLock(result.name, input[4].split(":")[1].trim() + ':' + input[0].split(":")[1].trim());
    if (checkLock === true) {
      this.alertify.error('This chemical has been locked! <br> Hóa chất này đã bị khóa!', true);
      this.qrCode = '';
      return {
        status: false,
        ingredient: null
      };
    }
    return {
      status: true,
      ingredient: result
    };
  }
  scanQRCode(): Promise<any> {
    return this.ingredientService.scanQRCode(this.qrCode).toPromise();
  }
  hasLock(ingredient, batch): Promise<any> {
    let buildingName = this.building.name;
    if (this.role.id === 1 || this.role.id === 2) {
      buildingName = 'E';
    }
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
  async onNgModelChangeScanQRCode(args) {
    const scanner: IScanner = {
      QRCode: args,
      ingredient: null
    };
    this.subject.next(scanner);
  }
  printData() {
    // <li class='subInfo' style='font-size: 18px ;'>${this.data.code}</li>
    // <li class='subInfo' style='font-size: 15px ;'> Batch: ${this.chemicalA?.batch === undefined ? 'N/A' : this.chemicalA?.batch}</li>
    let html = '';
    const printContent = document.getElementById('qrcode');

    // tslint:disable-next-line:max-line-length
    const exp = (this.data.expiredTime as any) === '0001-01-01T00:00:00' ? 'N/A' : this.datePipe.transform(new Date(this.data.expiredTime), 'MMdd HH:mm');
    html += `
       <div class='content'>
        <div class='qrcode'>
         ${printContent.innerHTML}
         </div>
          <div class='info'>
          <ul>
              <li class='subInfoTo'>${this.data.glueName}</li>
              <li class='subInfoNho' >MFG: ${this.datePipe.transform(new Date(this.data.createdTime), 'MMdd HH:mm')}</li>
              <li class='subInfoNho' >EXP: ${exp}</li>
          </ul>
         </div>
      </div>
      `;
    this.configurePrint(html);
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
          width: 110px;
          margin-top: 10px;
          padding: 0;
          margin-left: 0px;
        }

        .content .info {
          float:left;
          list-style: none;
          width: 190px;
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
        .content .info ul .subInfoTo  {
          font-size: 22px;
          font-weight: bold;
          width: 180px;
          word-wrap: break-word;
        }
        .content .info ul .subInfoNho  {
          font-size: 15px;
          width: 180px;
          word-wrap: break-word;
        }

        @page {
          size: 2.65 x 1.20 inch;
          page-break-after: always;
          margin: 0;
        }
        @media print {
          html, body {
            width: 90mm;
          }
        }
      </style>
      <body onload="window.print(); window.close()">
        ${html}
      </body>
    </html>
    `);
    WindowPrt.document.close();
    this.finish();
  }
  finish() {
    this.todolistService.printGlue(this.data.id).subscribe((data: any) => {
      this.alertify.success('success', true);
      this.activeModal.close();
      this.todolistService.setValue(false);
    });
  }
}
