// const SYSTEM_CODE = 3;
// export const environment = {
//   production: true,
//   systemCode: SYSTEM_CODE,
//   apiUrlEC: 'http://10.4.5.174:1002/api/',
//   apiUrl: 'http://10.4.5.174:1086/api/',
//   apiUrl2: 'http://10.4.5.174:1086/api/',
//   hub: 'http://10.4.5.174:1002/ec-hub',
//   scalingHub: 'http://10.4.5.174:5000/scalingHub',
// };

const SYSTEM_CODE = 3;
export const environment = {
  production: true,
  systemCode: SYSTEM_CODE,
  apiUrlEC: 'http://10.4.5.174:80/api/',
  apiUrl: 'http://10.4.5.174:108/api/',
  apiUrl2: 'http://10.4.5.174:108/api/',
  hub: 'http://10.4.5.174:80/ec-hub',
  scalingHub: 'http://10.4.5.174:80/ec-hub',
  scalingHubLocal: 'http://localhost:5001/scalingHub'
};
