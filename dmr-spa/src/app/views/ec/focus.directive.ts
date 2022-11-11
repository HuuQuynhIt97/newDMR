import { Directive, AfterViewInit, ElementRef, Input, OnChanges, HostListener } from '@angular/core';

@Directive({
  // tslint:disable-next-line:directive-selector
  selector: '[autofocus]'
})
export class AutofocusDirective implements AfterViewInit {
  @Input('autofocus') autofocusStatus: boolean;
  @HostListener('focusout') onFocusout() {
    setTimeout( () => {
      this.host.nativeElement.focus();
      this.host.nativeElement.select();
    }, 5000);
  }
  @HostListener('focus') onBlur() {
    setTimeout( () => {
      this.host.nativeElement.select();
    }, 300);
  }

  constructor(private host: ElementRef<HTMLInputElement>) { }
  ngAfterViewInit() {
    if (this.autofocusStatus === true) {
      setTimeout(() => {
        this.host.nativeElement.focus();
        this.host.nativeElement.select();
      }, 300);
    }
  }
}
