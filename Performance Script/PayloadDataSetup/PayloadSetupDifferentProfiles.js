const Config = JSON.parse(open('../config.json'));

export function PayloadSetup(payload, productList) {

    var productArraySize = payload.data.products.length;
    var unitOfSalesArraySize = payload.data.unitsOfSale.length;
    var jsonObj = payload;

    payload.data.correlationId = generateUniqueCorrId();

    for (var i = 0; i < productArraySize; i++) {
        jsonObj.data.products[i].permit = Config.Permit;
    }
    return jsonObj;
}

function generateRandomString(length) {
    let result = '';
    const characters = 'abcdefghijklmnopqrstuvwxyz0123456789';
    const charactersLength = characters.length;

    for (let i = 0; i < length; i++) {
        const randomIndex = Math.floor(Math.random() * charactersLength);
        result += characters.charAt(randomIndex);
    }
    return result;
}

function generateUniqueCorrId() {
    let result = '';
    const date = new Date().toJSON().slice(0, 10).replace('-', '').replace('-', '');

    result = Date.now().toString(36) + "-" + generateRandomString(8) + "-" + generateRandomString(9);

    result = ('lt-' + date + "-" + result).substring(0, 36);
    return result;
}
