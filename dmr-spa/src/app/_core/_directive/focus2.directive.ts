import { Directive, AfterViewInit, ElementRef, Input, OnChanges, HostListener, OnDestroy, OnInit } from '@angular/core';
import { Subject, Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';

@Directive({
  // tslint:disable-next-line:directive-selector
  selector: '[autofocus]'
})
export class Autofocus2Directive implements AfterViewInit, OnInit, OnDestroy {
  @Input('autofocus') autofocusStatus: boolean;

  subject = new Subject<string>();
  subscription: Subscription[] = [];
  @HostListener('ngModelChange', ['$event']) onChange(value) {
    this.subject.next(value);
  }
  @HostListener('focusout') onFocusout() {
    setTimeout(() => {
      this.host.nativeElement.focus();
      this.host.nativeElement.select();
    }, 5000);
  }
  @HostListener('focus') onBlur() {
    setTimeout(() => {
      this.host.nativeElement.select();
    }, 300);
  }

  constructor(private host: ElementRef<HTMLInputElement>) { }
  ngOnInit() {
    this.subscription.push(this.subject
      .pipe(debounceTime(1000))
      .subscribe(async (arg) => {
        this.host.nativeElement.select();
      }));
  }
  ngOnDestroy() {
    this.subscription.forEach(item => item.unsubscribe());
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
