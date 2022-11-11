import { Injectable, OnInit } from '@angular/core';
// import * as signalR from '@aspnet/signalr';
import { HttpTransportType, HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SignalrTodolistService {
  public hubConnection: HubConnection;
  private connectionUrl = environment.hub;
  private counter = 0;
  reloadDispatch: BehaviorSubject<any>;
  reloadTodo: BehaviorSubject<any>;
  receiveTodolist: BehaviorSubject<any>;
  constructor() {
    this.counter++;
    console.log('SignalrTodolistService', this.counter);
    this.reloadDispatch = new BehaviorSubject<any>(null);
    this.reloadTodo = new BehaviorSubject<any>(null);
    this.receiveTodolist = new BehaviorSubject<any>(null);
    this.connect();
  }
  public connect = () => {
    this.startConnection();
  }
  public close = async () => {
    try {
      await this.hubConnection.stop();
      console.log('todolist service stoped hub');
    } catch (error) {
      console.log('todolist service Cant not stop hub', error);
    }
  }

  startConnection = async () => {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.connectionUrl, { transport: HttpTransportType.WebSockets })
      .build();
    this.setSignalrClientMethods();
    try {
      await this.hubConnection.start();
      console.log('todolist service signalr connected!');

      this.hubConnection
        .invoke('JoinReloadDispatch')
        .catch(error => {
          console.log(`JoinReloadDispatch error: ${error}`);
        }
        );

      this.hubConnection
        .invoke('JoinReloadTodo')
        .catch(error => {
          console.log(`JoinReloadTodo error: ${error}`);
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
      console.log('client todolist reconnected', '');
    });

    this.hubConnection.on('ReloadDispatch', (message: any) => {
      this.reloadDispatch.next(message);
    });

    this.hubConnection.on('ReloadTodo', (percentage: any) => {
      this.reloadTodo.next(percentage);
    });

    this.hubConnection.on('ReceiveTodolist', (message: any) => {
      this.receiveTodolist.next(message);
    });
  }
}
