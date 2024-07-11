# ERP Facade
ENC publishing is an event driven ENC data harvesting and validation service with some workflow elements. ENC Publishing outputs metadata to the Enterprise Event Service (EES) whenever a batch of ENC changes occur and does so in an EncContentPublished event. This event needs to be accessed by the ERP system (SAP R/3) so that it can update its database to pay royalties. However, for architectural reasons it is not intended that SAP will subscribe directly to the EncContentPublished events.
 
An event handler application called the ERP Facade will subscribe to EES events. It will communicate with a SOAP web service exposed by SAP so that SAP can receive this inbound information.
 
## Outcome
 
* Webhook to receive the *uk.gov.ukho.encpublishing.enccontentpublished.v2.1* CloudEvent from the Authority’s Enterprise Event Service (EES). The functionality can be rolled out incrementally based on information in the *enccontentpublished* event as it becomes available, e.g. initially it could handle stand-alone new cells; then stand-alone cancellations.
 
* Each event will contain all of the applicable ‘releasability set’ or batch of ENCs that should be processed. There is no need to cache events until others have arrived.
 
* Extraction of the relevant information. Some event content is not relevant to SAP.
 
* Translation of the relevant event contents for each releasability set into the required SAP Inbound batch ordering structure. This order allows SAP to amend its master data tables in the correct order, which is generally Cell changes, followed by Unit changes, followed by associations between Cells and Units.
 
* Lookup of some values to find a code required for SAP. For example, "unitType": "AVCS Folio Transit" = “10”.
 
* Recording of the Inbound Trace ID on the *enccontentpublished* event for each intended SAP Inbound communication batch.
 
* Communication of each batch of information (which may contain multiple Units of Sale) via API calls to the SAP Inbound web service using SOAP 1.2 protocol.

# ADDS SAP Mock

SAP mock for support of manual or automated ADDS testing in absence of real SAP instance.

Initial version supports:
- Supports hosting of /z_adds_mat_info endpoint to which SAP action items XML can be posted.
- Supports sending pricing information JSON to a configurable ERP facade pricing endpoint, using the product names from the SAP action items recieved.
- Supports configuring the price output depending on the product name(s) provided in the SAP XML - one price info JSON object per duration per product name is sent, combined a single array. Product names are matched to durations as specified in `ProductDummyData.json`

Initial version does not support:
- Hosting of /z_adds_ros endpoint for communication with Fleet Manager
- Customisation of other price information values. These are hard coded in `ErpFacadePriceEndpointClient`

There is an integration level test, for which you may need to specify a .runsettings file in your IDE or test runner. One runsettings file is provided though secrets are ommitted.
