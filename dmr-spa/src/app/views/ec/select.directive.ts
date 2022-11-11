import { Directive, AfterViewInit, ElementRef, OnDestroy, OnChanges, HostListener, OnInit } from '@angular/core';
import { Subject, Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { IScanner } from 'src/app/_core/_model/IToDoList';

@Directive({
  // tslint:disable-next-line:directive-selector
  selector: '[autoselect]'
})
export class AutoSelectDirective implements AfterViewInit, OnInit, OnDestroy {
  subject = new Subject<string>();
  subscription: Subscription[] = [];
  isShow: boolean;
  @HostListener('focus') onFocus() {
    setTimeout( () => {
      this.host.nativeElement.select();
    }, 300);
  }
  @HostListener('focusout', ['$event']) onFocusout(value) {
    setTimeout(() => {
      this.host.nativeElement.focus();
      this.host.nativeElement.select();
    }, 300);
  }
  @HostListener('ngModelChange', ['$event']) onChange(value) {
    this.subject.next(value);
  }
  constructor(private host: ElementRef ) { }
  ngAfterViewInit() {
    // tslint:disable-next-line:only-arrow-functions
    document.addEventListener('keydown', function(event) {
      if (event.keyCode === 13 || event.keyCode === 17 || event.keyCode === 74) {
        event.preventDefault();
      }
    });
    setTimeout(() => {
      this.host.nativeElement.focus();
    }, 300);
  }
  ngOnInit() {
    this.subscription.push(this.subject
      .pipe(
        debounceTime(1000)
        )
      .subscribe(async (arg) => {
        this.host.nativeElement.focus();
        this.host.nativeElement.select();
      }));
  }
  ngOnDestroy() {
    this.subscription.forEach(item => item.unsubscribe());
  }
  @HostListener('document:keydown.enter', ['$event'])
  onKeydownHandler(event: KeyboardEvent) {
    console.log('document:keydown.enter');
    event.preventDefault();
    this.host.nativeElement.value = this.host.nativeElement.value + '    ';
  }
  @HostListener('document:keydown.tab', ['$event'])
  onKeydownTabHandler(event: KeyboardEvent) {
    console.log('document:keydown.tab');
    event.preventDefault();
    this.host.nativeElement.value = this.host.nativeElement.value + '    ';
  }
  @HostListener('document:keydown', ['$event']) keyDown(event: KeyboardEvent) {
    this.isShow = true;
    if (event.ctrlKey && (event.keyCode === 13 || event.keyCode === 17 || event.keyCode === 74)) {
      event.preventDefault();
    }
    if (event.altKey) {
      event.preventDefault();
    }
  }
}
