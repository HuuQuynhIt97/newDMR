import { BaseComponent } from 'src/app/_core/_component/base.component';
import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';

import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { ActivatedRoute } from '@angular/router';
import { IngredientModalComponent } from './ingredient-modal/ingredient-modal.component';
import { Pagination, PaginatedResult } from 'src/app/_core/_model/pagination';
import { IngredientService } from 'src/app/_core/_service/ingredient.service';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { IGlueType, IIngredient } from 'src/app/_core/_model/Ingredient';
import { ModalNameService } from 'src/app/_core/_service/modal-name.service';
import { QRCodeGenerator, DisplayTextModel } from '@syncfusion/ej2-angular-barcode-generator';
import { ExcelExportProperties, Column, ColumnModel, GridComponent, RowDataBoundEventArgs, RowDDService } from '@syncfusion/ej2-angular-grids';
import { DatePipe } from '@angular/common';
import { DataManager } from '@syncfusion/ej2-data';
import { Tooltip } from '@syncfusion/ej2-angular-popups';
import { IBuilding } from 'src/app/_core/_model/building';
import { IRole } from 'src/app/_core/_model/role';
import { environment } from 'src/environments/environment';
import { ActionConstant } from 'src/app/_core/_constants';
declare let $: any;
const CURRENT_DATE = new Date();
@Component({
  selector: 'app-ingredient',
  templateUrl: './ingredient.component.html',
  styleUrls: ['./ingredient.component.scss'],
  providers: [DatePipe, RowDDService]
})
export class IngredientComponent extends BaseComponent implements OnInit {

  pageSettings = { pageCount: 20, pageSizes: true, currentPage: 1, pageSize: 20 };
  data: any;
  @ViewChild('importModal') importModal: NgbModalRef;
  destData: object[] = [];
  modalReference: NgbModalRef;
  excelDownloadUrl: string;
  defaultDate = new Date(null);
  currentDate = new Date();
  srcDropOptions = { targetID: 'DestGrid' };
  glueTypeData: IGlueType[] = [];
  public displayTextMethod: DisplayTextModel = {
    visibility: false
  };
  destDropOptions = { targetID: 'Grid' };
  public mfg = this.datePipe.transform(new Date(), 'yyyyMMdd');
  public exp = this.datePipe.transform(new Date(new Date().setMonth(new Date().getMonth() + 4)), 'yyyyMMdd');
  @ViewChild('barcode')
  public barcode: QRCodeGenerator;
  @ViewChild('printGrid')
  public printGrid: GridComponent;
  ingredient: IIngredient = {
    id: 0,
    name: '',
    code: '',
    percentage: 0,
    createdDate: new Date(),
    supplierID: 0,
    position: 0,
    allow: 0,
    expiredTime: 0,
    daysToExpiration: 0,
    voc: 0,
    materialNO: '',
    partNO: '',
    unit: 0,
    real: 0,
    cbd: 0,
    glueTypeID: 0,
    replacementFrequency: 0,
    prepareTime: 0,
    standardCycle: 0
  };
  pagination: Pagination;
  page = 1;
  currentPage = 1;
  itemsPerPage = 15;
  totalItems: any;
  file: any;
  toolbar = ['Search', 'Excel Export'];
  text: any;
  dataPrint: Array<{
    id: number,
    code: string,
    name: string,
    unit: number,
    supplier: string,
    supplierID: number,
    batch: string,
    expiredTime: number,
    productionDate: Date,
    daysToExpiration: number,
    exp: string,
    qrCode: string
  }> = [];
  dataPicked: Array<{
    id: number,
    code: string,
    name: string,
    supplier: string,
    supplierID: number,
    batch: string,
    expiredTime: number,
    productionDate: Date,
    daysToExpiration: number,
    exp: string,
    qrCode: string
  }> = [];
  filterSettings: any;
  toolbarOptions: any;
  @ViewChild('ingredientGrid') ingredientGrid: GridComponent;
  show: boolean;
  pd: Date;
  dataIsDone: any;
  role: IRole;
  building: IBuilding;
  SUPERVISOR = 2;
  ADMIN = 1;
  info: { total: any, completeRate: any, complete: any } = { total: 0, completeRate: 0, complete: 0 } ;
  constructor(
    private modalNameService: ModalNameService,
    public modalService: NgbModal,
    private ingredientService: IngredientService,
    private alertify: AlertifyService,
    private datePipe: DatePipe,
    private route: ActivatedRoute,
  ) { super(); }

