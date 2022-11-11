import { Component, OnDestroy, OnInit } from '@angular/core';
import { L10n, loadCldr, setCulture, Ajax } from '@syncfusion/ej2-base';
import { Subscription } from 'rxjs';
declare var require: any;
loadCldr(
  require('cldr-data/supplemental/numberingSystems.json'),
  require('cldr-data/main/en/ca-gregorian.json'),
  require('cldr-data/main/en/numbers.json'),
  require('cldr-data/main/en/timeZoneNames.json'),
  require('cldr-data/supplemental/weekdata.json')); // To load the culture based first day of week

loadCldr(
  require('cldr-data/supplemental/numberingSystems.json'),
  require('cldr-data/main/vi/ca-gregorian.json'),
  require('cldr-data/main/vi/numbers.json'),
  require('cldr-data/main/vi/timeZoneNames.json'),
  require('cldr-data/supplemental/weekdata.json')); // To load the culture based first day of week

import { connect, IClientOptions } from "mqtt";
import { environment } from 'src/environments/environment';


@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnDestroy{
  title = 'WM';
  // private subscription: Subscription;
  // public message: string;
  // client: any;
  // constructor() {
  //   const MQTT_SERVICE_OPTIONS: IClientOptions = {
  //     hostname: "localhost",
  //     port:  5000,
  //     protocol: "ws",
  //     path: '/mqtt',
  //     clientId: "stir_machine_client"
  //   };
  //   this.client = connect("localhost:5000", MQTT_SERVICE_OPTIONS);
  //   this.client.on('connect', () => {
  //     this.client.subscribe('#', { qos: 0 }, (err, granted) => {
  //       console.log(err);
  //     });
  //     this.client.publish('presence', 'Hello mqtt');
  //     console.log("[connect]");
  //   });
  //   this.client.on('message', (topic, message) => {
  //     console.log(topic + ": " + message.toString());
  //   });
  // }
  // public unsafePublish(topic: string, message: string): void {
  //   //this.mqttService.unsafePublish(topic, message, { qos: 1, retain: true });
  // }

  public ngOnDestroy() {
    // this.subscription.unsubscribe();
  }
}
