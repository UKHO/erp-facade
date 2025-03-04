import http from 'k6/http';
import { check } from 'k6';
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";
import { textSummary } from "https://jslib.k6.io/k6-summary/0.0.1/index.js";
import { URL } from 'https://jslib.k6.io/url/1.0.0/index.js';
import { PayloadSetup, S100PayloadSetup } from '../PayloadDataSetup/PayloadSetupDifferentProfiles.js';
import { Trend,Counter } from 'k6/metrics';

var Config = JSON.parse(open('./../config.json'));
const S57ResponseTimewithOneProduct = new Trend('S57ResponseTimewithOneProduct');
const S100ResponseTimewithOneProduct = new Trend('S100ResponseTimewithOneProduct');
const S57ResponseTimewithTwoProduct = new Trend('S57ResponseTimewithTwoProduct');
const S100ResponseTimewithTwoProduct = new Trend('S100ResponseTimewithTwoProduct');
const S57ResponseTimewithHundredProduct = new Trend('S57ResponseTimewithHundredProduct');
const S100ResponseTimewithHundredProduct = new Trend('S100ResponseTimewithHundredProduct');

const S57RequestCountWithOneProduct = new Counter('S57RequestCountWithOneProduct');
const S100RequestCountWithOneProduct = new Counter('S100RequestCountWithOneProduct');
const S57RequestCountWithTwoProduct = new Counter('S57RequestCountWithTwoProduct');
const S100RequestCountWithTwoProduct = new Counter('S100RequestCountWithTwoProduct');
const S57RequestCountWithHundredProduct = new Counter('S57RequestCountWithHundredProduct');
const S100RequestCountWithHundredProduct = new Counter('S100RequestCountWithHundredProduct');


var PayloadOneProduct = JSON.parse(open('./../PayloadData/S57Payloads/1ProductNewCell.json'));
var PayloadTwoProducts = JSON.parse(open('./../PayloadData/S57Payloads/2ProductENCNewAndMoveCell.json'));
var PayloadHundredProducts = JSON.parse(open('./../PayloadData/S57Payloads/100ProductsNewCell.json'));

var S100PayloadOneProduct = JSON.parse(open('./../PayloadData/S100Payloads/1ProductNewCell_S100.json'));
var S100PayloadTwoProducts = JSON.parse(open('./../PayloadData/S100Payloads/2ProductsNewCellAndNewEdition_S100.json'));
var S100PayloadHundredProducts = JSON.parse(open('./../PayloadData/S100Payloads/100ProductsNewCell_S100.json'));

if (!Config.BaseUrl.toString().toUpperCase().includes("DEV")) {
    throw new Error("Invalid Environment !! Please use DEV environment for performance testing.\n");
}
const url = new URL(Config.BaseUrl + Config.WebhookUrl);

const headers = {
    'Content-Type': 'application/json',
    Authorization: `Bearer ${Config.Token}`,

};

export function setup() {
    const event = new Date(Date.now());
    console.log("start time:" + event.toUTCString());
}

export const options = {
    discardResponseBodies: false,
    thresholds: {
        // Overall response time P95 threshold
        'http_req_duration': ['p(95)<=1000'], // 95% of requests must complete within 1s

        // Overall response time Pmax threshold 
        'http_req_duration': ['p(100)<=5000'], // 100% of requests must complete within 5s 

        // Total request count threshold S-57 & S-100
        'http_reqs': ['count>=2000'], // Total request count should be more then 2000
    },
    scenarios: {

        SingleProductLoadS57: {
            executor: 'constant-arrival-rate',
            exec: 'ScenarioWithOneProduct',
            rate: 8, //80% load with one S-57 product
            preAllocatedVUs: 1,
            maxVUs: 8,
            timeUnit: '36s',
            duration: '1h',
        },

        SingleProductLoadS100: {
            executor: 'constant-arrival-rate',
            exec: 'ScenarioWithOneProductS100',
            rate: 8, //80% load with one S-100 product
            preAllocatedVUs: 1,
            maxVUs: 8,
            timeUnit: '36s',
            duration: '1h',
        },

        TwoProductLoadS57: {
            executor: 'constant-arrival-rate',
            exec: 'ScenarioWithTwoProducts',
            rate: 15, //15% load with two S-57 products
            preAllocatedVUs: 1,
            maxVUs: 15,
            timeUnit: '6m',
            duration: '1h',
        },

        TwoProductLoadS100: {
            executor: 'constant-arrival-rate',
            exec: 'ScenarioWithTwoProductsS100',
            rate: 15, //15% load with two S-100 products
            preAllocatedVUs: 1,
            maxVUs: 15,
            timeUnit: '6m',
            duration: '1h',
        },

        HundredProductLoadS57: {
            executor: 'constant-arrival-rate',
            exec: 'ScenarioWithHundredProducts',
            rate: 25,//5% load with hundred S-57 products
            preAllocatedVUs: 25,
            maxVUs: 30,
            timeUnit: '30m',
            duration: '1h',
        },

        HundredProductLoadS100: {
            executor: 'constant-arrival-rate',
            exec: 'ScenarioWithHundredProductsS100',
            rate: 25,//5% load with hundred S-100 products
            preAllocatedVUs: 25,
            maxVUs: 30,
            timeUnit: '30m',
            duration: '1h',
        }
    }
};

