using System;
using System.Runtime.Serialization;
namespace Webservice.DataContract
{
	[DataContract(Namespace = "http://vncapit.com/orderservice/order")]
	public class Order
	{
		[DataMember]
		public int OrderNumber { get; set; }
        [DataMember]
        public DateTime OrderDate { get; set; }
        [DataMember]
        public DateTime RequiredDate { get; set; }
        [DataMember]
        public DateTime? ShippedDate { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public string? Comments { get; set; }
        [DataMember]
        public int CustomerNumber { get; set; }
        [DataMember]
        public List<OrderDetail> OrderDetails { get; set; }
    }

    [DataContract(Namespace = "http://vncapit.com/orderservice/orderdetail")]
    public class OrderDetail
    {
        [DataMember]
        public int OrderLineNumber { get; set; }
        [DataMember]
        public int OrderNumber { get; set; }
        [DataMember]
        public float PriceEach { get; set; }
        [DataMember]
        public string ProductCode { get; set; }
        [DataMember]
        public int QuantityOrdered { get; set; }
        [DataMember]
        public Product Product { get; set; }
    }

    [DataContract(Namespace = "http://vncapit.com/orderservice/product")]
    public class Product
    {
        [DataMember]
        public float BuyPrice { get; set; }
        [DataMember]
        public float MSRP { get; set; }
        [DataMember]
        public string ProductCode { get; set; }
        [DataMember]
        public string ProductDescription { get; set; }
        [DataMember]
        public string ProductLine { get; set; }
        [DataMember]
        public string ProductName { get; set; }
        [DataMember]
        public string ProductScale { get; set; }
        [DataMember]
        public string ProductVendor { get; set; }
        [DataMember]
        public int QuantityInStock { get; set; }
    }

}

