import { BaseComponent } from 'src/app/_core/_component/base.component';
import { Component, OnInit, ViewChild , OnDestroy} from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { IngredientService } from 'src/app/_core/_service/ingredient.service';
import { DatePipe } from '@angular/common';
import { FilteringEventArgs } from '@syncfusion/ej2-angular-dropdowns';
import { EmitType } from '@syncfusion/ej2-base/';
import { Query } from '@syncfusion/ej2-data/';
import { DecimalPipe } from '@angular/common';
import { GridComponent } from '@syncfusion/ej2-angular-grids';
import { BuildingService } from 'src/app/_core/_service/building.service';
import { IBuilding } from 'src/app/_core/_model/building';
import { Subject, Subscription } from 'rxjs';
import { IScanner } from 'src/app/_core/_model/IToDoList';
import { ActivatedRoute } from '@angular/router';
import { ActionConstant } from 'src/app/_core/_constants';
import { AuthService } from 'src/app/_core/_service/auth.service';
const ADMIN = 1;
const SUPERVISER = 2;
const BUILDING_LEVEL = 2;
@Component({
  selector: 'app-inventory',
  templateUrl: './inventory.component.html',
  styleUrls: ['./inventory.component.css'],
  providers: [
    DatePipe,
    DecimalPipe
  ]
})
export class InventoryComponent extends BaseComponent implements OnInit {
  startDate: object = new Date();
  endDate: object = new Date();
  qrcodeChange: any;
  searchText: any;
  public role = JSON.parse(localStorage.getItem('level'));
  public building = JSON.parse(localStorage.getItem('building'));
  pageSettings = { pageCount: 20, pageSizes: true, pageSize: 20 };
  filterSettings = { type: 'Excel' };
  public fieldsBuildings: object = { text: 'name', value: 'id' };

  @ViewChild('ingredientinforeportGrid') ingredientinforeportGrid: GridComponent;
  data: any = [];
  public ingredients: any = [];
  buildingID: any;
  buildings: IBuilding[];
  IsAdmin = false;
  buildingName: any;
  subject = new Subject<IScanner>();
  subscription: Subscription[] = [];
  constructor(
    public modalService: NgbModal,
    public ingredientService: IngredientService,
    private buildingService: BuildingService,
    private authService: AuthService,
    private alertify: AlertifyService,
    private route: ActivatedRoute,
  ) {
    super();
  }

  ngOnInit() {
    this.Permission(this.route);
    this.checkRole();
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
  }
  getBuilding(callback): void {
    const userID = +JSON.parse(localStorage.getItem('user')).user.id;
    this.authService.getBuildingUserByUserID(userID).subscribe((res) => {
      this.buildings = res.data;
      callback();
    });
    // this.buildingService.getBuildings().subscribe(async (buildingData) => {
    //   this.buildings = buildingData.filter(item => item.level === SystemConstant.BUILDING_LEVEL);
    //   callback();
    // });
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
    this.buildingName = args.itemData.name;
    localStorage.setItem('buildingID', args.itemData.id);
    this.searchWithBuilding(this.startDate, this.endDate);
  }

  // private getBuilding(): void {
  //   this.buildingService.getBuildings().subscribe(async (buildingData) => {
  //     this.buildings = buildingData.filter(item => item.level === BUILDING_LEVEL);
  //   });
  // }
  resetSearch() {
    this.searchText = null ;
  }
  toolbarClick(args) {
    switch (args.item.text) {
      /* tslint:disable */
      case 'ExcelExport':
        this.ingredientinforeportGrid.excelExport();
        break;
    }
  }
  getIngredientInfoReport() {
    this.ingredientService.getAllIngredientInfo().subscribe((res: any) => {
      this.data = res ;
    });
  }
  getIngredientInfoReportByBuilding() {
    this.ingredientService.getAllIngredientInfoReportByBuildingName(this.buildingName).subscribe((res: any) => {
      this.data = res ;
    });
  }

  startDateOnchange(args) {
    this.startDate = (args.value as Date);
    // this.search(this.startDate, this.endDate);
    this.searchWithBuilding(this.startDate, this.endDate);
  }

  endDateOnchange(args) {
    this.endDate = (args.value as Date);
    // this.search(this.startDate, this.endDate);
    this.searchWithBuilding(this.startDate, this.endDate);
  }

  onClickDefault() {
    this.startDate = new Date();
    this.endDate = new Date();
    // this.getIngredientInfoReport();
    this.searchWithBuilding(this.startDate, this.endDate);
  }
  searchWithBuilding(startDate, endDate) {
    this.ingredientService.searchInventoryByBuildingName(startDate.toDateString(), endDate.toDateString(), this.buildingName)
    .subscribe((res: any) => {
      this.data = res ;
    });
  }

}
