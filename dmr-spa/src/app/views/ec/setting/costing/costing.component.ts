import { Component, OnInit, ViewChild } from '@angular/core';
import { GridComponent } from '@syncfusion/ej2-angular-grids';
import { IIngredient } from 'src/app/_core/_model/Ingredient';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { IngredientService } from 'src/app/_core/_service/ingredient.service';

@Component({
  selector: 'app-costing',
  templateUrl: './costing.component.html',
  styleUrls: ['./costing.component.css']
})
export class CostingComponent implements OnInit {
  pageSettings = { pageCount: 20, pageSizes: true, currentPage: 1, pageSize: 20 };
  @ViewChild('ingredientGrid') ingredientGrid: GridComponent;
  data: any;
  toolbarOptions: string[];
  filterSettings: { type: string; };
  editSettings = { showDeleteConfirmDialog: false, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Normal' };
  setFocus: any;
  constructor(
    private ingredientService: IngredientService,
    private alertify: AlertifyService,
  ) { }

  ngOnInit() {
    this.toolbarOptions = ['Search', 'Edit', 'Cancel', 'ExcelExport'];
    this.filterSettings = { type: 'Excel' };
    this.getIngredients();
  }
  toolbarClick(args): void {
    switch (args.item.id) {
      /* tslint:disable */
      case 'grid_excelexport':
        this.ingredientGrid.excelExport();
        break;
      /* tslint:enable */
      case 'PDF Export':
        break;
    }
  }
  actionBegin(args) {
    if (args.requestType === 'save' && args.action === 'edit') {
      const data = args.data;
      const ingredient: IIngredient = {
        id: data.id,
        name: data.name,
        percentage: 0,
        code: data.code,
        createdDate: data.createdDate,
        supplierID: data.supplierID,
        position: 0,
        allow: 0,
        voc: data.voc,
        expiredTime: data.expiredTime,
        daysToExpiration: data.daysToExpiration,
        materialNO: data.materialNO,
        unit: data.unit,
        real: data.real,
        cbd: data.cbd,
        glueTypeID: data.glueTypeID,
        replacementFrequency: data.replacementFrequency,
        prepareTime: data.prepareTime,
        standardCycle: data.standardCycle,
        partNO: data.partNO
      };
      this.update(ingredient);
    }
  }

  onDoubleClick(args: any): void {
    this.setFocus = args.column; // Get the column from Double click event
  }
  actionComplete(args) {
    if (args.requestType === 'beginEdit') {
      args.form.elements.namedItem(this.setFocus.field).focus(); // Set focus to the Target element
      // e.form.elements.namedItem(this.setFocus.field).value = ''; // Set focus to the Target element
    }
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
            unit: item.unit,
            real: item.real,
            cbd: item.cbd,
            createdDate: new Date(item.createdDate),
          };
        });
      }, error => {
        this.alertify.error(error);
      });
  }

  update(ingredient) {
    this.ingredientService.update(ingredient).subscribe(res => {
      this.alertify.success('Updated successed!');
      this.getIngredients();
    });
  }

  NO(index) {
    return (this.ingredientGrid.pageSettings.currentPage - 1) * this.ingredientGrid.pageSettings.pageSize + Number(index) + 1;
  }
}
