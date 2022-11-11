import * as signalR from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

export const CONNECTION_HUB = new HubConnectionBuilder()
    .withUrl(environment.hub)
    .withAutomaticReconnect([1000, 3000, 5000, 10000, 30000])
    // .configureLogging(signalR.LogLevel.Information)
    .build();
// Start the connection.
start();
function start() {
    CONNECTION_HUB.start().then(function () {

        CONNECTION_HUB.on('UserConnected', (conId) => {
            console.log("UserConnected", conId);
        });
        CONNECTION_HUB.on('UserDisconnected', (conId) => {
            console.log("UserDisconnected", conId);

        });
        invoke();
        console.log("Signalr connected");
    }).catch(function (err) {
        setTimeout(() => start(), 5000);
    });
}
function invoke() {
    CONNECTION_HUB
        .invoke('JoinReloadDispatch')
        .catch(error => {
            console.log(`JoinReloadDispatch error: ${error}`);
        }
        );

    CONNECTION_HUB
        .invoke('JoinReloadTodo')
        .catch(error => {
            console.log(`JoinReloadTodo error: ${error}`);
        }
        );


}
// CONNECTION_HUB.onclose((error) => {
//     CONNECTION_HUB.start();
//     console.error(`Something went wrong: ${error}`);
// });
// export const SCALING_CONNECTION_HUB = new HubConnectionBuilder()
//     .withUrl(environment.scalingHub)
//     .withAutomaticReconnect([1000, 3000, 5000, 10000, 30000])
//     //  .configureLogging(LogLevel.Information)
//     .build();
// // Start the connection.
// function startScalingHub() {
//     SCALING_CONNECTION_HUB.start().then(function () {
//         SCALING_CONNECTION_HUB.on('Scaling Hub UserConnected', (conId) => {
//             console.log("Scaling Hub UserConnected", conId);

//         });
//         SCALING_CONNECTION_HUB.on('Scaling Hub User Disconnected', (conId) => {
//             console.log("Scaling Hub User Disconnected", conId);

//         });

//         console.log("Scaling Hub Signalr connected");
//     }).catch(function (err) {
//         setTimeout(() => startScalingHub(), 5000);
//     });
// }
// startScalingHub();
