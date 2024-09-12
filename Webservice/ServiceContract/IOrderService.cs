using System;
using System.ServiceModel;
using System.Xml.Linq;
using Webservice.DataContract;
namespace Webservice.ServiceContract
{
	[ServiceContract(Namespace = "http://vncapit.com/orderservice")]
	public interface IOrderService
	{
		[OperationContract]
		List<Order> getOrderByIds(int[] ids);

		[OperationContract]
		int addOrder(Order order);
	}
}

