export  function PayloadSetup(payload, productList) {
    
    var productArraySize = payload.data.products.length;
    var unitOfSalesArraySize = payload.data.unitsOfSale.length;
    var jsonObj = payload;
    
    payload.data.correlationId = generateUniqueCorrId();

    for(var i = 0;  i< productArraySize; i++){
      
      jsonObj.data.products[i].dataSetName = productList.frenchProducts[700 + i] + '.001';
      jsonObj.data.products[i].productName = productList.frenchProducts[700 + i];
      jsonObj.data.products[i].inUnitsOfSale[0] = productList.frenchProducts[700 + i];
      jsonObj.data.products[i].providerCode = "2";
      jsonObj.data.products[i].providerName = "PRIMAR";
      jsonObj.data.products[i].size = "large";
      jsonObj.data.products[i].agency = "FR";
      
      jsonObj.data.unitsOfSale[i].unitName = productList.frenchProducts[700 + i];
      jsonObj.data.unitsOfSale[i].unitSize = "large";
      jsonObj.data.unitsOfSale[i].compositionChanges.addProducts.splice(0, 1, productList.frenchProducts[700 + i]);
      //jsonObj.data.unitsOfSale[i].compositionChanges.addProducts.splice(0, 1, generatedProductName);
      // update AVCSO & PAYSF
      jsonObj.data.unitsOfSale[unitOfSalesArraySize-2].compositionChanges.addProducts.push(productList.frenchProducts[700 + i]);
      jsonObj.data.unitsOfSale[unitOfSalesArraySize-2].unitSize = "large";
      jsonObj.data.unitsOfSale[unitOfSalesArraySize-1].compositionChanges.addProducts.push(productList.frenchProducts[700 + i]);
      jsonObj.data.unitsOfSale[unitOfSalesArraySize-1].unitSize = "large";
    }

    return jsonObj;
} 


function generateRandomString(length) {
    let result = '';
    //const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    const characters = 'abcdefghijklmnopqrstuvwxyz0123456789';
    const charactersLength = characters.length;
  
    for (let i = 0; i < length; i++) {
      const randomIndex = Math.floor(Math.random() * charactersLength);
      result += characters.charAt(randomIndex);
    } 
    return result;
}

function generateCorrId(){
  let result = '';
  const characters = 'abcdefghijklmnopqrstuvwxyz0123456789';
  const charactersLength = characters.length;

  for (let i = 0; i < length; i++) {
    const randomIndex = Math.floor(Math.random() * charactersLength);
    result += characters.charAt(randomIndex);
  };
  
  result = result.slice(0, 4) + "-" + result.slice(4, 12) + "-" + result.slice(12);
  result = 'lt-' + (new Date().toJSON().slice(0, 10).replace('-','').replace('-','')) + '-' + result;
  console.log("Generated Correlation ID: "+result);
  return result;
}

function generateUniqueCorrId(){
  let result = '';
  const date = new Date().toJSON().slice(0, 10).replace('-','').replace('-','');

  result = Date.now().toString(36) + "-" + generateRandomString(8) + "-" + generateRandomString(9);
   
  result = ('lt-' + date + "-" + result).substring(0, 36);
  console.log("Generated Unique Correlation ID: " +result);
  return result;
}