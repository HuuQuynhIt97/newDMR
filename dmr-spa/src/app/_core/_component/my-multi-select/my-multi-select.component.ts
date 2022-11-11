import { Component, Input, OnInit, Self, ViewChild } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';

@Component({
  selector: 'app-my-multi-select',
  templateUrl: './my-multi-select.component.html',
  styleUrls: ['./my-multi-select.component.css']
})
export class MyMultiSelectComponent implements OnInit, ControlValueAccessor {

  disabled: boolean;
  selectedValues: any;

  @Input() optionItems: any[];
  @ViewChild('combo', { static: true }) combo;
  constructor(@Self() public controlDir: NgControl) {
    this.controlDir.valueAccessor = this;
  }

  ngOnInit(): void {
  }

  toggleCheckAll(values: any) {
    if (values.currentTarget.checked) {
      this.selectAllItems();
    } else {
      this.unselectAllItems();
    }
  }
  onChange(event) {
  }

  onTouched() { }

  onSelectionChange(selectedItems) {
    if (Array.isArray(selectedItems)) {
      const newList = selectedItems.map((x) => x.id);
      this.selectedValues = [...newList];
      this.onChange([...newList]);
    }
    this.onTouched();
  }

  writeValue(obj: any): void {
    this.combo.select([...obj]);
  }
  registerOnChange(fn: any): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }
  setDisabledState?(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  private selectAllItems() {
    const newList = this.optionItems.map((x) => x.id);
    this.selectedValues = [...newList];
    this.onChange([...newList]);
  }

  private unselectAllItems() {
    this.selectedValues = [];
    this.onChange([]);
  }

}
