import { async } from '@angular/core/testing';
import { Component, OnInit, AfterViewInit, ViewChild, Renderer2, ElementRef, QueryList, HostListener, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { DisplayTextModel } from '@syncfusion/ej2-angular-barcode-generator';
import { IngredientService } from 'src/app/_core/_service/ingredient.service';
import { DatePipe } from '@angular/common';
import { GridComponent } from '@syncfusion/ej2-angular-grids';
import { Subject, Subscription } from 'rxjs';
import { IScanner } from 'src/app/_core/_model/IToDoList';
import { debounceTime } from 'rxjs/operators';
import { IBuilding } from 'src/app/_core/_model/building';

import { DropDownListComponent, FilteringEventArgs } from '@syncfusion/ej2-angular-dropdowns';
import { Query } from '@syncfusion/ej2-data/';
import { EmitType } from '@syncfusion/ej2-base';
import { BuildingService } from 'src/app/_core/_service/building.service';
import { NgxSpinnerService } from 'ngx-spinner';
const BUILDING_LEVEL = 2;
@Component({
  selector: 'app-incoming',
  templateUrl: './incoming.component.html',
  styleUrls: ['./incoming.component.css'],
  providers: [
    DatePipe
  ]
})
export class IncomingComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('ddlelement')
  public dropDownListObject: DropDownListComponent;
  @ViewChild('scanQRCode') scanQRCodeElement: ElementRef;
  public displayTextMethod: DisplayTextModel = {
    visibility: false
  };
  // public filterSettings: object;
  pageSettings = { pageCount: 20, pageSizes: true, pageSize: 20 };
  @ViewChild('grid') public grid: GridComponent;
  // toolbarOptions: string[];
  @ViewChild('scanText', { static: false }) scanText: ElementRef;
  @ViewChild('ingredientinfoGrid') ingredientinfoGrid: GridComponent;
  qrcodeChange: any;
  data: [];
  dataOut: [];
  checkout = false;
  checkin = true;
  public ingredients: any = [];
  test: any = 'form-control w3-light-grey';
  checkCode: boolean;
  autofocus = false;
  toolbarOptions = ['Search'];
  filterSettings = { type: 'Excel' };
  subject = new Subject<IScanner>();
  subscription: Subscription[] = [];
  subjectSpinner = new Subject<boolean>();

  buildings: IBuilding[];
  fieldsBuildings: object = { text: 'name', value: 'id' };
  buildingID = 0;
  buildingName = '';
  toggleColor = true;
  isShow: boolean;
  constructor(
    public modalService: NgbModal,
    private alertify: AlertifyService,
    private datePipe: DatePipe,
    private spinner: NgxSpinnerService,
    private buildingService: BuildingService,
    public ingredientService: IngredientService,
    private cdr: ChangeDetectorRef
  ) {
  }
 receiveMessage(isShow) {
    const newEvent = isShow;
    if (newEvent !== this.isShow) {
    if (isShow === true) {
      this.spinner.show();
      console.log('this.isShow === true', isShow, new Date().toISOString());
      this.isShow = true;

    } else if (isShow === false) {
      console.log('this.isShow === false', isShow);
      this.isShow = false;
      this.spinner.hide();
      }
    }
    // const newEvent = isShow;
    // if (newEvent !== this.isShow) {

    //   if (newEvent === true) {

    //     this.subjectSpinner.next(true);
    //   } else if (newEvent === false){
    //     console.log('this.isShow === false', isShow);

    //     this.subjectSpinner.next(false);
    //   }
    // }
  }
  ngAfterViewInit(): void {
    this.cdr.detectChanges();
  }
  ngOnDestroy(): void {
    this.subscription.forEach(item => item.unsubscribe());
  }
  public ngOnInit(): void {
    this.subscription.push(this.subjectSpinner.pipe(debounceTime(50)).subscribe( async (show) => {
      // if (show === true) {
      //   console.log('this.isShow === true', show);
      //   this.isShow = true;
      //   this.spinner.show();
      // } else if (show === false) {
      //   console.log('this.isShow === false', show);
      //   this.isShow = false;
      //   this.spinner.hide();
      // }
    }));
    // this.getIngredientInfo();
    this.getBuilding(() => {
      this.buildingID = +localStorage.getItem('buildingID');
      if (this.buildingID === 0) {
        this.alertify.warning('Vui lòng chọn tòa nhà trước!', true);
      }
    });
    this.getAllIngredient();

    this.checkQRCode();
  }
  getBuilding(callback): void {
    this.buildingService.getBuildings().subscribe(async (buildingData) => {
      this.buildings = buildingData.filter(item => item.level === BUILDING_LEVEL);
      callback();
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
    localStorage.setItem('buildingID', this.buildingID + '');
    this.buildingID = args.itemData.id;
    this.buildingName = args.itemData.name;
    this.getAllIngredientInfoByBuilding();

  }
  NO(index) {
    return (this.ingredientinfoGrid.pageSettings.currentPage - 1) * this.ingredientinfoGrid.pageSettings.pageSize + Number(index) + 1;
  }
  dataBound() {
    this.ingredientinfoGrid.autoFitColumns();
  }
  OutputChange(args) {
    this.checkin = false;
    this.checkout = true;
    // this.qrcodeChange = null ;
    this.getAllIngredientInfoOutputByBuilding();
  }

  InputChange(args) {
    this.checkin = true;
    this.checkout = false;
    this.getAllIngredientInfoByBuilding();
    // this.qrcodeChange = null ;
  }
  toolbarClick(args): void {
    switch (args.item.text) {
      /* tslint:disable */
      case 'Excel Export':
        this.grid.excelExport();
        break;
      /* tslint:enable */
      case 'PDF Export':
        break;
    }
  }
  showPopupWindow(count, chemical) {
    this.alertify.$swal.fire({
      html: `<div class='d-flex justify-content-center align-items-center' style='Width:100%; height: 400px;'>
               <h1 style='font-size: 150px;' class='display-1 mb-3 align-self-center text-${this.toggleColor === true ? 'success' : 'danger'} font-weight-bold'> ${count} | ${chemical.name}</h1>
             </div>`,
      timer: 2000,
      showConfirmButton: false,
      timerProgressBar: true,
      width: '90%',
      icon: 'success'
    });
    this.toggleColor = !this.toggleColor;
  }
  private checkQRCode() {
    this.subscription.push(this.subject
      .pipe(debounceTime(500))
      .subscribe(async (res) => {
        // const commonPattern = /(\d+)-(\w+)-([\w\-\d]+)/g;
        const dateAndBatch = /(\d+)-(\w+)-/g;
        const validFormat = res.QRCode.match(dateAndBatch);
        const array = [];
        for (const item of res.QRCode.split('    ')) {
          array.push(item);
        }
        console.log(array);
        // Update 08/04/2021 - Leo
        const input = res.QRCode.split('    ') || [];
        const qrcode = input[2].split(":")[1].trim() + ':' + input[0].split(":")[1].trim();
        // End Update

        // const qrcode = res.QRCode.replace(validFormat[0], '');
        const levels = [1, 0];
        const building = JSON.parse(localStorage.getItem('building'));
        let buildingName = building.name;
        if (levels.includes(building.level)) {
          buildingName = 'E';
        }
        const chemical = this.findIngredientCode(qrcode);

        if (this.checkin === true) {
          if (this.checkCode === true) {
            const userID = JSON.parse(localStorage.getItem('user')).user.id;
            const model = {
              qrCode: res.QRCode,
              building: this.buildingName,
              userid: userID
            };
            // this.ingredientService.scanQRCodeFromChemicalWareHouse(res.QRCode, this.buildingName, userID).subscribe((status: any) => {
            //   if (status === true) {
            //     this.getAllIngredientInfoByBuilding();
            //     const count = this.findInputedIngredient(qrcode);
            //     this.showPopupWindow(count, chemical);
            //   }
            // });


            this.ingredientService.scanQRCodeFromChemicalWareHouseV1(model).subscribe((status: any) => { // Update 08/04/2021 - Leo
              if (status === true) {
                this.getAllIngredientInfoByBuilding();
                const count = this.findInputedIngredient(qrcode);
                this.showPopupWindow(count, chemical);
              }
            });
          } else {
            this.alertify.error('Wrong Chemical!');
          }
        } else {
          if (this.checkCode === true) {
            const userID = JSON.parse(localStorage.getItem('user')).user.id;
            const model = {
              qrCode: res.QRCode,
              building: this.buildingName,
              userid: userID
            };
            // this.ingredientService.scanQRCodeOutput(res.QRCode, this.buildingName, userID).subscribe((status: any) => {
            //   if (status === true) {
            //     this.getAllIngredientInfoOutputByBuilding();
            //     const count = this.findOutputedIngredient(qrcode);
            //     this.showPopupWindow(count, chemical);
            //   } else {
            //     this.alertify.error(status.message);
            //   }
            // });

            this.ingredientService.scanQRCodeOutputV1(model).subscribe((status: any) => { // Update 08/04/2021 - Leo
              if (status === true) {
                this.getAllIngredientInfoOutputByBuilding();
                const count = this.findOutputedIngredient(qrcode);
                this.showPopupWindow(count, chemical);
              } else {
                this.alertify.error(status.message);
              }
            });
          } else {
            this.alertify.error('Wrong Chemical!');
          }
        }
      }));
  }

  // sau khi scan input thay doi
  async onNgModelChangeScanQRCode(args) {
    if (this.buildingID === 0) {
      this.alertify.warning('Vui lòng chọn tòa nhà trước!', true);
    } else {
      const scanner: IScanner = {
        QRCode: args,
        ingredient: null
      };
      this.subject.next(scanner);
    }
  }

  // load danh sach IngredientInfo
  getIngredientInfo() {
    this.ingredientService.getAllIngredientInfo().subscribe((res: any) => {
      this.data = res;
      // this.ConvertClass(res);
    });
  }

  getIngredientInfoOutput() {
    this.ingredientService.getAllIngredientInfoOutput().subscribe((res: any) => {
      this.data = res;
      // this.ConvertClass(res);
    });
  }
  getAllIngredientInfoByBuilding() {
    this.ingredientService.getAllIngredientInfoByBuilding(this.buildingName).subscribe((res: any) => {
      this.data = res;
      // this.ConvertClass(res);
    });
  }

  getAllIngredientInfoOutputByBuilding() {
    this.ingredientService.getAllIngredientInfoOutputByBuilding(this.buildingName).subscribe((res: any) => {
      this.data = res;
      // this.ConvertClass(res);
    });
  }
  // tim Qrcode dang scan co ton tai khong
  findIngredientCode(code) {
    for (const item of this.ingredients) {
      if (item.partNO === code) {
        // return true;
        this.checkCode = true;
        return item;
      } else {
        this.checkCode = false;
      }
    }
  }
  findInputedIngredient(code) {
    let count = this.data.filter((item: any) => item.code === code && item.status === false).length;
    return count = count === 0 ? 1 : count + 1;
  }
  findOutputedIngredient(code) {
    let count = this.data.filter((item: any) => item.code === code && item.status === true).length;
    return count = count === 0 ? 1 : count + 1;
  }
  // lay toan bo Ingredient
  getAllIngredient() {
    this.ingredientService.getAllIngredient().subscribe((res: any) => {
      this.ingredients = res;
      console.log('Global Ingerdient: ', res);
    });
  }

  // dung de convert color input khi scan nhung chua can dung
  ConvertClass(res) {
    if (res.length !== 0) {
      this.test = 'form-control success-scan';
    } else {
      this.test = 'form-control error-scan';
      this.alertify.error('Wrong Chemical!');
    }
  }

  // xoa Ingredient Receiving
  delete(item) {
    this.ingredientService.deleteIngredientInfo(item.id, item.code, item.qty, item.batch).subscribe(() => {
      this.alertify.success('Delete Success!');
      this.getIngredientInfo();
      this.getIngredientInfoOutput();
    });
  }

  // luu du lieu sau khi scan Qrcode vao IngredientReport
  confirm() {
    this.alertify.confirm('Do you want confirm this', 'Do you want confirm this', () => {
      this.alertify.success('Confirm Success');
    });
  }
}
