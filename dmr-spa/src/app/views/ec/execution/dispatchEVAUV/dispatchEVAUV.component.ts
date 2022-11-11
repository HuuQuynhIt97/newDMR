import { DatePipe } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { IAddDispatchParams, IDispatchParams, IUpdateDispatchParams } from 'src/app/_core/_model/dispatch';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { BottomFactoryService } from 'src/app/_core/_service/bottom.factory.service';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'app-dispatchEVAUV',
  templateUrl: './dispatchEVAUV.component.html',
  styleUrls: ['./dispatchEVAUV.component.scss'],
  providers: [DatePipe]
})
export class DispatchEVAUVComponent implements OnInit {
  @Input() data: any;
  dispatchData: any;
  editSettings = { showDeleteConfirmDialog: false, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Normal' };
  toolbarOptions = ['Add', 'Edit', 'Cancel', 'Search'];
  option = 'Default';
  lineNames: [];
  lines: { name: string, amountTotal: number }[] = [] as { name: string, amountTotal: number }[];
  obj: IAddDispatchParams;
  objForGet: IDispatchParams;
  lineItem: string;
  isEditing: boolean;
  constructor(
    public activeModal: NgbActiveModal,
    public alertify: AlertifyService,
    private datePipe: DatePipe,
    public bottomFactoryService: BottomFactoryService
  ) { }

  ngOnInit() {
    this.lineNames = this.data.lineNames;
    this.objForGet = {
      buildingID: this.data.buildingID,
      glueNameID: this.data.glueNameID,
      mixingInfoID: this.data.mixingInfoID
    };
    this.getAllDispatch();
  }
  toolbarClick(args) {
    // if (args.item.id === "grid_367907553_0_add") {
    //   this.isEditing = false;
    //   this.lineItem = '';
    // }
    this.isEditing = false;
    this.lineItem = '';
  }
  recordDoubleClick(args) {
    this.isEditing = true;
  }
  actionBegin(args) {
    if (args.requestType === 'beginEdit') {
      this.lineItem = args.rowData.building.name;
    }
    if (args.requestType === 'save' && args.action === 'edit') {
      const data = args.data;
      if (data.amount - data.remainingAmount < 0) {
        this.alertify.warning('Vui lòng nhập KL còn lại nhỏ hơn hoặc bằng khối lượng!');
        args.cancel = true;
        return;
      }
      else if (data.amount - data.remainingAmount === 0) {
        this.alertify.warning('Vui lòng nhập số lượng còn lại bằng 0.!');
        args.cancel = true;
        return;
      }
      const update: IUpdateDispatchParams = {
        iD: data.id,
        remaningAmount: data.remainingAmount
      };
      this.bottomFactoryService.updateDispatch(update).subscribe(() => {
        this.alertify.success('Thành Công!');
        this.getAllDispatch();
      }, error => {
          this.alertify.warning('Lỗi máy chủ! Vui lòng liên hệ Lab-Team!');
      });
    }
  }
  onChangeLine(args) {
    console.log('onchangeline', args.itemData.value);
    this.obj = {
      mixingInfoID: this.data.mixingInfoID,
      lineName: args.itemData.value,
      glueNameID: this.data.glueNameID,
      option: this.option,
      buildingID: this.data.buildingID,
      estimatedStartTime: this.data.estimatedStartTime,
      estimatedFinishTime: this.data.estimatedFinishTime
    };
    this.addDispatch();
    console.log('onchangeline', this.obj);

  }
  onChangeReset(args) {
    console.log('onChangeReset', this.option);
  }
  onChangeDefault(args) {
    console.log('onChangeDefault', this.option);
  }

  getAllDispatch() {
    this.bottomFactoryService.getAllDispatch(this.objForGet)
      .subscribe((res: any) => {
        this.dispatchData = res.dispatches;
        this.lines = res.lines;
       }
        , err => {
        });
  }
  addDispatch() {
    this.bottomFactoryService.addDispatch(this.obj)
      .subscribe((res: any) => {
        this.option = 'Default';
        this.lineItem = '';
        this.getAllDispatch();
       }
        , err => {
          this.alertify.warning(err);
          this.getAllDispatch();
        });
  }
}
