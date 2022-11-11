import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { AuthGuard } from "src/app/_core/_guards/auth.guard";
import { BPFCScheduleComponent, BpfcDetailComponent,
  BpfcStatusComponent, Bpfc1Component } from ".";
import { BpfcDetailV2Component } from "./bpfc-detailv2/bpfc-detailv2.component";
const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: 'bpfc-schedule',
        data: {
          title: 'bpfc-schedule',
          breadcrumb: 'BPFC Schedule',
          functionCode: 'BPFC Schedule'
        },
        canActivate: [AuthGuard],
        children: [
          {
            path: '',
            component: BPFCScheduleComponent
          },
          {
            path: ':tab',
            data: {
              title: 'bpfc-schedule',
              breadcrumb: 'BPFC Schedule',
              functionCode: 'BPFC Schedule'
            },
            component: BPFCScheduleComponent
          },
          {
            path: ':tab/:keySearch',
            data: {
              title: 'bpfc-schedule',
              breadcrumb: 'BPFC Schedule',
              functionCode: 'BPFC Schedule'
            },
            component: BPFCScheduleComponent
          },
          {
            path: ':tab/detail',
            data: {
              title: 'Detail',
              breadcrumb: 'Detail'
            },
            children: [
              {
                path: '',
                component: BpfcDetailComponent,
              },
              {
                path: ':id',
                component: BpfcDetailComponent,
              }
            ]
          },
          {
            path: ':tab/detailv2',
            data: {
              title: 'Detail',
              breadcrumb: 'Detail'
            },
            children: [
              {
                path: '',
                component: BpfcDetailV2Component,
              },
              {
                path: ':id',
                component: BpfcDetailV2Component,
              }
            ]
          },


        ]
      },
      {
        path: 'bpfc-status',
        component: BpfcStatusComponent,
        data: {
          title: 'BPFC Status',
          breadcrumb: 'BPFC Status',
          functionCode: 'BPFC Status'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'bpfc-1',
        component: Bpfc1Component,
        data: {
          title: 'BPFC ',
          breadcrumb: 'BPFC',
          functionCode: 'BPFC'
        },
        canActivate: [AuthGuard]
      }
    ]
  }
];
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EstablishRoutingModule { }
