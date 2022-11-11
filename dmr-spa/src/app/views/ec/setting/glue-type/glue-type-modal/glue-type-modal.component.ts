import { Component, OnInit, Input } from '@angular/core';
import { GlueTypeService } from 'src/app/_core/_service/glue-type.service';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { IGlueType } from 'src/app/_core/_model/glue-type';

@Component({
  selector: 'app-glue-type-modal',
  templateUrl: './glue-type-modal.component.html',
  styleUrls: ['./glue-type-modal.component.css']
})
export class GlueTypeModalComponent implements OnInit {
  @Input() title: string;
  @Input() glueType: IGlueType;
  fields: object = { text: 'text', value: 'value' };
  methods = [{ text: 'Stir', value: 'Stir' }, { text: 'Shaking', value: 'Shaking' } ];
  method: any;
  constructor(
    public activeModal: NgbActiveModal,
    private glueTypeService: GlueTypeService,
    private alertify: AlertifyService,
  ) { }

  ngOnInit() {
  }
  validation() {
    if (this.glueType.title === '') {
      this.alertify.warning('Please enter glue type name!', true);
      return false;
    } else {
      return true;
    }
  }
  onChangeMethod(args) {
    this.method = args.itemData.value;
  }
  createGlueType() {
    if (this.validation()) {
      if (this.glueType.parentID > 0) {
        this.glueType.method = this.method;
        this.glueTypeService.createChild(this.glueType).subscribe(res => {
          this.alertify.success('The glue type has been created!!');
          this.activeModal.dismiss();
          this.glueTypeService.changeMessage(200);
        });
      } else {
        this.glueTypeService.createParent(this.glueType).subscribe(res => {
          this.glueTypeService.changeMessage(200);
          this.alertify.success('The glue type has been created!!');
          this.activeModal.dismiss();
        });
      }
    }
  }
}
