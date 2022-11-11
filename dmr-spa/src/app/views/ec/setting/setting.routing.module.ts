import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from 'src/app/_core/_guards/auth.guard';
import {
  AccountComponent, BuildingComponent, BuildingLunchTimeComponent,
  BuildingSettingComponent, CostingComponent, DecentralizationComponent,
  GlueTypeComponent,
  IngredientComponent, KindComponent, LunchTimeComponent, MailingComponent,
  MaterialComponent, PartComponent, PrintQRCodeComponent, ScalingSettingComponent,
  SubpackageCapacityComponent, SuppilerComponent,
  RoleComponent, PrivilegeComponent
} from '.';
const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: 'account',
        component: AccountComponent,
        data: {
          title: 'Account',
          breadcrumb: 'Account',
          functionCode: 'Account'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'building',
        component: BuildingComponent,
        data: {
          title: 'Building',
          breadcrumb: 'Building',
          functionCode: 'Building'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'supplier',
        component: SuppilerComponent,
        data: {
          title: 'Supplier',
          breadcrumb: 'Supplier',
          functionCode: 'Supplier Menu'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'chemical',
        data: {
          title: 'Chemical',
          breadcrumb: 'Chemical',
          functionCode: 'Chemical'
        },
        canActivate: [AuthGuard],
        children: [
          {
            path: '',
            component: IngredientComponent,
          },
          {
            path: 'print-qrcode/:id/:code/:name',
            component: PrintQRCodeComponent,
            data: {
              title: 'Print QRCode',
              breadcrumb: 'Print QRCode'
            }
          }
        ]
      },
      {
        path: 'kind',
        component: KindComponent,
        data: {
          title: 'Kind',
          breadcrumb: 'Kind',
          functionCode: 'Kind'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'part',
        component: PartComponent,
        data: {
          title: 'Part',
          breadcrumb: 'Part',
          functionCode: 'Part'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'material',
        component: MaterialComponent,
        data: {
          title: 'Material',
          breadcrumb: 'Material',
          functionCode: 'Material'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'glue-type',
        component: GlueTypeComponent,
        data: {
          title: 'Glue Type',
          breadcrumb: 'Glue Type',
          functionCode: 'Glue Type'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'building-setting',
        component: BuildingSettingComponent,
        data: {
          title: 'RPM IoT',
          breadcrumb: 'RPM IoT',
          functionCode: 'RPM IoT'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'building-lunch-time',
        component: BuildingLunchTimeComponent,
        data: {
          title: 'Period',
          breadcrumb: 'Period',
          functionCode: 'Period'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'decentralization',
        component: DecentralizationComponent,
        data: {
          title: 'Decentralization',
          breadcrumb: 'Decentralization'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'scaling-setting',
        component: ScalingSettingComponent,
        data: {
          title: 'Scaling Setting',
          breadcrumb: 'Scaling Setting'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'mailing',
        component: MailingComponent,
        data: {
          title: 'Mailing',
          breadcrumb: 'Mailing'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'lunch-time',
        component: LunchTimeComponent,
        data: {
          title: 'Lunch Time',
          breadcrumb: 'Lunch Time'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'subpackage-capacity',
        component: SubpackageCapacityComponent,
        data: {
          title: 'Subpackage Capacity',
          breadcrumb: 'Subpackage Capacity'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'costing',
        component: CostingComponent,
        data: {
          title: 'costing',
          breadcrumb: 'Costing'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'role',
        data: {
          title: 'Role',
          functionCode: 'Role'
        },
        canActivate: [AuthGuard],
        children: [
          {
            path: '',
            component: RoleComponent
          },
          {
            path: 'privilege/:id',
            component: PrivilegeComponent,
            data: {
              title: 'Privilege'
            }
          }
        ]
      }
    ]
  }
];
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ECRoutingModule { }
