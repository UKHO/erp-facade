import { sleep } from 'k6';
import http from 'k6/http';
import { check } from 'k6';
import {htmlReport} from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";
import {textSummary} from "https://jslib.k6.io/k6-summary/0.0.1/index.js";
import { URL } from 'https://jslib.k6.io/url/1.0.0/index.js';

var Config = JSON.parse(open('./config.json'));
var defaultPayload1 = JSON.parse(open('./PayloadData/UoSPayload/1ProductUoS.json'));
var defaultPayload2 = JSON.parse(open('./PayloadData/UoSPayload/5ProductUoS.json'));
var defaultPayload3 = JSON.parse(open('./PayloadData/UoSPayload/10ProductUoS.json'));
var defaultPayload4 = JSON.parse(open('./PayloadData/UoSPayload/100ProductUoS.json'));



export const options = {

    discardResponseBodies: true,

    scenarios: {

ScenarioWithOneProduct: {
  executor: 'constant-arrival-rate',
  exec: 'ScenarioWithOneProduct',
  rate: 40,
  timeUnit: '1m',
  startTime: '10s',
  duration: '20m',
  preAllocatedVUs: 1,
  maxVUs: 20,

},

ScenarioWithFiveProduct: {
  executor: 'constant-arrival-rate',
  exec: 'ScenarioWithFiveProduct',
  rate: 5,
  timeUnit: '1m',
  startTime: '20m',
  duration: '15m',
  preAllocatedVUs: 1,
  maxVUs: 15

},

ScenarioWithTenProduct: {
  executor: 'constant-arrival-rate',
  exec: 'ScenarioWithTenProduct',
  rate: 5,
  timeUnit: '1m',
  startTime: '35m',
  duration: '15m',
  preAllocatedVUs: 1,
  maxVUs: 15
},

ScenarioWithHundredProduct: {
  executor: 'constant-arrival-rate',
  exec: 'ScenarioWithHundredProduct',
  rate: 5,
  timeUnit: '1m',
  startTime: '50m',
  duration: '10m',
  preAllocatedVUs: 1,
  maxVUs: 15

}
    },

  };


export  function ScenarioWithOneProduct() {
  const url = new URL(Config.baseURL + Config.UnitOFSaleURL);
  url.searchParams.append('Key', Config.Key);
  const headers = {
    'Content-Type': 'application/json',
  };
      const res = http.post(url.toString(), JSON.stringify(defaultPayload1), { headers },{ tags: { my_custom_tag: 'ScenarioWithOneProduct' } });
      console.log("In ScenarioWithOneProduct");
      console.log(defaultPayload1[0].corrid);

      check(res, {
      'Status is 200': (r) => r.status === 200,
      });
  
      console.log("Status code:"+res.status);
      sleep(1);
  }
  
  export function ScenarioWithFiveProduct() {
    const url = new URL(Config.baseURL + Config.UnitOFSaleURL);
    url.searchParams.append('Key', Config.Key);
    
    const headers = {
      'Content-Type': 'application/json',
    };
      
      const res = http.post(url.toString(), JSON.stringify(defaultPayload2), { headers },{ tags: { my_custom_tag: 'ScenarioWithFiveProduct' } });
      console.log("In ScenarioWithFiveProduct");
      console.log(defaultPayload2[0].corrid);

      check(res, {
        'Status is 200': (r) => r.status === 200,
      });
    
      console.log("Status code:"+res.status);
      sleep(1);
}

export function ScenarioWithTenProduct() {
  const url = new URL(Config.baseURL + Config.UnitOFSaleURL);
    url.searchParams.append('Key', Config.Key);
    
    const headers = {
      'Content-Type': 'application/json',
    };

    const res = http.post(url.toString(), JSON.stringify(defaultPayload3), { headers },{ tags: { my_custom_tag: 'ScenarioWithTenProduct' } });
    console.log("In ScenarioWithTenProduct");
    console.log(defaultPayload3[0].corrid);
    
    check(res, {
      'Status is 200': (r) => r.status === 200,
    });
    
    console.log("Status code:"+res.status);
    sleep(1);
}

export function ScenarioWithHundredProduct() {
  const url = new URL(Config.baseURL + Config.UnitOFSaleURL);
    url.searchParams.append('Key', Config.Key);
    
    const headers = {
      'Content-Type': 'application/json',
    };

  
  const res = http.post(url.toString(), JSON.stringify(defaultPayload4), { headers },{ tags: { my_custom_tag: 'ScenarioWithHundredProduct' } });
      console.log("In ScenarioWithHundredProduct");
      console.log(defaultPayload4[0].corrid)

  check(res, {
    'Status is 200': (r) => r.status === 200,
  });

  console.log("Status code:"+res.status);
  sleep(1);
}


export function handleSummary(data) {
  return {
    ["Summary/TestResult_" + new Date().toISOString().substr(0, 19).replace(/(:|-)/g, "").replace("T", "_") + ".html"]: htmlReport(data),
    stdout: textSummary(data, { indent: " ", enableColors: true }),
    ["Summary/TestResult_" + new Date().toISOString().substr(0, 19).replace(/(:|-)/g, "").replace("T", "_") + ".json"]: JSON.stringify(data),
  }
}
