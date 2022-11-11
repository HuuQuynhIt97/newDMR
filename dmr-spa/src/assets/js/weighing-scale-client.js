import * as signalR from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

export const CONNECTION_WEIGHING_SCALE_HUB = new HubConnectionBuilder()
    .withUrl(environment.scalingHubLocal)
    .withAutomaticReconnect([1000, 3000, 5000, 10000, 30000])
    // .configureLogging(signalR.LogLevel.Information)
    .build();
// Start the connection.
// start();
function start() {
    CONNECTION_WEIGHING_SCALE_HUB.start().then(function () {

        CONNECTION_WEIGHING_SCALE_HUB.on('UserConnected', (conId) => {
            console.log("CONNECTION_WEIGHING_SCALE_HUB UserConnected", conId);
        });
        CONNECTION_WEIGHING_SCALE_HUB.on('UserDisconnected', (conId) => {
            console.log("CONNECTION_WEIGHING_SCALE_HUB UserDisconnected", conId);

        });
        console.log("Signalr CONNECTION_WEIGHING_SCALE_HUB connected");
    }).catch(function (err) {
        setTimeout(() => start(), 5000);
    });
}