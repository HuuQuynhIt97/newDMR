import { ActivatedRoute } from "@angular/router";
import { ActionConstant } from "../_constants";
import { FunctionSystem } from "../_model/application-user";

export abstract class BaseComponent {
  functions: FunctionSystem[];
  editSettingsTree = { allowEditing: false, allowAdding: false, allowDeleting: false, newRowPosition: 'Child', mode: 'Row' };
  editSettings = { showDeleteConfirmDialog: false, allowEditing: false, allowAdding: false, allowDeleting: false, mode: 'Normal' };
  toolbarOptions = ['ExcelExport', 'Add', 'Edit', 'Delete', 'Cancel', 'Search'];
  toolbarOptionsTree = [
  'Add',
  'Delete',
  'Search',
  'ExpandAll',
  'CollapseAll',
  'ExcelExport'
  ];
  contextMenuItems = [
    {
      text: 'Add Child',
      iconCss: ' e-icons e-add',
      target: '.e-content',
      id: 'Add-Sub-Item'
    },
    {
      text: 'Delete',
      iconCss: ' e-icons e-delete',
      target: '.e-content',
      id: 'DeleteOC'
    }
  ];
  protected Permission(route: ActivatedRoute) {
    const functionCode = route.snapshot.data.functionCode;
    this.functions = JSON.parse(localStorage.getItem('functions')).filter(x => x.functionCode === functionCode) || [];
    for (const item of this.functions) {
        const toolbarOptions = [];
        for (const action of item.childrens) {
          const optionItem = this.makeAction(action.code);
          toolbarOptions.push(...optionItem.filter(Boolean));
        }
        toolbarOptions.push('Search');
        const uniqueOptionItem = toolbarOptions.filter((elem, index, self) => {
          return index === self.indexOf(elem);
        });
        this.toolbarOptions = uniqueOptionItem;
    }
  }
  protected PermissionForTreeGrid(route: ActivatedRoute) {
    this.contextMenuItems = [];
    this.functions = JSON.parse(localStorage.getItem('functions'));
    for (const item of this.functions) {
      if (route.snapshot.data.functionCode.includes(item.functionCode)) {
        const toolbarOptionsTree = [];
        for (const action of item.childrens) {
          const optionItem = this.makeActionTreeGrid(action.code);
          toolbarOptionsTree.push(...optionItem.filter(Boolean));
        }
        toolbarOptionsTree.push(...['Search',
          'ExpandAll',
          'CollapseAll',
          'ExcelExport']);
        const uniqueOptionItem = toolbarOptionsTree.filter((elem, index, self) => {
          return index === self.indexOf(elem);
        });
        this.toolbarOptionsTree = uniqueOptionItem;
        break;
      }
    }
  }
  // Đổi action code thanh action của ej2-grid
  protected makeAction(input: string): string[] {
    switch (input) {
      case ActionConstant.CREATE:
        this.editSettings.allowAdding = true;
        return ['Add', 'Cancel'];
      case ActionConstant.EDIT:
        this.editSettings.allowEditing = true;
        return ['Edit', 'Update', 'Cancel'];
      case ActionConstant.DELETE:
        this.editSettings.allowDeleting = true;
        return ['Delete'];
      case ActionConstant.EXCEL_EXPORT:
        return ['ExcelExport'];
      default:
        return [undefined];
    }
  }

  protected makeActionTreeGrid(input: string): string[] {
    switch (input) {
      case ActionConstant.EXCEL_EXPORT:
        return ['ExcelExport'];
      case ActionConstant.CREATE:
        this.editSettingsTree.allowAdding = true;
        this.contextMenuItems.push({
          text: 'Add Child',
          iconCss: ' e-icons e-add',
          target: '.e-content',
          id: 'Add-Sub-Item'
        });
        return ['Add'];
      case ActionConstant.EDIT:
        this.editSettingsTree.allowEditing = true;
        return [undefined];
      case ActionConstant.DELETE:
        this.editSettingsTree.allowDeleting = true;
        this.contextMenuItems.push({
          text: 'Delete',
          iconCss: ' e-icons e-delete',
          target: '.e-content',
          id: 'DeleteOC'
        });
        return [undefined];
      default:
        return [undefined];
    }
  }
}
