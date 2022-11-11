import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { AuthGuard } from "src/app/_core/_guards/auth.guard";
import {
  GlueHistoryComponent, IncomingComponent, MixingComponent,
  PlanComponent, PlanOutputQuantityComponent, ShakeComponent,
  StirComponent, TodolistComponent,
  DispatchComponent} from ".";
const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: 'todolist',
        data: {
          title: 'todolist',
          breadcrumb: 'Todolist'
        },
        canActivate: [AuthGuard],
        children: [
          {
            path: '',
            component: TodolistComponent,
          },
          {
            path: 'stir',
            component: StirComponent,
            data: {
              title: 'Stir',
              breadcrumb: 'Stir'
            }
          },
          {
            path: 'stir/:glueName',
            component: StirComponent,
            data: {
              title: 'Stir',
              breadcrumb: 'Stir'
            }
          },
          {
            path: 'history/:glueID',
            component: GlueHistoryComponent,
            data: {
              title: 'History',
              breadcrumb: 'History'
            }
          },
        ]
      },
      {
        path: 'todolist-2',
        data: {
          title: 'todolist-2',
          breadcrumb: 'To Do List 2',
          functionCode: 'To Do List 2'
        },
        canActivate: [AuthGuard],
        children: [
          {
            path: '',
            component: TodolistComponent,
          },
          {
            path: ':tab',
            component: TodolistComponent,
          },
          {
            path: ':tab/:glueName',
            component: TodolistComponent,
          },
          {
            path: ';tab=:tab;glueName=:glueName',
            component: TodolistComponent,
          },
          {
            path: 'stir',
            component: StirComponent,
            data: {
              title: 'Stir',
              breadcrumb: 'Stir'
            }
          },
          {
            path: 'stir/:tab/:mixingInfoID/:glueName',
            component: StirComponent,
            data: {
              title: 'Stir',
              breadcrumb: 'Stir'
            }
          },
          {
            path: 'mixing',
            component: MixingComponent,
            data: {
              title: 'Mixing',
              breadcrumb: 'Mixing'
            }
          },
          {
            path: 'mixing/:tab/:glueID/:estimatedStartTime/:estimatedFinishTime/:stdcon',
            component: MixingComponent,
            data: {
              title: 'Mixing',
              breadcrumb: 'Mixing'
            }
          },
          {
            path: 'shake',
            component: ShakeComponent,
            data: {
              title: 'Shake',
              breadcrumb: 'Shake'
            }
          },
          {
            path: 'shake/:tab/:mixingInfoID',
            component: ShakeComponent,
            data: {
              title: 'Shake',
              breadcrumb: 'Shake'
            }
          },
          {
            path: 'dispatch',
            component: DispatchComponent,
            data: {
              title: 'dispatch',
              breadcrumb: 'Dispatch'
            }
          },
          {
            path: 'dispatch/:mixingInfoID',
            component: DispatchComponent,
            data: {
              title: 'dispatch',
              breadcrumb: 'Dispatch'
            }
          },
        ]
      },
      {
        path: 'workplan',
        component: PlanComponent,
        data: {
          title: 'Workplan',
          breadcrumb: 'Work Plan',
          functionCode: 'Work Plan'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'output-quantity',
        component: PlanOutputQuantityComponent,
        data: {
          title: 'Output Quantity',
          breadcrumb: 'Output Quantity',
          functionCode: 'Output Quantity'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'incoming',
        component: IncomingComponent,
        data: {
          title: 'Stock',
          breadcrumb: 'Stock',
          functionCode: 'Stock'
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
export class ExecutionRoutingModule { }
