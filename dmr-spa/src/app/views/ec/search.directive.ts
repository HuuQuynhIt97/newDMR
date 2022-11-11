import { Directive, AfterViewInit, ElementRef, Input, OnChanges, HostListener, OnInit, OnDestroy, Output, EventEmitter } from '@angular/core';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, finalize } from 'rxjs/operators';

@Directive({
  // tslint:disable-next-line:directive-selector
  selector: '[autofocusSearch]'
})
export class SearchDirective implements AfterViewInit, OnInit, OnDestroy {
  subject = new Subject<string>();
  subscription: Subscription[] = [];
  regexStr = '^[a-zA-Z0-9_]*$';
  isShow: boolean;
  @Output() messageEvent = new EventEmitter<boolean>();
  lastValue: boolean;
  @HostListener('focus') onFocus() {
    setTimeout(() => {
      this.host.nativeElement.select();
    }, 300);
  }
  @HostListener('focusout') onFocusout() {
    setTimeout(() => {
      this.host.nativeElement.focus();
    }, 5000);
  }
  @HostListener('ngModelChange', ['$event']) onChange(value) {
    this.isShow = true;
    this.messageEvent.emit(true);
    this.subject.next(value);
  }
  @HostListener('document:keydown', ['$event']) onKeyDown(event: KeyboardEvent) {
    if (event.keyCode === 74) {
      event.preventDefault();
    }
  }
  constructor(private host: ElementRef) { }
  ngAfterViewInit() {
    // document.addEventListener('keydown', (event) => {
    //   if (event.keyCode === 13 || event.keyCode === 17 || event.keyCode === 74) {
    //     event.preventDefault();
    //   }
    // });
    setTimeout(() => {
      this.host.nativeElement.focus();
    }, 500);
  }
  ngOnInit() {
    this.subscription.push(this.subject
      .pipe(
        debounceTime(50)
      )
      .subscribe(async (args) => {
        this.host.nativeElement.select();
        // console.log(args);
        this.messageEvent.emit(false);
        this.isShow = false;
      }));
  }
  ngOnDestroy() {
    this.subscription.forEach(item => item.unsubscribe());
  }
  @HostListener('document:keydown.enter', ['$event'])
  onKeydownHandler(event: KeyboardEvent) {
    event.preventDefault();
    this.host.nativeElement.value = this.host.nativeElement.value + '    ';
  }

  @HostListener('document:keydown.tab', ['$event'])
  onKeydownTabHandler(event: KeyboardEvent) {
    event.preventDefault();
    this.host.nativeElement.value = this.host.nativeElement.value + '    ';
  }
}
