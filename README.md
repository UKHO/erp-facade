# ERP Facade
ENC publishing is an event driven ENC data harvesting and validation service with some workflow elements. ENC Publishing outputs metadata to the Enterprise Event Service (EES) whenever a batch of ENC changes occur and does so in an EncContentPublished event. This event needs to be accessed by the ERP system (SAP R/3) so that it can update its database to pay royalties and to respond with pricing information needed by the Sales Catalogue. However, for architectural reasons it is not intended that SAP will subscribe directly to the EncContentPublished events. 

An event handler application called the ERP Facade will subscribe to EES events. It will communicate with a SOAP web service exposed by SAP so that SAP can receive this inbound information. In return, the ERP Facade will expose a web service to allow SAP to communicate outbound pricing information. The ERP Facade will then convert this output into JSON and combine with the inbound event data to produce an outbound EES event that now also contains the pricing information. This the unitOfSaleUpdated event which will be published on the EES.

The ERP Facade will subscribe to Event Grid events and translate these into SAP API calls. In return, it will also expose a web service to allow SAP to call the ERP Facade API to respond with pricing information. These will be translated into Event Grid events by the Facade.

The incremental development of the ERP Facade will be split into 3 outcomes.

## Outcomes
### Outcome 1

* Webhook to receive the *uk.gov.ukho.encpublishing.enccontentpublished.v2* CloudEvent from the Authority’s Enterprise Event Service (EES). The functionality can be rolled out incrementally based on information in the *enccontentpublished* event as it becomes available, e.g. initially it could handle stand-alone new cells; then stand-alone cancellations. 

* Each event will contain all of the applicable ‘releasability set’ or batch of ENCs that should be processed. There is no need to cache events until others have arrived. 

* Extraction of the relevant information. Some event content is not relevant to SAP. See the of API call data elements expected at Annex 1.4 vs the sample event in Annex 1.3.1 

* Translation of the relevant event contents for each releasability set into the required SAP Inbound batch ordering structure. This order allows SAP to amend its master data tables in the correct order, which is generally Cell changes, followed by Unit changes, followed by associations between Cells and Units. 

* Lookup of some values to find a code required for SAP. For example, "unitType": "AVCS Folio Transit" = “10”, see Annex 1.8 for examples. 

* Recording of the Inbound Trace ID on the *enccontentpublished* event for each intended SAP Inbound communication batch. 

* Communication of each batch of information (which may contain multiple Units of Sale) via API calls to the SAP Inbound web service using SOAP 1.2 protocol.

### Outcome 2

* Expose a webservice (SOAP 1.2) for SAP Outbound 1 & 2 API calls. 

* Receive any SAP Outound1 calls. As these are only in response to a previous SAP Inbound call, the Message IDs should be compared. The SAP Outbound1 data received will meet the specification in Annex 1.6 

* Log successful Inbound and Outbound1 Trace IDs to the Enterprise logging service (ELK Stack). This ensures that each inbound message gained a response. 
* Check that a price is returned for each and every unit of sale sent to SAP. Log any discrepancies to the Enterprise Logging service but let the Event continue with a NULL price for the errored item. This will include all units of sale in the inbound event regardless of whether the price has actually changed. 

* For each received Outbound1 batch, generate a single *uk.gov.ukho.erpFacade*. unitOfSaleUpdated event for the batch. This will contain all of the inbound data in the *uk.gov.ukho.encpublishing.enccontentpublished* event with the addition of the price information returned from SAP, and will conform to the event specification below, and publish to the EES. Each Unit of Sale will have a current and may have a future price (if applicable). 

> **Note:** Although there is a danger that the pricing data appended to the *enccontentpublished* event makes the *unitOfSaleUpdated* event larger than the EventGrid message size limits (even if the *enccontentpublished* event was under the limit), the splitting of the *enccontentpublished* event will be performed by ENC Publishing with a headroom tolerance for the additional pricing information. If the ERP Facade were to do this, there would then need to be an extra Trace ID for each additional outbound event message which would not have a corresponding inbound Trace ID 

### Outcome 3

* Receive any SAP Outbound2 calls. Determine the number of events required based on EventGrid message size limitations. 

* Create the appropriate uk.gov.ukho.licensing.bulkPriceChange event(s) for the batch. 

* As the Outbound2 calls were NOT in response to a previous SAP Inbound call, a suitable Trace ID should be created for each uk.gov.ukho.licensing.bulkPriceChange event. The Trace IDs should be distinguishable from the Message IDs created for the Inbound/Outbound1 calls, e.g. with a suitable prefix. 

* Publish the bulkPriceChange event(s) to the EES. 

* Log successful Outbound2 Trace IDs to the Authority’s Enterprise logging service. 



