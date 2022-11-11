import { Injectable } from '@angular/core';
// import * as signalR from '@aspnet/signalr';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  public hubConnection: HubConnection;
  private connectionUrl = environment.hub;

  online: BehaviorSubject<any>;
  userNames: BehaviorSubject<any>;
  userID: number;
  userName: string;
  constructor() {
    this.online = new BehaviorSubject<any>(null);
    this.userNames = new BehaviorSubject<any>(null);
    this.userID = +JSON.parse(localStorage.getItem('user')).User.ID;
    this.userName = JSON.parse(localStorage.getItem('user')).User.Username;
  }
  public connect = () => {
    this.startConnection();
  }
  public close = async () => {
     return await this.hubConnection.stop();
  }

  startConnection = async () => {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.connectionUrl)
      .build();
    this.setSignalrClientMethods();
    try {
      await this.hubConnection.start();
      this.hubConnection
        .invoke('CheckOnline', this.userID, this.userName)
        .catch(error => {
          console.log(`CheckOnline error: ${error}`);
        }
        );

    } catch (error) {
      setTimeout(async () => {
        await this.startConnection();
      }, 5000);
    }
  }
  // This method will implement the methods defined in the ISignalrDemoHub inteface in the SignalrDemo.Server .NET solution
  private setSignalrClientMethods(): void {
    this.hubConnection.onreconnected(() => {
      console.log('Restarted signalr!');
    });
    this.hubConnection.on('Online', (numberOfUser: any) => {
      this.online.next(numberOfUser);
    });
    this.hubConnection.on('UserOnline', (userNames: any) => {
      const userNameList = JSON.stringify(userNames);
      localStorage.setItem('userOnline', userNameList);
      this.userNames.next(userNames);
    });
  }
}
