using System;
using Webservice.DataContract;
using Webservice.ServiceContract;
using MySql.Data.MySqlClient;
using Cap.Helpers.Logger;

namespace Webservice.ServiceImpl
{
	public class OrderService : IOrderService
	{
        private string CONNECTION_STRING;
        readonly Logger logger;

        public OrderService(IConfiguration configuration)
        {
            CONNECTION_STRING = configuration["ConnectionStrings:TestMySql"];
            this.logger = new Logger();
        }

        public List<Order> getOrderByIds(int[] ids)
        {
            logger.Info($"Request query orders: ", ids);
            List<Order> orders = new List<Order>();
            try
            {
                MySqlConnection conn = new MySqlConnection(CONNECTION_STRING);
                conn.Open();
                using(var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    string idsInString = "(" + string.Join(",", ids) + ")";
                    cmd.CommandText = $"SELECT * FROM orders WHERE orderNumber IN {idsInString}";
                    using (var myReader = cmd.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            Order order = new Order();
                            order.OrderNumber = myReader.GetInt32("orderNumber");
                            order.OrderDate = myReader.GetDateTime("orderDate");
                            order.RequiredDate = myReader.GetDateTime("requiredDate");
                            order.ShippedDate = myReader.IsDBNull(myReader.GetOrdinal("shippedDate")) ? null : myReader.GetDateTime("shippedDate");
                            order.Status = myReader.GetString("status");
                            order.Comments = myReader.IsDBNull(myReader.GetOrdinal("comments")) ? null : myReader.GetString("comments");
                            order.CustomerNumber = myReader.GetInt32("customerNumber");
                            orders.Add(order);
                        }
                    };
                }
                List<Order> newOrders = new List<Order>();
                foreach(var order in orders)
                {
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "SELECT * FROM orderdetails od INNER JOIN products p ON od.productCode = p.productCode WHERE orderNumber = @on";
                        cmd.Parameters.AddWithValue("on", order.OrderNumber);
                        List<OrderDetail> orderDetails = new List<OrderDetail>();

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                OrderDetail orderDetail = new OrderDetail();
                                orderDetail.OrderNumber = reader.GetInt32("orderNumber");
                                orderDetail.OrderLineNumber = reader.GetInt32("orderLineNUmber");
                                orderDetail.PriceEach = reader.GetFloat("priceEach");
                                orderDetail.ProductCode = reader.GetString("productCode");
                                orderDetail.QuantityOrdered = reader.GetInt32("quantityOrdered");

                                Product product = new Product();
                                product.BuyPrice = reader.GetFloat("buyPrice");
                                product.MSRP = reader.GetFloat("MSRP");
                                product.ProductCode = reader.GetString("productCode");
                                product.ProductDescription = reader.GetString("productDescription");
                                product.ProductLine = reader.GetString("productLine");
                                product.ProductName = reader.GetString("productName");
                                product.ProductScale = reader.GetString("productScale");
                                product.ProductVendor = reader.GetString("productVendor");
                                product.QuantityInStock = reader.GetInt32("quantityInStock");

                                orderDetail.Product = product;
                                orderDetails.Add(orderDetail);
                            }
                        }
                        order.OrderDetails = orderDetails;
                        newOrders.Add(order);
                    }
                }
                return newOrders;
            }
            catch(Exception ex)
            {
                logger.Error("Query orders error: ", ex);
            }
            logger.Info("Order query results", orders);
            return orders;
        }

        public int addOrder(Order order)
        {
            logger.Info("Request to insert new order", order);

            using(var conn = new MySqlConnection(CONNECTION_STRING))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        int orderNumber = 0;
                        using(var cmd = new MySqlCommand())
                        {
                            cmd.Connection = conn;
                            cmd.CommandText = "SELECT MAX(orderNumber + 1) FROM ORDERS;";
                            orderNumber = Convert.ToInt32(cmd.ExecuteScalar() ?? 1);
                        }

                        using (var cmd = new MySqlCommand())
                        {
                            cmd.Connection = conn;
                            cmd.CommandText = @"INSERT INTO orders(orderNumber, orderDate, requiredDate, shippedDate, status, comments, customerNumber) VALUES(@on, @od, @rd, @sd, @s, @c, @cn)";
                            cmd.Parameters.AddWithValue("on", orderNumber);
                            cmd.Parameters.AddWithValue("od", DateTime.Now);
                            cmd.Parameters.AddWithValue("rd", order.RequiredDate);
                            cmd.Parameters.AddWithValue("sd", null);
                            cmd.Parameters.AddWithValue("s", "In Process");
                            cmd.Parameters.AddWithValue("c", order.Comments);
                            cmd.Parameters.AddWithValue("cn", order.CustomerNumber);
                            cmd.ExecuteNonQuery();
                        }

                        foreach(OrderDetail orderDetail in order.OrderDetails)
                        {
                            using (var cmd = new MySqlCommand())
                            {
                                cmd.Connection = conn;
                                cmd.CommandText = "INSERT INTO orderdetails(orderNumber, orderLineNumber, priceEach, productCode, quantityOrdered) VALUES(@on, @oln, @pe, @pc, @qo)";
                                cmd.Parameters.AddWithValue("on", orderNumber);
                                cmd.Parameters.AddWithValue("oln", orderDetail.OrderLineNumber);
                                cmd.Parameters.AddWithValue("pe", orderDetail.PriceEach);
                                cmd.Parameters.AddWithValue("pc", orderDetail.ProductCode);
                                cmd.Parameters.AddWithValue("qo", orderDetail.QuantityOrdered);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                        return 1;

                    }
                    catch(Exception ex)
                    {
                        transaction.Rollback();
                        logger.Error(ex.Message, ex);
                    }
                }
            }
            return 0;
        }
    }
}

