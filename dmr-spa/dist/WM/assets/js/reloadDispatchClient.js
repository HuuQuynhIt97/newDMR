import * as signalR from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

export const CONNECTION_HUB = new HubConnectionBuilder()
    .withUrl(environment.hub)
    .withAutomaticReconnect([1000, 3000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Information)
    .build();
// Start the connection.
start();
function start() {
    CONNECTION_HUB.start().then(function () {

        CONNECTION_HUB.on('UserConnected', (conId) => {
            console.log("UserConnected reloadDispatchClient", conId);
        });
        CONNECTION_HUB.on('UserDisconnected', (conId) => {
            console.log("UserDisconnected reloadDispatchClient", conId);

        });
        console.log("Signalr connected reloadDispatchClient");
    }).catch(function (err) {
        setTimeout(() => start(), 5000);
    });
}
