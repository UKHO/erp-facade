using System.Globalization;
using System.Xml;
using UKHO.ERPFacade.API.Models;

namespace UKHO.ERPFacade.API.XmlHelpers
{
    public class SapMessageBuilder : ISapMessageBuilder
    {
        private readonly ILogger<SapMessageBuilder> logger;
        private const string DATE_FORMAT = "yyyyMMdd";
        private const string NON_ENGLISH_CHARACTERS_REGEX = @"[^\u0000-\u007F]+";

        public SapMessageBuilder(ILogger<SapMessageBuilder> logger)
        {
            this.logger = logger;
        }
        /// <summary>
        /// Generate SAP message xml file.
        /// </summary>
        /// <param name="messageTemplateName"></param>
        /// <param name="eesEventData"></param>
        /// <returns></returns>
        public XmlDocument BuildSapMessageXml(string messageTemplateName, EESEvent eesEventData)
        {
            //Check StripePaymentData
            if (eesEventData == null)
            {
                throw new ArgumentNullException(nameof(eesEventData), "Stripe Payment Data can not be NULL.");
            }

            //get message template path
            //var messageTemplatePath = Path.Combine(executionContext.Value.AppDirectory, messageTemplateName);

            ////Check whether file exists or not
            //if (!File.Exists(messageTemplatePath))
            //{
            //    throw new FileNotFoundException("The SAP message xml template does not exist in specified path " + messageTemplatePath);
            //}

            XmlDocument xmlDocument = new XmlDocument();

            ////Load xml template
            //xmlDocument.Load(messageTemplatePath);

            ////Retrieve order details node
            //XmlNode orderNode = xmlDocument.SelectSingleNode("//*[local-name()='IM_ORDER']");

            //if (orderNode == null)
            //{
            //    throw new InvalidDataException("Xml node 'IM_ORDER' not available in xml template.");
            //}

            ////Bind value of elements           
            //BindValue(orderNode, "UNIQUEID", stripePaymentData.SessionId, 250);
            //BindValue(orderNode, "TRANTYP", stripePaymentData.TransactionType, 10);
            //BindValue(orderNode, "NAME", stripePaymentData.CustomerName, 35);
            //BindValue(orderNode, "ADDR1", stripePaymentData.Line1, 35);
            //BindValue(orderNode, "ADDR2", stripePaymentData.Line2, 35);
            ////Pass country value if city is NULL from stripe
            //if (!string.IsNullOrWhiteSpace(stripePaymentData.City))
            //{
            //    BindValue(orderNode, "CITY", stripePaymentData.City, 35);
            //}
            //else
            //{
            //    BindValue(orderNode, "CITY", stripePaymentData.Country, 3);
            //}
            //BindValue(orderNode, "POSTCODE", stripePaymentData.PostalCode, 10);
            //BindValue(orderNode, "COUNTRY", stripePaymentData.Country, 3);
            //BindValue(orderNode, "PHONE", stripePaymentData.Phone, 16);
            //BindValue(orderNode, "EMAIL", stripePaymentData.Email, 241);
            //BindValue(orderNode, "PONO", stripePaymentData.ReceiptNumber, 12);
            //BindValue(orderNode, "POURL", stripePaymentData.ReceiptUrl, 250);
            ////Check non-english characters for state
            //if (!string.IsNullOrWhiteSpace(stripePaymentData.State))
            //{
            //    BindValue(orderNode, "STATE",
            //        Regex.IsMatch(stripePaymentData.State, NON_ENGLISH_CHARACTERS_REGEX) ? string.Empty : stripePaymentData.State, 3);
            //}

            ////Bind fee details
            //BindValue(orderNode, "GROSSAMOUNT", TransformAmount(stripePaymentData.GrossAmount), 16);
            //BindValue(orderNode, "FEE", TransformAmount(stripePaymentData.TransactionFee), 16);
            //BindValue(orderNode, "NETAMOUNT", TransformAmount(stripePaymentData.NetAmount), 16);
            //BindValue(orderNode, "ERDAT", TransformDate(stripePaymentData.TransactionDate), 8);
            //BindValue(orderNode, "CURRENCY", stripePaymentData.Currency, 5);

            //XmlNode products = orderNode.SelectSingleNode($"//*[local-name()='PRODUCTS']");

            //foreach (OrderLineItem lineItem in stripePaymentData.OrderLineItems)
            //{
            XmlElement item = xmlDocument.CreateElement("item");

            //    XmlElement itemNname = xmlDocument.CreateElement("PRODNAME");
            //    /* As per specification, the lenght of this filed will be decided later. As of now, it is 250 and will be modified once it is decided.*/
            //    SetValue(itemNname, lineItem.OrderLineItemDescription.Name, 250);

            //    XmlElement itemDescription = xmlDocument.CreateElement("PRODDESC");
            //    SetValue(itemDescription, lineItem.OrderLineItemDescription.Description, 250);

            //    XmlElement quantity = xmlDocument.CreateElement("QTY");
            //    SetValue(quantity, lineItem.Quantity.ToString(), 15);

            //    XmlElement amount = xmlDocument.CreateElement("PRICE");
            //    SetValue(amount, TransformAmount(lineItem.Amount), 16);

            //    //Append child elements to item element.
            //    item.AppendChild(itemNname);
            //    item.AppendChild(itemDescription);
            //    item.AppendChild(quantity);
            //    item.AppendChild(amount);

            //    //Append each item to products element
            //    products.AppendChild(item);
            //}
            return xmlDocument;
        }

        /// <summary>
        /// Bind element value.
        /// </summary>
        /// <param name="parentNode">Parent node</param>
        /// <param name="element">Actual element, in which value to be binded.</param>
        /// <param name="value">Value of the node</param>
        /// <param name="maxLength">Max lenght of the corresponding field in SAP</param>
        private static void BindValue(XmlNode parentNode, string element, string value, int maxLength)
        {
            //Retrieve node
            XmlNode node = parentNode?.SelectSingleNode($"//*[local-name()='{element}']");

            if (node == null)
            {
                throw new InvalidDataException($"Xml element '{element}' not available in xml template.");
            }
            //Set value
            SetValue(node, value, maxLength);
        }

        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="node">Actual node, in which value to be set</param>
        /// <param name="value">Value of the node</param>
        /// <param name="maxLength">>Max lenght of the corresponding field in SAP</param>
        private static void SetValue(XmlNode node, string value, int maxLength)
        {
            //As NULL check for 'node' is already taken care, no need to do it again here.
            //Set value
            node.InnerText = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Substring(0, Math.Min(maxLength, value.Length));
        }
        /// <summary>
        /// Convert date into 'yyyy-M-dd' format
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private static string TransformDate(DateTime date)
        {
            string data = string.Empty;

            if (date != DateTime.MinValue)
            {
                data = date.ToString(DATE_FORMAT, CultureInfo.InvariantCulture);
            }

            return data;
        }
        /// <summary>
        /// Convert amount from data type decimal to string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string TransformAmount(decimal? value)
        {
            string data = string.Empty;

            if (value.HasValue)
            {
                data = value.Value.ToString("#.##");
            }
            return data;
        }
    }
}
