import { Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { ShakeService } from 'src/app/_core/_service/shake.service';

@Component({
  selector: 'app-shake',
  templateUrl: './shake.component.html',
  styleUrls: ['./shake.component.scss']
})
export class ShakeComponent implements OnInit {
  shakesData: any;
  editSettings = { showDeleteConfirmDialog: false, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Normal' };
  // toolbarOptions = ['Add', 'Edit', 'Cancel', 'Search'];
  option = 'Default';
  lineNames: [];
  lineItem: string;
  glueName: string;
  tab: string;
  mixingInfoID: number;
  constructor(
    public alertify: AlertifyService,
    public shakeService: ShakeService,
    private route: ActivatedRoute,
    private router: Router,
  ) { }

  ngOnInit() {
    this.onRouteChange();
  }

  onRouteChange() {
    this.route.data.subscribe(data => {
      if (this.route.snapshot.params.glueName !== undefined) {
        this.glueName = this.route.snapshot.params.glueName.includes('%2B')
          ? this.route.snapshot.params.glueName.replace(/\%2B/g, '+')
          : this.route.snapshot.params.glueName;
      }
      const tab = this.route.snapshot.params.tab;
      this.tab = tab;
      this.mixingInfoID = this.route.snapshot.params.mixingInfoID;
      this.getShakesByMixingInfoID();
    });
  }
  finish() { }
  actionBegin(args) { }

  getShakesByMixingInfoID() {
    this.shakeService.getShakesByMixingInfoID(this.mixingInfoID).subscribe((res: any) => {
      if (res.status) {
        this.shakesData = res.data || [];

      } else {
        this.alertify.error(res.message);
      }
    } , err => {
      this.alertify.error('Lỗi mạng hoặc máy chủ đã bị tắt!');
    });
  }

  back() {
    this.router.navigate([
      `/ec/execution/todolist-2/${this.tab}`
    ]);
  }
}
