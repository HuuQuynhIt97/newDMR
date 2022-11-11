"use strict";
// This file can be replaced during build by using the `fileReplacements` array.
// `ng build --prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.
Object.defineProperty(exports, "__esModule", { value: true });
exports.environment = void 0;
var SYSTEM_CODE = 3;
exports.environment = {
    production: false,
    systemCode: SYSTEM_CODE,
    apiUrlEC: 'http://10.4.4.224:1009/api/',
    apiUrl: 'http://10.4.5.174:108/api/',
    apiUrl2: 'http://10.4.5.174:108/api/',
    hub: 'http://10.4.4.224:1009/ec-hub',
    scalingHub: 'http://10.4.4.224:1009/ec-hub',
    scalingHubLocal: 'http://localhost:5001/scalingHub',
    mqtt: {
        server: 'localhost',
        protocol: "ws",
        port: 1883
    }
    // apiUrlEC: 'http://10.4.4.224:10022/api/',
    // apiUrl: 'https://localhost:5000/api/',
    // hub: 'http://10.4.4.224:10022/ec-hub',
    // scalingHub: 'http://10.4.5.174:5000/scalingHub',
};
/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.
//# sourceMappingURL=environment.js.map