export function ScenarioWithOneProduct() {
    const updatedPayloadOneProduct = PayloadSetup(PayloadOneProduct);
    const res = http.post(url.toString(), JSON.stringify(updatedPayloadOneProduct), { headers }, { tags: { my_custom_tag: 'ScenarioWithOneProduct' } });
    S57ResponseTimewithOneProduct.add(res.timings.duration);
    S57RequestCountWithOneProduct.add(1); // Increment the counter by 1
    check(res, {
        'Status is 200': (r) => r.status === 200,
    });
    console.log("||ScenarioWithOneProductS57||Status code:" + res.status);
}

export function ScenarioWithOneProductS100() {
    const updatedS100PayloadOneProduct = S100PayloadSetup(S100PayloadOneProduct);
    const res = http.post(url.toString(), JSON.stringify(updatedS100PayloadOneProduct), { headers }, { tags: { my_custom_tag: 'ScenarioWithOneProductS100' } });
    S100ResponseTimewithOneProduct.add(res.timings.duration);
    S100RequestCountWithOneProduct.add(1);
    check(res, {
        'Status is 200': (r) => r.status === 200,
    });
    console.log("||ScenarioWithOneProductS100||Status code:" + res.status);
}

export function ScenarioWithTwoProducts() {
    const updatedPayloadTwoProducts = PayloadSetup(PayloadTwoProducts);
    const res = http.post(url.toString(), JSON.stringify(updatedPayloadTwoProducts), { headers }, { tags: { my_custom_tag: 'ScenarioWithTwoProducts' } });
    S57ResponseTimewithTwoProduct.add(res.timings.duration);
    S57RequestCountWithTwoProduct.add(1); 
    check(res, {
        'Status is 200': (r) => r.status === 200,
    });
    console.log("||ScenarioWithTwoProductsS57||Status code:" + res.status);
}

export function ScenarioWithTwoProductsS100() {
    const updatedS100PayloadTwoProducts = S100PayloadSetup(S100PayloadTwoProducts);
    const res = http.post(url.toString(), JSON.stringify(updatedS100PayloadTwoProducts), { headers }, { tags: { my_custom_tag: 'ScenarioWithTwoProductsS100' } });
    S100ResponseTimewithTwoProduct.add(res.timings.duration);
    S100RequestCountWithTwoProduct.add(1); 
    check(res, {
        'Status is 200': (r) => r.status === 200,
    });
    console.log("||ScenarioWithTwoProductsS100||Status code:" + res.status);
}

export function ScenarioWithHundredProducts() {
    const updatedPayloadHundredProducts = PayloadSetup(PayloadHundredProducts);
    const res = http.post(url.toString(), JSON.stringify(updatedPayloadHundredProducts), { headers }, { tags: { my_custom_tag: 'ScenarioWithHundredProducts' } });
    S57ResponseTimewithHundredProduct.add(res.timings.duration);
    S57RequestCountWithHundredProduct.add(1); 
    check(res, {
        'Status is 200': (r) => r.status === 200,
    });
    console.log("||ScenarioWithHundredProductsS57||Status code:" + res.status);
}

export function ScenarioWithHundredProductsS100() {
    const updatedS100PayloadHundredProducts = S100PayloadSetup(S100PayloadHundredProducts);
    const res = http.post(url.toString(), JSON.stringify(updatedS100PayloadHundredProducts), { headers }, { tags: { my_custom_tag: 'ScenarioWithHundredProductsS100' } });
    S100ResponseTimewithHundredProduct.add(res.timings.duration);
    S100RequestCountWithHundredProduct.add(1); 
    check(res, {
        'Status is 200': (r) => r.status === 200,
    });
    console.log("||ScenarioWithHundredProductsS100||Status code:" + res.status);
}

export function teardown() {
    const eventEndDate = new Date(Date.now());
    console.log("End time:" + eventEndDate.toUTCString());
}

//reporting
export function handleSummary(data) {
    return {
        ["./Summary/BaselineResult_" + new Date().toISOString().substr(0, 19).replace(/(:|-)/g, "").replace("T", "_") + ".html"]: htmlReport(data),
        stdout: textSummary(data, { indent: " ", enableColors: true }),
        ["./Summary/BaselineResult_" + new Date().toISOString().substr(0, 19).replace(/(:|-)/g, "").replace("T", "_") + ".json"]: JSON.stringify(data),
    }
}

