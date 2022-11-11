import { Injectable, OnDestroy } from '@angular/core';
// import * as signalR from '@aspnet/signalr';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MixingService implements OnDestroy {
  numberOfAttempts: number;
  public hubConnection: HubConnection;
  private connectionUrl = environment.scalingHubLocal;
  private counter = 0;
  receiveAmount: BehaviorSubject<{ weighingScaleID: any, amount: any, unit: any }>;
  constructor() {
    this.counter++;
    console.log(this.counter);
    this.numberOfAttempts = 0;
    this.receiveAmount = new BehaviorSubject<{ weighingScaleID: any, amount: any, unit: any }>(null);
  }

  ngOnDestroy() {
    console.log('Mixing Service destroy');
    this.numberOfAttempts = 0;
  }

  public connect = () => {
    this.startConnection();
  }
  public close = async () => {
    try {
      await this.hubConnection.stop();
      console.log('Mixing service stoped hub');
    } catch (error) {
      console.log('Mixing service Cant not stop hub', error);
    }
  }

  startConnection = async () => {
    this.numberOfAttempts = this.numberOfAttempts + 1;
    this.hubConnection = new HubConnectionBuilder()
      .configureLogging(LogLevel.Error)
      .withUrl(this.connectionUrl)
      .build();
    this.setSignalrClientMethods();
    try {
      await this.hubConnection.start();
      console.log('MixingService connected hub');

    } catch (error) {
      alert('Không thể kết nối tới cân! Vui lòng liên hệ administrator!');
      // setTimeout(async () => {
      //   if (this.numberOfAttempts === 5) {
      //     this.numberOfAttempts = 0;
      //     console.log(`mixing service cant not connected hub: ${error}`, this.numberOfAttempts);
      //     alert('Đã kết nối lại cân 5 lần nhưng không được! Vui lòng liên hệ administrator!');
      //     return;
      //   }
      //   await this.startConnection();
      // }, 5000);
    }
  }
  public offWeighingScale() {
    this.hubConnection.off('Welcom');
  }
  // This method will implement the methods defined in the ISignalrDemoHub inteface in the SignalrDemo.Server .NET solution
  private setSignalrClientMethods(): void {
    this.hubConnection.onreconnected(() => {
      console.log('Mixing service Restarted signalr!');
    });

    this.hubConnection.on('Welcom', (weighingScaleID, amount, unit) => {
      this.receiveAmount.next({ weighingScaleID, amount, unit });
    });

    this.hubConnection.on('UserConnected', (conId) => {
      console.log('Mixing service', conId);
    });

    this.hubConnection.on('UserDisconnected', (conId) => {
      console.log('Mixing service UserDisconnected', conId);

    });

  }
}
