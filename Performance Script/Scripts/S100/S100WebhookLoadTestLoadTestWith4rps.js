import http from 'k6/http';
import { check } from 'k6';
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";
import { textSummary } from "https://jslib.k6.io/k6-summary/0.0.1/index.js";
import { URL } from 'https://jslib.k6.io/url/1.0.0/index.js';
import { PayloadSetup, S100PayloadSetup } from './../../PayloadDataSetup/PayloadSetupDifferentProfiles.js';

var Config = JSON.parse(open('./../../config.json'));

var PayloadOneProduct = JSON.parse(open('./../../PayloadData/S57Payloads/1ProductNewCell.json'));
var PayloadTwoProducts = JSON.parse(open('./../../PayloadData/S57Payloads/2ProductENCNewAndMoveCell.json'));
var PayloadHundredProducts = JSON.parse(open('./../../PayloadData/S57Payloads/100ProductsNewCell.json'));

var S100PayloadOneProduct = JSON.parse(open('./../../PayloadData/S100Payloads/1ProductNewCell_S100.json'));
var S100PayloadTwoProducts = JSON.parse(open('./../../PayloadData/S100Payloads/2ProductsNewCellAndNewEdition_S100.json'));
var S100PayloadHundredProducts = JSON.parse(open('./../../PayloadData/S100Payloads/100ProductsNewCell_S100.json'));

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

        'http_req_duration{scenario:ScenarioWithOneProductS100}': [`max>=0`],
        'iterations{scenario:ScenarioWithOneProductS100}': [`count>=0`],

        'http_req_duration{scenario:ScenarioWithTwoProducts}': [`max>=0`],
        'iterations{scenario:ScenarioWithTwoProducts}': [`count>=0`],

        'http_req_duration{scenario:ScenarioWithTwoProductsS100}': [`max>=0`],
        'iterations{scenario:ScenarioWithTwoProductsS100}': [`count>=0`],

        'http_req_duration{scenario:ScenarioWithHundredProducts}': [`max>=0`],
        'iterations{scenario:ScenarioWithHundredProducts}': [`count>=0`],

        'http_req_duration{scenario:ScenarioWithHundredProductsS100}': [`max>=0`],
        'iterations{scenario:ScenarioWithHundredProductsS100}': [`count>=0`],
    },
    scenarios: {
        ScenarioWithOneProduct: {
            executor: 'constant-arrival-rate',
            exec: 'ScenarioWithOneProduct',
            rate: 4,
            timeUnit: '1s',
            startTime: '1s',
            duration: '27m',
            preAllocatedVUs: 5,
            maxVUs: 12,
        },
        ScenarioWithOneProductS100: {
            executor: 'constant-arrival-rate',
            exec: 'ScenarioWithOneProductS100',
            rate: 4,
            timeUnit: '1s',
            startTime: '1s',
            duration: '27m',
            preAllocatedVUs: 5,
            maxVUs: 12,
        },
        ScenarioWithTwoProducts: { 
            executor: 'constant-arrival-rate',
            exec: 'ScenarioWithTwoProducts',
            rate: 4,
            timeUnit: '1s',
            startTime: '27m',
            duration: '179s',
            preAllocatedVUs: 5,
            maxVUs: 12
        },
        ScenarioWithTwoProductsS100: { 
            executor: 'constant-arrival-rate',
            exec: 'ScenarioWithTwoProductsS100',
            rate: 4,
            timeUnit: '1s',
            startTime: '27m',
            duration: '179s',
            preAllocatedVUs: 5,
            maxVUs: 12
        },
        ScenarioWithHundredProducts: { 
            executor: 'constant-arrival-rate',
            exec: 'ScenarioWithHundredProducts',
            rate: 4,
            timeUnit: '1s',
            startTime: '1799s',
            duration: '1s',
            preAllocatedVUs: 1
        },
        ScenarioWithHundredProductsS100: { 
            executor: 'constant-arrival-rate',
            exec: 'ScenarioWithHundredProductsS100',
            rate: 4,
            timeUnit: '1s',
            startTime: '1799s',
            duration: '1s',
            preAllocatedVUs: 1
        }
    }
};

export function ScenarioWithOneProduct() {

    const updatedPayloadOneProduct = PayloadSetup(PayloadOneProduct);
    const res = http.post(url.toString(), JSON.stringify(updatedPayloadOneProduct), { headers }, { tags: { my_custom_tag: 'ScenarioWithOneProduct' } });
    console.log("In ScenarioWithOneProduct:");
    console.log(updatedPayloadOneProduct.data.correlationId);

    check(res, {
        'Status is 200': (r) => r.status === 200,
    });
    console.log("Status code:" + res.status);
}

export function ScenarioWithOneProductS100() {

    const updatedS100PayloadOneProduct = S100PayloadSetup(S100PayloadOneProduct);
    const res = http.post(url.toString(), JSON.stringify(updatedS100PayloadOneProduct), { headers }, { tags: { my_custom_tag: 'ScenarioWithOneProductS100' } });
    console.log("In ScenarioWithOneProductS100:");
    console.log(updatedS100PayloadOneProduct.data.correlationId);

    check(res, {
        'Status is 200': (r) => r.status === 200,
    });
    console.log("Status code:" + res.status);
}

export function ScenarioWithTwoProducts() {

    const updatedPayloadTwoProducts = PayloadSetup(PayloadTwoProducts);
    const res = http.post(url.toString(), JSON.stringify(updatedPayloadTwoProducts), { headers }, { tags: { my_custom_tag: 'ScenarioWithTwoProducts' } });
    console.log("In ScenarioWithTwoProducts");
    console.log(updatedPayloadTwoProducts.data.correlationId);

    check(res, {
        'Status is 200': (r) => r.status === 200,
    });
    console.log("Status code:" + res.status);
}

export function ScenarioWithTwoProductsS100() {

    const updatedS100PayloadTwoProducts = S100PayloadSetup(S100PayloadTwoProducts);
    const res = http.post(url.toString(), JSON.stringify(updatedS100PayloadTwoProducts), { headers }, { tags: { my_custom_tag: 'ScenarioWithTwoProductsS100' } });
    console.log("In ScenarioWithTwoProductsS100");
    console.log(updatedS100PayloadTwoProducts.data.correlationId);

    check(res, {
        'Status is 200': (r) => r.status === 200,
    });
    console.log("Status code:" + res.status);
}

export function ScenarioWithHundredProducts() {

    const updatedPayloadHundredProducts = PayloadSetup(PayloadHundredProducts);
    const res = http.post(url.toString(), JSON.stringify(updatedPayloadHundredProducts), { headers }, { tags: { my_custom_tag: 'ScenarioWithHundredProducts' } });
    console.log("In ScenarioWithHundredProducts");
    console.log(updatedPayloadHundredProducts.data.correlationId);

    check(res, {
        'Status is 200': (r) => r.status === 200,
    });
    console.log("Status code:" + res.status);
}

export function ScenarioWithHundredProductsS100() {

    const updatedS100PayloadHundredProducts = S100PayloadSetup(S100PayloadHundredProducts);
    const res = http.post(url.toString(), JSON.stringify(updatedS100PayloadHundredProducts), { headers }, { tags: { my_custom_tag: 'ScenarioWithHundredProductsS100' } });
    console.log("In ScenarioWithHundredProductsS100");
    console.log(updatedS100PayloadHundredProducts.data.correlationId);

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

