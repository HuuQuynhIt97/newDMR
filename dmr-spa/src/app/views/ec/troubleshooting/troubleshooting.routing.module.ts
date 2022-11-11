import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { AbnormalListComponent } from "./abnormal-list/abnormal-list.component";
import { SearchComponent } from "./search/search.component";

const routes: Routes = [
  {
    path: '',
    data: {
      title: 'Troubleshooting',
      breadcrumb: 'Troubleshooting'
    },
    children: [
      {
        path: 'search',
        component: SearchComponent,
        data: {
          title: 'Troubleshooting Search',
          breadcrumb: 'Search'
        }
      },
      {
        path: 'Abnormal-List',
        component: AbnormalListComponent,
        data: {
          title: 'Troubleshooting Black List',
          breadcrumb: 'Abnormal-List'
        }
      },
    ]
  },
];
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TroubleshootingRoutingModule { }
