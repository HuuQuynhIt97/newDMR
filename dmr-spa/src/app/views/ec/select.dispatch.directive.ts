import { Directive, AfterViewInit, ElementRef, Input, OnChanges, HostListener } from '@angular/core';
import { IDispatch } from 'src/app/_core/_model/plan';

@Directive({
  // tslint:disable-next-line:directive-selector
  selector: '[autoselectdispatch]'
})
export class AutoSelectDispatchDirective implements AfterViewInit {
  @Input('autoselectdispatch') data: IDispatch;
  @HostListener('focus') onFocus() {
    setTimeout( () => {
      if (this.data.scanStatus === false) { return; }
      if (this.data.scanStatus === true) {
        this.host.nativeElement.select();
        this.host.nativeElement.focus();
      } else {
        this.host.nativeElement.blur();
      }
    }, 0);
  }
  @HostListener('blur') onBlur() {
    setTimeout(() => {
      this.data.scanStatus = false;
    }, 0);
  }
  @HostListener('change', ['$event']) onChange(args) {
    const amount = this.data.standardAmount * 1000;
    if (this.data.scanStatus === true && amount === +args.target.value) {
      this.host.nativeElement.blur();
    }
  }
  constructor(private host: ElementRef<HTMLInputElement>) { }
  ngAfterViewInit() {
    setTimeout(() => {
      if (this.data.scanStatus === false) {
        this.host.nativeElement.blur();
      } else {
        this.host.nativeElement.select();
        this.host.nativeElement.focus();
      }
    }, 300);
  }
}
