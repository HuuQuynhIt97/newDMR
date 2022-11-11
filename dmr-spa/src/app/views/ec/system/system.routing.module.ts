import { VersionAddComponent } from './version/version-add/version-add.component';
import { ActionFunctionComponent } from './action-function/action-function.component';
import { FunctionComponent } from './function/function.component';
import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { ModuleComponent } from "./module/module.component";
import { ActionComponent } from './action/action.component';
import { VersionComponent } from './version/version.component';
import { AuthGuard } from 'src/app/_core/_guards/auth.guard';

const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: 'module',
        component: ModuleComponent,
        data: {
          title: 'Module',
          functionCode: 'Module'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'function',
        component: FunctionComponent,
        data: {
          title: 'Function',
          functionCode: 'Function'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'action',
        component: ActionComponent,
        data: {
          title: 'Action',
          functionCode: 'Action'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'action-in-function',
        component: ActionFunctionComponent,
        data: {
          title: 'Action In Function',
          functionCode: 'Action In Function'
        },
        canActivate: [AuthGuard]
      },
      {
        path: 'version',
        data: {
          title: 'Verison',
          functionCode: 'Version'
        },
        canActivate: [AuthGuard],
        children: [
          {
            path: '',
            component: VersionComponent
          },
         {
            path: 'add',
            component: VersionAddComponent,
            data: {
              title: 'Add'
            }
         },
          {
            path: 'edit/:id',
            component: VersionAddComponent,
            data: {
              title: 'Edit'
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
