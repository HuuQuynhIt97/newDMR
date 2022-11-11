import { Directive, AfterViewInit, ElementRef, Input, OnChanges, HostListener, OnInit } from '@angular/core';

@Directive({
  // tslint:disable-next-line:directive-selector
  selector: '[autofocussubpackage]'
})
export class AutofocusSubpackageDirective implements AfterViewInit, OnInit {
  @Input('autofocussubpackage') autofocusStatus: boolean;
  // @HostListener('focusout') onFocusout() {
  //   setTimeout( () => {
  //     this.host.nativeElement.focus();
  //     this.host.nativeElement.select();
  //   }, 5000);
  // }
  @HostListener('focus') onBlur() {
    setTimeout( () => {
      console.log(this.autofocusStatus);
      this.host.nativeElement.select();
    }, 300);
  }

  constructor(private host: ElementRef<HTMLInputElement>) { }
  ngOnInit(): void {
    if (this.autofocusStatus === true) {
      setTimeout(() => {
        this.host.nativeElement.focus();
        this.host.nativeElement.select();
      }, 300);
    }
  }
  ngAfterViewInit() {
    if (this.autofocusStatus === true) {
      setTimeout(() => {
        this.host.nativeElement.focus();
        this.host.nativeElement.select();
      }, 300);
    }
  }
}