  ngOnInit() {
    this.Permission(this.route);
    const BUIDLING: IBuilding = JSON.parse(localStorage.getItem('building'));
    const ROLE: IRole = JSON.parse(localStorage.getItem('level'));
    this.role = ROLE;
    this.building = BUIDLING;
    // if (this.role.id === this.ADMIN || this.role.id === this.SUPERVISOR) {
    //   this.toolbarOptions = ['Search', 'Add ', 'Print QR Code', 'Excel Export'];
    // } else {
    //   this.toolbarOptions = ['Search', 'Print QR Code', 'Excel Export'];
    // }
    this.filterSettings = { type: 'Excel' };
    this.excelDownloadUrl = `${environment.apiUrlEC}Ingredient/ExcelExport`;
    this.ingredientService.currentIngredient.subscribe(res => {
      if (res === 300) {
        this.getIngredients();
        this.ingredient = {
          id: 0,
          name: '',
          code: '',
          percentage: 0,
          createdDate: new Date(),
          supplierID: 0,
          position: 0,
          allow: 0,
          voc: 0,
          expiredTime: 0,
          daysToExpiration: 0,
          materialNO: '',
          partNO: '',
          unit: 0,
          real: 0,
          cbd: 0,
          glueTypeID: 0,
          replacementFrequency: 0,
          prepareTime: 0,
          standardCycle: 0
        };
      }
    });
    this.getIngredients();
    this.getAllIngredients();
    this.rate();
  }
  Permission(route: ActivatedRoute) {
    this.functions = JSON.parse(localStorage.getItem('functions'));
    for (const item of this.functions) {
      if (route.snapshot.data.functionCode.includes(item.functionCode)) {
        const toolbarOptions = [];
        for (const action of item.childrens) {
          const optionItem = this.makeAction(action.code);
          toolbarOptions.push(...optionItem.filter(Boolean));
        }
        toolbarOptions.push('Search');
        const uniqueOptionItem = toolbarOptions.filter((elem, index, self) => {
          return index === self.indexOf(elem);
        });
        this.toolbarOptions = uniqueOptionItem;
        break;
      }
    }
  }
  makeAction(input: string): string[] {
    switch (input) {
      case ActionConstant.CREATE:
        this.editSettings.allowAdding = true;
        return ['Add'];
      case ActionConstant.EXCEL_EXPORT:
        return ['Excel Export'];
      case ActionConstant.PRINT:
        return ['Print QR Code'];
      default:
        return [undefined];
    }
  }
  rate() {
    this.ingredientService.rate()
      .subscribe( res => {
        this.info = res.data;
      });
  }
  rowDB(args: RowDataBoundEventArgs) {
    const data = args.data as any;
    if (data.expiredTime === 0 || data.unit === 0 || data.voc === 0 ||  data.daysToExpiration === 0 || data.materialNO === '') {
      args.row.classList.add('bgcolor');
    }
  }
  toolbarClickPrint(args): void {
    switch (args.item.text) {
      case 'Excel Export':
        const data = this.dataPrint.map(item => {
          return {
            supplier: item.supplier,
            name: item.name,
            unit: item.unit,
            qrCode: item.qrCode
          };
        });
        const supplierModel: ColumnModel =
        {
          field: 'supplier',
          headerText: 'Supplier',
          textAlign: 'Center',
          autoFit: true,
          width: 120,
        };
        const nameModel: ColumnModel =
        {
          field: 'name',
          headerText: 'Ingredient',
          textAlign: 'Center',
          autoFit: true,
          width: 120,
        };
        const unitModel: ColumnModel =
        {
          field: 'unit',
          headerText: 'Amount',
          textAlign: 'Center',
          autoFit: true,
          width: 80,
        };
        const qrCoderModel: ColumnModel = {
          field: 'qrCode',
          headerText: 'QR Code',
          textAlign: 'Center',
          autoFit: true,
          width: 200,
        };
        const excelExportProperties: ExcelExportProperties = {
          dataSource: data,
          columns: [
            new Column(supplierModel),
            new Column(nameModel),
            new Column(unitModel),
            new Column(qrCoderModel)
          ],
          fileName: `${new Date().getFullYear()}${new Date().getMonth()}${new Date().getDay()}_Print_Chemical.xlsx`
        };
        this.printGrid.excelExport(excelExportProperties);
        break;
      default:
        break;
    }
  }
  toolbarClick(args): void {
    switch (args.item.id) {
      case 'grid_Excel Export':
        let pdfdata;
        const query = this.ingredientGrid.renderModule.data.generateQuery(); // get grid corresponding query
        for (let i = 0; i < query.queries.length; i++) {
          if (query.queries[i].fn === 'onPage') {
            query.queries.splice(i, 1);       // remove page query to get all records
            break;
          }
        }
        const fdata = new DataManager({ json: this.data }).executeQuery(query).then((e: any) => {
          pdfdata = e.result as object[];  // get all filtered records
          const exportProperties = {
            dataSource: pdfdata,
          };
          const data = pdfdata.map(item => {
            return {
              supplier: item.supplier,
              name: item.name,
              unit: item.unit,
              code: item.materialNO,
              daysToExpiration: item.daysToExpiration
            };
          });
          const supplierModel: ColumnModel =
          {
            field: 'supplier',
            headerText: 'Supplier',
            textAlign: 'Center',
            autoFit: true,
            width: 120,
          };
          const nameModel: ColumnModel =
          {
            field: 'name',
            headerText: 'Ingredient',
            textAlign: 'Center',
            autoFit: true,
            width: 120,
          };
          const unitModel: ColumnModel =
          {
            field: 'unit',
            headerText: 'Amount',
            textAlign: 'Center',
            autoFit: true,
            width: 80,
          };
          const qrCoderModel: ColumnModel = {
            field: 'code',
            headerText: 'Code',
            textAlign: 'Center',
            autoFit: true,
            width: 200,
          };
          const daysToExpirationModel: ColumnModel = {
            field: 'daysToExpiration',
            headerText: 'Days to Expiry',
            textAlign: 'Center',
            autoFit: true,
            width: 200,
          };
          const excelExportProperties: ExcelExportProperties = {
            dataSource: data,
            columns: [
              new Column(supplierModel),
              new Column(nameModel),
              new Column(unitModel),
              new Column(qrCoderModel),
              new Column(daysToExpirationModel)
            ],
            fileName: `${new Date().getFullYear()}${new Date().getMonth()}${new Date().getDay()}_Chemical.xlsx`
          };
          this.ingredientGrid.excelExport(excelExportProperties);
        }).catch((e) => true);
        break;
      case 'grid_add':
        this.openIngredientModalComponent();
        this.ingredientGrid.refresh();
        break;
      case 'grid_Import Excel': this.showModal(); break;
      case 'grid_Print QR Code': this.printBarcode(); break;
      default:
        break;
    }
  }
  actionBegin(args) {
    if (args.requestType === 'save' && args.action === 'edit') {
      for (const key in this.dataPrint) {
        if (this.dataPrint[key].id === args.data.id) {
          this.dataPrint[key].batch = args.data.batch;
          this.dataPrint[key].productionDate = args.data.productionDate;
          // tslint:disable-next-line:max-line-length
          this.dataPrint[key].qrCode = `${this.datePipe.transform(args.data.productionDate, 'yyyyMMdd')}-${args.data.batch}-${args.data.code}`;
          break;
        }
      }
    }
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
  printData() {
    let html = '';
    for (const item of this.dataPicked) {
      const content = document.getElementById(item.code);
      html += `
       <div class='content'>
        <div class='qrcode'>
         ${content.innerHTML}
         </div>
          <div class='info'>
          <ul>
          <li class='subInfo'>Name: ${item.name}</li>
          <li class='subInfo'>QR Code: ${this.datePipe.transform(item.productionDate, 'yyyyMMdd')}-${item.batch}-${item.code}</li>
          <li class='subInfo'>MFG: ${this.datePipe.transform(item.productionDate, 'yyyyMMdd')}</li>
          <li class='subInfo'>EXP: ${item.exp}</li>
          </ul>
         </div>
      </div>
      `;
    }
    this.configurePrint(html);
  }
  printBarcode() {
    this.show = true;
    this.getAllIngredients();

  }
  rowSelected(args) {
    setTimeout(() => {
      this.dataPicked = this.printGrid.getSelectedRecords() as any;
    }, 300);
  }
  rowDeselected(args) {
    setTimeout(() => {
      this.dataPicked = this.printGrid.getSelectedRecords() as any;
    }, 300);
  }
  updateBatch(id, batch) {
    for (const key in this.dataPrint) {
      if (this.dataPrint[key].id === id) {
        this.dataPrint[key].batch = batch;
        break;
      }
    }
    for (const key in this.dataPicked) {
      if (this.dataPicked[key].id === id) {
        this.dataPicked[key].batch = batch;
        break;
      }
    }
  }
  headerCellInfo(args) {
    // if ('replacementFrequency' === args.cell.column.field) {
    //   const toolcontent = 'Replacement Frequency (hours)';
    //   const tooltip: Tooltip = new Tooltip({
    //     content: toolcontent
    //   });
    //   tooltip.appendTo(args.node);
    // }
    // if ('prepareTime' === args.cell.column.field) {
    //   const toolcontent = 'Prepare Time (hour)';
    //   const tooltip: Tooltip = new Tooltip({
    //     content: toolcontent
    //   });
    //   tooltip.appendTo(args.node);
    // }
  }
  updateProductionDate(id, productionDate) {
    for (const key in this.dataPrint) {
      if (this.dataPrint[key].id === id) {
        this.dataPrint[key].productionDate = productionDate;
        break;
      }
    }
    for (const key in this.dataPicked) {
      if (this.dataPicked[key].id === id) {
        this.dataPicked[key].productionDate = productionDate;
        break;
      }
    }
  }
  updateExp(id, batch) {
    for (const key in this.dataPrint) {
      if (this.dataPrint[key].id === id) {
        this.dataPrint[key].exp = batch;
        break;
      }
    }
    for (const key in this.dataPicked) {
      if (this.dataPicked[key].id === id) {
        this.dataPicked[key].exp = batch;
        break;
      }
    }
  }
  pad(d) {
    return (d < 10) ? '0' + d.toString() : d.toString();
  }
  onChangeDate(args, data) {
    if (args.isInteracted) {
      // if (args.value === null) { return; }
      // this.pd = (args.value as Date);
      // const productionDate = this.datePipe.transform(this.pd, 'yyyyMMdd');
      // console.log(productionDate);
      // this.updateProductionDate(data.id, productionDate);
      // const expDate = this.datePipe.transform(this.pd.setDate(this.pd.getDate() + data.daysToExpiration), 'yyyy/MM/dd');
      // this.updateExp(data.id, expDate);
    }
  }
  backList() {
    this.show = false;
    this.dataPicked = [];
  }
  getIngredients() {
    // this.spinner.show();
    this.ingredientService.getAllIngredient()
      .subscribe((res: any) => {
        this.data = res.map((item: any) => {
          return {
            id: item.id,
            supplier: item.supplier,
            supplierID: item.supplierID,
            name: item.name,
            code: item.code,
            voc: item.voc,
            expiredTime: item.expiredTime,
            daysToExpiration: item.daysToExpiration,
            materialNO: item.materialNO,
            partNO: item.partNO?.split(':')[0],
            unit: item.unit,
            real: item.real,
            cbd: item.cbd,
            glueTypeID: item.glueType === null ? 0 : item.glueType?.id,
            glueType: item.glueType === null ? '' : item.glueType?.title,
            replacementFrequency: item.replacementFrequency,
            prepareTime: item.prepareTime,
            createdDate: new Date(item.createdDate),
            standardCycle: item.standardCycle
          };
        }) || [];
        const total = this.data.length;
        const undone = this.data.filter(item =>
          item.expiredTime === 0 || item.unit === 0 || item.daysToExpiration === 0 || item.materialNO === ''
        ).length;
        this.dataIsDone = `${undone}/${total}`;
      }, error => {
        this.alertify.error(error);
      });
  }
  getAllIngredients() {
    this.ingredientService.getAllIngredient()
      .subscribe((res: any) => {
        this.dataPrint = res.map((item: any) => {
          return {
            id: item.id,
            code: item.materialNO,
            name: item.name,
            supplier: item.supplier,
            supplierID: item.supplierID,
            batch: 'DEFAULT',
            unit: item.unit,
            expiredTime: item.expiredTime,
            daysToExpiration: item.daysToExpiration,
            replacementFrequency: item.replacementFrequency,
            prepareTime: item.prepareTime,
            productionDate: new Date(),
            qrCode: `${this.datePipe.transform(new Date(), 'yyyyMMdd')}-DEFAULT-${item.materialNO}`,
            exp: this.datePipe.transform(new Date(new Date().setDate(new Date().getDate() + item.daysToExpiration)), 'yyyyMMdd')
          };
        });
      }, error => {
        this.alertify.error(error);
      });
  }
  search() {
    // this.spinner.show();
    if (this.text) {
      this.ingredientService.search(this.currentPage, this.itemsPerPage, this.text)
        .subscribe((res: PaginatedResult<IIngredient[]>) => {
          this.data = res.result;
          this.pagination = res.pagination;
          this.totalItems = res.pagination.totalItems;
          this.currentPage = res.pagination.currentPage;
          this.itemsPerPage = res.pagination.itemsPerPage;
          //    this.spinner.hide();
        }, error => {
          this.alertify.error(error);
        });
    } else {
      this.getIngredients();
    }
  }

  getAll() {
    this.ingredientService.getAllIngredient().subscribe(res => {
      this.data = res;
      console.log(res);
    });
  }
  delete(ingredient: IIngredient) {
    this.alertify.confirm('Delete Ingredient', 'Are you sure you want to delete this IngredientID "' + ingredient.id + '" ?', () => {
      this.ingredientService.delete(ingredient.id).subscribe(() => {
        this.getIngredients();
        this.alertify.success('Ingredient has been deleted');
      }, error => {
        this.alertify.error('Failed to delete the Ingredient');
      });
    });
  }
  onPageChange($event) {
    this.currentPage = $event;
    if (this.text) {
      this.ingredientService.search(this.currentPage, this.itemsPerPage, this.text)
        .subscribe((res: PaginatedResult<IIngredient[]>) => {
          this.data = res.result;
          this.pagination = res.pagination;
          this.totalItems = res.pagination.totalItems;
          this.currentPage = res.pagination.currentPage;
          this.itemsPerPage = res.pagination.itemsPerPage;
          //    this.spinner.hide();
        }, error => {
          this.alertify.error(error);
        });
    } else {
      this.getIngredients();
    }
  }
  openIngredientModalComponent() {
    const modalRef = this.modalService.open(IngredientModalComponent, { size: 'xl' });
    modalRef.componentInstance.ingredient = this.ingredient;
    modalRef.componentInstance.title = 'Add Chemical';
    modalRef.result.then((result) => {
    }, (reason) => {
    });
  }
  openIngredientEditModalComponent(item) {
    const modalRef = this.modalService.open(IngredientModalComponent, { size: 'xl' });
    modalRef.componentInstance.ingredient = item;
    modalRef.componentInstance.title = 'Edit Chemical';
    modalRef.result.then((result) => {
    }, (reason) => {
    });
  }
  fileProgress(event) {
    this.file = event.target.files[0];
  }
  uploadFile() {
    const createdBy = JSON.parse(localStorage.getItem('user')).user.id;
    this.ingredientService.import(this.file, createdBy)
      .subscribe((res: any) => {
        this.getAll();
        this.alertify.success('The excel has been imported into system!');
        this.modalService.dismissAll();
      }, error => {
          this.alertify.error(error, true);
      });
  }
  showModal() {
    this.modalReference = this.modalService.open(this.importModal, { size: 'xl' });
  }
  NO(index) {
    return (this.ingredientGrid.pageSettings.currentPage - 1) * this.ingredientGrid.pageSettings.pageSize + Number(index) + 1;
  }
  dataBound() {
   this.ingredientGrid.autoFitColumns();
  }
}
