import { Injectable } from '@angular/core';
import { resolve } from 'dns';
declare let Swal: any;
@Injectable({
  providedIn: 'root'
})
export class AlertifyService {
  public $swal = Swal;
  timerInterval: NodeJS.Timeout;
  constructor() { }
  private Toast = Swal.mixin({
    toast: true,
    position: 'bottom-end',
    showConfirmButton: false,
    timer: 3000,
    timerProgressBar: true,
    onOpen: (toast) => {
      toast.addEventListener('mouseenter', Swal.stopTimer);
      toast.addEventListener('mouseleave', Swal.resumeTimer);
    }
  });
  showLoading(timer = 2000) {
    {
      Swal({
        title: 'Now loading',
        allowEscapeKey: false,
        allowOutsideClick: false,
        timer,
        onOpen: () => {
          Swal.showLoading();
        }
      }).then(
        () => { },
        (dismiss) => {
          if (dismiss === 'timer') {
            console.log('closed by timer!!!!');
            Swal({
              title: 'Finished!',
              type: 'success',
              timer: 2000,
              showConfirmButton: false
            });
          }
        }
      );
    }
  }
  confirm(title: string, message: string, okCallback: () => void) {
    Swal.fire({
      title,
      // text: message,
      html: message,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Yes',
      cancelButtonText: 'No'
    }).then((result) => {
      if (result.value) {
        okCallback();
      } else if (result.dismiss === Swal.DismissReason.cancel) {
        Swal.fire(
          'Cancelled',
          'Your imaginary file is safe :)',
          'error'
        );
      }
    });
  }
  confirm2(title: string, message: string, okCallback: () => void, cancelCallback: () => void) {
    Swal.fire({
      title,
      html: message,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Yes',
      cancelButtonText: 'No'
    }).then((result) => {
      if (result.value) {
        okCallback();
      } else if (result.dismiss === Swal.DismissReason.cancel) {
        cancelCallback();
      }
    });
  }
  valid(title: string, message: string): Promise<boolean> {
    return new Promise((res, rejects) => {
      Swal.fire({
        title,
        text: message,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes',
        cancelButtonText: 'No'
      }).then((result) => {
        if (result.value) {
          res(true);
        } else if (result.dismiss === Swal.DismissReason.cancel) {
          rejects(false);
        }
      });
    });
  }
  validation(title: string, message: string) {
    Swal.fire(title, message, 'warning');
  }

  success(message: string, noToast?: boolean) {
    if (!noToast) {
      this.Toast.fire({
        icon: 'success',
        title: message
      });
    } else {
      Swal.fire('Successfully!', message, 'success');
    }
  }

  error(message: string, noToast?: boolean) {
    if (!noToast) {
      this.Toast.fire({
        icon: 'error',
        title: message
      });
    } else {
      Swal.fire('Error!', message, 'error');
    }
  }

  warning(message: string, noToast?: boolean) {
    if (!noToast) {
      this.Toast.fire({
        icon: 'warning',
        title: message
      });
    } else {
      Swal.fire('Warning!', message, 'warning');
    }
  }

  message(message: string, noToast?: boolean) {
    if (!noToast) {
      this.Toast.fire({
        icon: 'info',
        title: message
      });
    } else {
      Swal.fire('Info!', message, 'info');
    }
  }
}
