import { sleep } from 'k6';
import http from 'k6/http';
import { check } from 'k6';
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";
import { textSummary } from "https://jslib.k6.io/k6-summary/0.0.1/index.js";
import { URL } from 'https://jslib.k6.io/url/1.0.0/index.js';
import { PayloadSetup } from './../../PayloadDataSetup/PayloadSetup.js';


var Config = JSON.parse(open('./../../config.json'));
var ProductList = JSON.parse(open('./../../PayloadData/S57Payloads/SAPProductList.json'));

var defaultPayload1 = JSON.parse(open('./../../PayloadData/S57Payloads/1ProductNewCell.json'));
var defaultPayload2 = JSON.parse(open('./../../PayloadData/S57Payloads/5ProductsNewCell.json'));
var defaultPayload3 = JSON.parse(open('./../../PayloadData/S57Payloads/10ProductsNewCell.json'));
var defaultPayload4 = JSON.parse(open('./../../PayloadData/S57Payloads/100ProductsNewCell.json'));

if (!Config.BaseUrl.toString().toUpperCase().includes("DEV")) {
    throw new Error("Invalid Environment !! Please use DEV environment for performance testing.\n");
}
const url = new URL(Config.BaseUrl + Config.WebhookUrl);
const headers = {
    'Content-Type': 'application/json',
    Authorization: `Bearer ${Config.Token}`,

};

export function setup() {
    const eventStartDate = new Date(Date.now());
    console.log("start time:" + eventStartDate.toUTCString());

}


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
        ScenarioWithFiveProduct: { // 10 for 5prod & 10 for 10prod
            executor: 'constant-arrival-rate',
            exec: 'ScenarioWithFiveProduct',
            rate: 5,
            timeUnit: '1m',
            startTime: '20m',
            duration: '15m',
            preAllocatedVUs: 1,
            maxVUs: 15
        },
        ScenarioWithTenProduct: { // change to 100 product
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
    }
};



export function ScenarioWithOneProduct() {

    const updatedPayload1 = PayloadSetup(defaultPayload1, ProductList);
    const res = http.post(url.toString(), JSON.stringify(updatedPayload1), { headers }, { tags: { my_custom_tag: 'ScenarioWithOneProduct' } });
    console.log("In ScenarioWithOneProduct");
    console.log(updatedPayload1.data.correlationId);

    check(res, {
        'Status is 200': (r) => r.status === 200,
    });

    console.log("Status code:" + res.status);
    sleep(1);
}
export function ScenarioWithFiveProduct() {

    const updatedPayload2 = PayloadSetup(defaultPayload2, ProductList);
    const res = http.post(url.toString(), JSON.stringify(updatedPayload2), { headers }, { tags: { my_custom_tag: 'ScenarioWithFiveProduct' } });
    console.log("In ScenarioWithFiveProduct");
    console.log(updatedPayload2.data.correlationId);

    check(res, {
        'Status is 200': (r) => r.status === 200,
    });

    console.log("Status code:" + res.status);
    sleep(1);
}

export function ScenarioWithTenProduct() {

    const updatedPayload3 = PayloadSetup(defaultPayload3, ProductList);
    const res = http.post(url.toString(), JSON.stringify(updatedPayload3), { headers }, { tags: { my_custom_tag: 'ScenarioWithTenProduct' } });
    console.log("In ScenarioWithTenProduct");
    console.log(updatedPayload3.data.correlationId);

    check(res, {
        'Status is 200': (r) => r.status === 200,
    });

    console.log("Status code:" + res.status);
    sleep(1);
}

export function ScenarioWithHundredProduct() {

    const updatedPayload4 = PayloadSetup(defaultPayload4, ProductList);
    const res = http.post(url.toString(), JSON.stringify(updatedPayload4), { headers }, { tags: { my_custom_tag: 'ScenarioWithHundredProduct' } });
    console.log("In ScenarioWithHundredProduct");
    console.log(updatedPayload4.data.correlationId)

    check(res, {
        'Status is 200': (r) => r.status === 200,
    });

    console.log("Status code:" + res.status);
    sleep(1);
}
export function teardown() {
    const eventEndDate = new Date(Date.now());
    console.log("End time:" + eventEndDate.toUTCString());
}

//reporting
export function handleSummary(data) {
    return {
        ["Summary/TestResult_" + new Date().toISOString().substr(0, 19).replace(/(:|-)/g, "").replace("T", "_") + ".html"]: htmlReport(data),
        stdout: textSummary(data, { indent: " ", enableColors: true }),
        ["Summary/TestResult_" + new Date().toISOString().substr(0, 19).replace(/(:|-)/g, "").replace("T", "_") + ".json"]: JSON.stringify(data),
    }
}

