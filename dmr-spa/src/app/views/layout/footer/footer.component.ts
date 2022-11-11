import { AuthenticationService } from 'src/app/_core/_service/authentication.service';
import { VersionService } from './../../../_core/_service/version.service';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { HubConnectionState } from '@microsoft/signalr';
import * as signalr from '../../../../assets/js/ec-client.js';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.css']
})
export class FooterComponent implements OnInit {
  online: number;
  userID: number;
  userName: any;
  modalReference: any;
  data: [] = [];
  firstItem: any;
  constructor(public modalService: NgbModal,
              private authenticationService: AuthenticationService,
              private versionService: VersionService) {
    // this.userName = JSON.parse(localStorage.getItem('user')).user.username;
    // this.userID = +JSON.parse(localStorage.getItem('user')).user.id;
    this.authenticationService.user$.subscribe( x => {
      this.userName = x.username;
      this.userID = x.id;
    });
  }
  ngOnInit(): void {
    this.versionService.getAllVersion().subscribe( (item: any) => {
      this.data = item;
      this.firstItem = item[0] || {};
    });
    if (signalr.CONNECTION_HUB.state === HubConnectionState.Connected) {
      signalr.CONNECTION_HUB
        .invoke('CheckOnline', this.userID, this.userName)
        .catch(error => {
          console.log(`CheckOnline error: ${error}`);
        }
        );
      signalr.CONNECTION_HUB.on('Online', (users) => {
        this.online = users;
      });

      signalr.CONNECTION_HUB.on('UserOnline', (userNames: any) => {
        const userNameList = JSON.stringify(userNames);
        localStorage.setItem('userOnline', userNameList);
      });
    }
  }
  openModal(ref) {
    this.modalReference = this.modalService.open(ref, { size: 'xl', backdrop: 'static', keyboard: false });
  }
}
