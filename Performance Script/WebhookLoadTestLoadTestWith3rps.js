import { sleep } from 'k6';
import http from 'k6/http';
import { check } from 'k6';
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";
import { textSummary } from "https://jslib.k6.io/k6-summary/0.0.1/index.js";
import { URL } from 'https://jslib.k6.io/url/1.0.0/index.js';
import { PayloadSetup } from './PayloadDataSetup/PayloadSetupDifferentProfiles.js';


var Config = JSON.parse(open('./config.json'));
var ProductList = JSON.parse(open('./PayloadData/WebhookPayloads/SAPProductList.json'));

var defaultPayload1 = JSON.parse(open('./PayloadData/WebhookPayloads/1ProductNewCell.json'));
var defaultPayload2 = JSON.parse(open('./PayloadData/WebhookPayloads/2ProductENCNewAndMoveCell.json'));
var defaultPayload3 = JSON.parse(open('./PayloadData/WebhookPayloads/100ProductsNewCell.json'));

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
    discardResponseBodies: true,
    thresholds: {
        'http_req_duration{scenario:ScenarioWithOneProduct}': [`max>=0`],
        'iterations{scenario:ScenarioWithOneProduct}': [`count>=0`],

        'http_req_duration{scenario:ScenarioWithTwoProduct}': [`max>=0`],
        'iterations{scenario:ScenarioWithTwoProduct}': [`count>=0`],

        'http_req_duration{scenario:ScenarioWithHundredProduct}': [`max>=0`],
        'iterations{scenario:ScenarioWithHundredProduct}': [`count>=0`],
    },
    scenarios: {
        ScenarioWithOneProduct: {
            executor: 'constant-arrival-rate',
            exec: 'ScenarioWithOneProduct',
            rate: 3,
            timeUnit: '1s',
            startTime: '1s',
            duration: '27m',
            preAllocatedVUs: 5,
            maxVUs: 12,
        },
        ScenarioWithTwoProduct: { 
            executor: 'constant-arrival-rate',
            exec: 'ScenarioWithTwoProduct',
            rate: 3,
            timeUnit: '1s',
            startTime: '27m',
            duration: '179s',
            preAllocatedVUs: 5,
            maxVUs: 12
        },
        ScenarioWithHundredProduct: { 
            executor: 'constant-arrival-rate',
            exec: 'ScenarioWithHundredProduct',
            rate: 1,
            timeUnit: '1s',
            startTime: '1799s',
            duration: '1s',
            preAllocatedVUs: 1
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
}
export function ScenarioWithTwoProduct() {

    const updatedPayload2 = PayloadSetup(defaultPayload2, ProductList);
    const res = http.post(url.toString(), JSON.stringify(updatedPayload2), { headers }, { tags: { my_custom_tag: 'ScenarioWithTwoProduct' } });
    console.log("In ScenarioWithTwoProduct");
    console.log(updatedPayload2.data.correlationId);

    check(res, {
        'Status is 200': (r) => r.status === 200,
    });
    console.log("Status code:" + res.status);
}

export function ScenarioWithHundredProduct() {

    const updatedPayload3 = PayloadSetup(defaultPayload3, ProductList);
    const res = http.post(url.toString(), JSON.stringify(updatedPayload3), { headers }, { tags: { my_custom_tag: 'ScenarioWithHundredProduct' } });
    console.log("In ScenarioWithTwoProduct");
    console.log(updatedPayload3.data.correlationId);

    check(res, {
        'Status is 200': (r) => r.status === 200,
    });
    console.log("Status code:" + res.status);
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

