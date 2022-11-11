import { Subscription } from 'rxjs';
import { Component, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { AuthService } from '../../../_core/_service/auth.service';
import { AlertifyService } from '../../../_core/_service/alertify.service';
import { Router } from '@angular/router';
import { SignalrService } from 'src/app/_core/_service/signalr.service';
import { HeaderService } from 'src/app/_core/_service/header.service';
import { DomSanitizer } from '@angular/platform-browser';
import { CalendarsService } from 'src/app/_core/_service/calendars.service';
import { IHeader } from 'src/app/_core/_model/header.interface';
import * as moment from 'moment';
import { Nav } from 'src/app/_core/_model/nav';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { AvatarModalComponent } from './avatar-modal/avatar-modal.component';
import { TranslateService } from '@ngx-translate/core';
import { RoleService } from 'src/app/_core/_service/role.service';
import { CookieService } from 'ngx-cookie-service';
import { L10n, loadCldr, setCulture, Ajax } from '@syncfusion/ej2-base';
import { DataService } from 'src/app/_core/_service/data.service';
import { PermissionService } from 'src/app/_core/_service/permission.service';
import { AuthenticationService } from 'src/app/_core/_service/authentication.service';
import { NgxSpinnerService } from 'ngx-spinner';
declare var require: any;

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit, AfterViewInit {
  public data: any;
  public navAdmin: any;
  public navClient: any;
  navEc: any;
  public total: number;
  public totalCount: number;
  public page: number;
  public ADMIN = 1;
  public SUPERVISOR = 2;
  public ADMIN_COSTING = 5;
  public STAFF = 3;
  public WORKER = 4;
  public WORKER2 = 6;
  public DISPATCHER = 6;
  public pageSize: number;
  public currentUser: string;
  public currentTime: any;
  userid: number;
  level: number;
  roleName: string;
  role: any;
  avatar: any;
  vi: any;
  en: any;
  langsData: object[];
  public fields = { text: 'name', value: 'id' };
  public value: string;
  zh: any;
  menus: any;
  constructor(
    private authService: AuthService,
    private authenticationService: AuthenticationService,
    private roleService: RoleService,
    private alertify: AlertifyService,
    private permissionService: PermissionService,
    private headerService: HeaderService,
    private calendarsService: CalendarsService,
    private sanitizer: DomSanitizer,
    private router: Router,
    private dataService: DataService,
    private spinner: NgxSpinnerService,
    private cookieService: CookieService,
    private modalService: NgbModal,
    public translate: TranslateService

  ) {
    this.role = JSON.parse(localStorage.getItem('level'));
    const lang = localStorage.getItem('lang');
    if (lang) {
      this.value = lang;
      translate.setDefaultLang(lang);
      translate.use(lang);

    } else {
      this.value = 'vi';
      translate.setDefaultLang('vi');
      translate.use('vi');
    }
  }

  ngOnInit(): void {
    this.vi = require('../../../../assets/ej2-lang/vi.json');
    this.en = require('../../../../assets/ej2-lang/en-US.json');
    this.langsData = [{ id: 'vi', name: 'VI' }, { id: 'en', name: 'EN' }];
    this.navAdmin = new Nav().getNavAdmin();
    this.navClient = new Nav().getNavClient();
    this.navEc = new Nav().getNavEc();
    // this.checkTask();
    this.getAvatar();
    this.currentUser = JSON.parse(localStorage.getItem('user')).user.username;
    this.page = 1;
    this.pageSize = 10;
    // this.signalrService.startConnection();
    this.userid = JSON.parse(localStorage.getItem('user')).user.id;
    this.getMenu();
    this.onService();
    this.currentTime = moment().format('LTS');
    setInterval(() => this.updateCurrentTime(), 1 * 1000);
  }
  ngAfterViewInit() {
    this.getBuilding();
    const img = localStorage.getItem('avatar');
    if (img === 'null') {
      this.avatar = this.defaultImage();
    } else {
      this.avatar = this.sanitizer.bypassSecurityTrustResourceUrl('data:image/png;base64, ' + img);
    }
  }

  getMenu() {
    if (localStorage.getItem('menus') === `undefined`) {
      this.spinner.show();
      console.log('Header ------- Begin getMenuByUserPermission');
      this.permissionService.getMenuByUserPermission(this.userid).subscribe((data: []) => {
        this.menus = data;
        localStorage.setItem('menus', JSON.stringify(data));
        this.spinner.hide();

      }, (err) => {
        this.spinner.hide();
      });
      console.log('Header ------- end getMenuByUserPermission');
    } else {
      setTimeout(() => {
        console.log('Header ------- Begin getlocalstore menu');
        const menus = JSON.parse(localStorage.getItem('menus')) || [];
        this.menus = menus;
      });
    }
  }
  home() {
    if (this.role.id === this.STAFF) {
      return '/ec/execution/todolist-2';
    } else {
      return'/ec/execution/todolist-2';
    }
  }
  onChange(args) {
    if (args.itemData) {
      localStorage.removeItem('lang');
      localStorage.setItem('lang', args.itemData.id);
      this.translate.use(args.itemData.id);
      if (args.itemData.id === 'vi') {
        this.dataService.setValueLocale(args.itemData.id);
        setTimeout(() => {
          L10n.load(this.vi);
          setCulture('vi');
        }, 500);
        location.reload();
      } else {
        this.dataService.setValueLocale(args.itemData.id);
        setTimeout(() => {
          L10n.load(this.en);
          setCulture('en-US');
          location.reload();
        }, 500);
      }
    }
  }
  getBuilding() {
    const userID = JSON.parse(localStorage.getItem('user')).user.id;
    this.roleService.getRoleByUserID(userID).subscribe((res: any) => {
      res = res || {};
      if (res !== {}) {
        this.level = res.id;
      }
    });
  }
  onService() {
    this.headerService.currentImage
      .subscribe(arg => {
        if (arg) {
          this.changeAvatar(arg);
        }
      });
  }
  changeAvatar(avt) {
    let avatar;
    if (avt) {
      avatar = avt.replace('data:image/png;base64,', '').trim();
      localStorage.removeItem('avatar');
      localStorage.setItem('avatar', avatar);
      this.getAvatar();
    } else {
      this.avatar = this.defaultImage();
    }

  }
  onScrollDown() {
    if (this.pageSize >= 200) {
      this.pageSize -= 10;
    } else {
      this.pageSize += 10;

    }
  }

  onScrollUp() {
    if (this.pageSize >= 200) {
      this.pageSize -= 10;

    } else {
      this.pageSize += 10;

    }
  }
  updateCurrentTime() {
    this.currentTime = moment().format('LTS');
  }
  logout() {
    this.cookieService.deleteAll();
    localStorage.clear();
    this.authService.decodedToken = null;
    this.authService.currentUser = null;
    this.authenticationService.logOut();
    const uri = this.router.url;
    this.router.navigate(['login'], { queryParams: { uri }, replaceUrl: true  });
    this.alertify.message('Logged out');

  }
  openAvatarModal() {
    const modalRef = this.modalService.open(AvatarModalComponent, { size: 'lg' });
    modalRef.componentInstance.title = 'Add Routine Main Task';
    // modalRef.componentInstance.user = 1;
    modalRef.result.then((result) => {
    }, (reason) => {
    });
  }

  pushToMainPage() {
    const role = JSON.parse(localStorage.getItem('user')).user.role;
    if (role === 1) {
      this.router.navigate(['/admin/dash']);
    } else if (role === 2) {
      this.router.navigate(['/todolist']);
    }
  }

  defaultImage() {
    return this.sanitizer.bypassSecurityTrustResourceUrl(`data:image/png;base64, iVBORw0KGgoAAAANSUhEUgAAAJYAA
      ACWBAMAAADOL2zRAAAAG1BMVEVsdX3////Hy86jqK1+ho2Ql521ur7a3N7s7e5Yhi
      PTAAAACXBIWXMAAA7EAAAOxAGVKw4bAAABAElEQVRoge3SMW+DMBiE4YsxJqMJtH
      OTITPeOsLQnaodGImEUMZEkZhRUqn92f0MaTubtfeMh/QGHANEREREREREREREtIJ
      J0xbH299kp8l8FaGtLdTQ19HjofxZlJ0m1+eBKZcikd9PWtXC5DoDotRO04B9YOvF
      IXmXLy2jEbiqE6Df7DTleA5socLqvEFVxtJyrpZFWz/pHM2CVte0lS8g2eDe6prOy
      qPglhzROL+Xye4tmT4WvRcQ2/m81p+/rdguOi8Hc5L/8Qk4vhZzy08DduGt9eVQyP
      2qoTM1zi0/uf4hvBWf5c77e69Gf798y08L7j0RERERERERERH9P99ZpSVRivB/rgAAAABJRU5ErkJggg==`);
  }
  getAvatar() {
    const img = localStorage.getItem('avatar');
    if (img === 'null') {
      this.avatar = this.defaultImage();
    } else {
      this.avatar = this.sanitizer.bypassSecurityTrustResourceUrl('data:image/png;base64, ' + img);
    }
  }
  imageBase64(img) {
    if (img === 'null') {
      return this.defaultImage();
    } else {
      return this.sanitizer.bypassSecurityTrustResourceUrl('data:image/png;base64, ' + img);
    }
  }
  datetime(d) {
    return this.calendarsService.JSONDateWithTime(d);
  }
  checkTask() {
    this.headerService.checkTask(this.userid)
      .subscribe(() => {});
  }
  seen(item) {
    this.headerService.seen(item).subscribe(res => {
      this.page = 1;
      this.data = [];
    });
    const obj: IHeader = {
      router: item.URL.split('/')[1],
      message: item.URL.split('/')[2],
    };
    if (obj.router === 'project-detail') {
      this.router.navigate([item.URL.replace('project-detail', 'project/detail')]);
    } else {
      const url = `/${obj.router}`;
      this.router.navigate([item.URL]);
    }
  }
}
