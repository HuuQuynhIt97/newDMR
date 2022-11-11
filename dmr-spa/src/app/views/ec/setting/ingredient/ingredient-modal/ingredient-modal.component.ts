import { Component, OnInit, Input } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { IGlueType } from 'src/app/_core/_model/glue-type';
import {  IIngredient } from 'src/app/_core/_model/Ingredient';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { IngredientService } from 'src/app/_core/_service/ingredient.service';

@Component({
  selector: 'app-ingredient-modal',
  templateUrl: './ingredient-modal.component.html',
  styleUrls: ['./ingredient-modal.component.scss']
})
export class IngredientModalComponent implements OnInit {
  fields: object = { text: 'title', value: 'id' };
  @Input() title: '';
  @Input() ingredient: IIngredient = {
    id: 0,
    name: '',
    percentage: 0,
    code: this.makeid(8),
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
  supplier: any [] = [];
  public fieldsGlue: object = { text: 'name', value: 'id' };
  public textGlue = 'Select Supplier name';
  showBarCode: boolean;
  glueTypeData: IGlueType[];
  glueTypeID: number;
  constructor(
    public activeModal: NgbActiveModal,
    private alertify: AlertifyService,
    private ingredientService: IngredientService,
  ) { }

  ngOnInit() {
    if (this.ingredient.id === 0) {
      this.showBarCode = false;
      this.genaratorIngredientCode();
    } else {
      this.showBarCode = true;
    }
    this.getAllGlueType();
    this.getSupllier();
  }
  getAllGlueType() {
    this.ingredientService.getAllGlueType().subscribe(res => {
      this.glueTypeData = res.filter(item => item.level === 2);
      const NA = {
        code: 'N/A',
        id: null,
        level: 1,
        minutes: 0,
        parentID: null,
        rpm: 0,
        title: 'N/A',
        method: ''
      } as IGlueType;
      this.glueTypeData.unshift(NA);
    });
  }
  onSelectGlueType(args: any): void {
    this.ingredient.glueTypeID = +args.itemData.id;
  }
  validForm() {
    if (this.ingredient.name === '') {
      this.alertify.warning('Please key in chemical name<br> Vui lòng nhập tên hóa chất', true);
      return false;
    }
    if (this.ingredient.materialNO === '') {
      this.alertify.warning('Please key in the material NO <br> Vui lòng nhập material NO', true);
      return false;
    }

    if (this.ingredient.partNO === '') {
      this.alertify.warning('Please key in the part NO <br> Vui lòng nhập part NO', true);
      return false;
    }
    if (this.ingredient.unit === 0) {
      this.alertify.warning('The amount must be greater than zero<br> Vui lòng nhập khối lượng phải lớn hơn 0', true);
      return false;
    }
    if (this.ingredient.expiredTime === 0) {
      this.alertify.warning('The expiry period must be greater than zero<br> Vui lòng nhập thời gian hết hạn phải lớn hơn 0', true);
      return false;
    }
    if (this.ingredient.daysToExpiration === 0) {
      this.alertify.warning('The days to expiry must be greater than zero<br> Vui lòng nhập ngày hết hạn phải lớn hơn 0', true);
      return false;
    }
    // if (this.ingredient.replacementFrequency === 0 || this.ingredient.replacementFrequency > 9.5) {
    //   this.alertify.warning(`
    //   Please key in the replacement frequency.
    //    This must be greater than zero or less than 9.5 hours (a working day)
    //   <br> Vui lòng nhập replacement frequency lớn hơn 0 và không được vượt quá 9.5 giờ (1 ngày làm việc tính cả giờ tăng ca)`, true);
    //   return false;
    // }
    // if (this.ingredient.prepareTime === 0) {
    //   this.alertify.warning('The prepare time must be greater than zero<br> Vui lòng nhập thời gian chuẩn bị phải lớn hơn 0', true);
    //   return false;
    // }
    if (this.ingredient.supplierID === 0) {
      this.alertify.warning('Please select a supplier<br> Vui lòng chọn 1 nhà cung cấp', true);
      return false;
    }
    return true;
  }
  create() {
    const check = this.validForm();
    if (!check) { return; }
    // tslint:disable-next-line:no-string-literal
    delete this.ingredient['glueType'];
    this.ingredientService.create(this.ingredient).subscribe( () => {
      this.alertify.success('Created successed!');
      this.activeModal.dismiss();
      this.ingredientService.changeIngredient(300);
    },
    (error) => {
      this.alertify.error(error);
      this.genaratorIngredientCode();
    });
  }

  update() {
    // const validNumber = /^(^[0-9]+[.])?[0-9]+$/.test(this.ingredient.replacementFrequency.toString());
    // if (this.ingredient.replacementFrequency ) {
      // }
     // tslint:disable-next-line:no-string-literal
    delete this.ingredient['glueType'];
    this.ingredientService.update(this.ingredient).subscribe( res => {
      this.alertify.success('Updated successed!');
      this.activeModal.dismiss();
      this.ingredientService.changeIngredient(300);
    });
  }

  onChangeSup(args) {
    this.ingredient.supplierID = args.value;
  }
  onChangeGlueType(args) {
    this.ingredient.glueTypeID = +args.value;
  }
  save() {
    if (this.ingredient.id === 0) {
      if (this.ingredient.code === '') {
        this.genaratorIngredientCode();
      }
      this.create();
    } else {
      this.update();
    }
  }

  onBlur($event) {
    this.showBarCode = true;
  }

  getSupllier() {
    this.ingredientService.getAllSupplier().subscribe(res => {
      this.supplier = res ;
    });
  }

  genaratorIngredientCode() {
    this.ingredient.code = this.makeid(8);
  }

  makeid(length) {
    let result           = '';
    const characters       = '0123456789';
    const charactersLength = characters.length;
    for ( let i = 0; i < length; i++ ) {
       result += characters.charAt(Math.floor(Math.random() * charactersLength));
    }
    return result;
   // return '59129032';
  }

}
