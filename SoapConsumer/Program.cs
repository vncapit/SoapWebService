using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using Cap.Helpers.Logger;
using System.Xml.Linq;

var logger = new Logger();

while(true)
{
    Console.WriteLine("Enter order ids to query:");
    string idsString = Console.ReadLine() ?? "";
    if(idsString != "")
    {
        var ids = idsString.Split(',').Select(x => Convert.ToInt32(x)).ToArray();
        Console.WriteLine($"Ids: {JsonConvert.SerializeObject(ids)}");
        logger.Info("Requets query Ids: ", ids);
        HttpClient client = new HttpClient();
        string xmlSoap = @"<?xml version=""1.0"" encoding=""utf-8""?>
                            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                              <soap:Body>
                                <getOrderByIds xmlns=""http://vncapit.com/orderservice"">
                                  <ids>"
                                    +
                                    string.Join("", ids.Select(id => $"<int>{id}</int>").ToArray())
                                    +
                                  @"</ids>
                                </getOrderByIds>
                              </soap:Body>
                            </soap:Envelope>";

        HttpContent content = new StringContent(xmlSoap, Encoding.UTF8, "text/xml");
        client.DefaultRequestHeaders.Add("SOAPAction", "http://tempuri.org/getOrderByIds");
        var response = await client.PostAsync("http://localhost:5278/OrderService.asmx?op=getOrderByIds", content);
        var soapRes = await response.Content.ReadAsStringAsync();
        Console.WriteLine(soapRes);

        XDocument xmlDoc = XDocument.Parse(soapRes);
        XNamespace s = "http://schemas.xmlsoap.org/soap/envelope/";
        XNamespace o = "http://vncapit.com/orderservice";
        var envelope = xmlDoc.Element(s + "Envelope");
        var body = envelope?.Element(s + "Body");
        var getOrderByIdsResponse = body?.Element(o + "getOrderByIdsResponse");
        var getOrderByIdsResult = getOrderByIdsResponse?.Element(o + "getOrderByIdsResult");
        var orders = getOrderByIdsResult?.Elements(o + "Order");

        if(orders is not null)
        {
            foreach (var order in orders)
            {
                int orderNumber = int.Parse(order?.Element(o+"OrderNumber")?.Value ?? "");
                string status = order?.Element(o+"Status")?.Value ?? "";
                string comments = order?.Element(o+"Comments")?.Value ?? "";
                var log = $"Order info: OrderNumber: {orderNumber}, Status: {status}, Comments: {comments}";
                logger.Info("Order info: ", log);
            }
        }

        // use linq:
        var ordersL = xmlDoc.Descendants().Where(node => node.Name == o + "Order");
        foreach (var item in ordersL)
        {
            int orderNumber = int.Parse(item?.Element(o + "OrderNumber")?.Value ?? "");
            string status = item?.Element(o + "Status")?.Value ?? "";
            string comments = item?.Element(o + "Comments")?.Value ?? "";
            var log = $"Order info: OrderNumber: {orderNumber}, Status: {status}, Comments: {comments}";
            logger.Info("Order info Linq: ", log);
        }
    }
}


