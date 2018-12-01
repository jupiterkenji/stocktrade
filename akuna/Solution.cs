using System;
using System.Collections.Generic;
using System.Linq;

namespace akuna
{
    public class Solution2
    {
        public Solution2()
        {
            BuyList = new List<Order>();
            SellList = new List<Order>();
        }

        public string Process(string input)
        {
            var result = string.Empty;

            var inputAsArray = input.Split(' ');
            if (inputAsArray.Length > 0)
            {
                var command = inputAsArray[0];
                switch (command)
                {
                    case Buy:
                    case Sell:
                        if (inputAsArray.Length == 5)
                        {
                            var orderType = inputAsArray[1];
                            var price = int.Parse(inputAsArray[2]);
                            var quantity = int.Parse(inputAsArray[3]);
                            var orderID = inputAsArray[4];
                            var order = new Order(orderType, price, quantity, orderID);

                            if (Validate(order))
                            {
                                result = Matching(command, order);
                            }
                        }
                        break;
                    case Cancel:
                        if (inputAsArray.Length == 2)
                        {
                            var orderID = inputAsArray[1];
                            Remove(orderID);
                        }
                        break;
                    case Modify:
                        if (inputAsArray.Length == 5)
                        {
                            var orderID = inputAsArray[1];

                            var foundOrder = FindOrder(orderID);
                            if (foundOrder != null)
                            {
                                Remove(orderID); // jk-todo: may be udpate and changen position?

                                var newCommand = inputAsArray[2];

                                var orderType = foundOrder.OrderType;
                                var price = int.Parse(inputAsArray[3]);
                                var quantity = int.Parse(inputAsArray[4]);
                                var order = new Order(orderType, price, quantity, orderID);
                                if (Validate(order))
                                {
                                    result = Matching(newCommand, order);
                                }
                            }
                        }
                        break;
                    case Print:
                        var printResult = new List<string>();

                        printResult.Add("SELL:");
                        foreach (var sellOrder in SellList.GroupBy(tempOrder => tempOrder.Price).OrderByDescending(order => order.Key))
                        {
                            var quantity = 0;
                            foreach (var tempOrder in sellOrder.OrderByDescending(order => order.Quantity))
                            {
                                quantity += tempOrder.Quantity;
                            }
                            printResult.Add($"{sellOrder.Key} {quantity}");
                        }

                        printResult.Add("BUY:");
                        foreach (var buyOrder in BuyList.GroupBy(tempOrder => tempOrder.Price).OrderByDescending(order => order.Key))
                        {
                            var quantity = 0;
                            foreach (var tempOrder in buyOrder)
                            {
                                quantity += tempOrder.Quantity;
                            }
                            printResult.Add($"{buyOrder.Key} {quantity}");
                        }

                        result = string.Join("\r\n", printResult);
                        break;
                }
            }

            return result;
        }

        string Matching(string command, Order order)
        {
            switch(order.OrderType)
            {
                case GFD:
                    return MatchingGFD(command, order);
                case IOC:
                    return MatchingIOC(command, order);
            }

            return string.Empty;
        }

        string MatchingGFD(string command, Order order)
        {
            var result = string.Empty;

            switch (command)
            {
                case Sell:
                    result = MatchingGFDCore(BuyList, SellList, order, command);
                    break;
                case Buy:
                    result = MatchingGFDCore(SellList, BuyList, order, command);
                    break;
            }

            return result;
        }

        string MatchingGFDCore(List<Order> matchingList, List<Order> otherList, Order order, string command)
        {
            var result = string.Empty;
            var matchingOrders = FindMatchingOrders(matchingList, order, command);

            if (matchingOrders.Any())
            {
                var resultList = new List<string>();
                foreach (var matchingOrder in matchingOrders)
                {
                    var matchedQuantity = 0;
                    var isAllQuantityFulfilled = order.Quantity >= matchingOrder.Quantity;

                    if(isAllQuantityFulfilled)
                    {
                        matchedQuantity = matchingOrder.Quantity;
                        resultList.Add($"TRADE {matchingOrder.OrderID} {matchingOrder.Price} {matchedQuantity} {order.OrderID} {order.Price} {matchedQuantity}");
                        matchingList.Remove(matchingOrder);
                    }
                    else
                    {
                        matchedQuantity = order.Quantity;
                        resultList.Add($"TRADE {matchingOrder.OrderID} {matchingOrder.Price} {matchedQuantity} {order.OrderID} {order.Price} {matchedQuantity}");
                        matchingOrder.Quantity -= matchedQuantity;
                        order.Quantity -= matchedQuantity;
                        break;
                    }
                    order.Quantity -= matchedQuantity;
                }
                result = string.Join("\r\n", resultList);
            }

            if (order.Quantity > 0)
            {
                otherList.Add(order);
            }

            return result;
        }

        IEnumerable<Order> FindMatchingOrders(IEnumerable<Order> sourceList, Order order, string command)
        {
            var results = new List<Order>();

            var quantityToFulfill = order.Quantity;
            foreach (var source in sourceList.OrderByDescending(sourceOrder => sourceOrder.Price))
            {
                var isPriceMatched = IsPriceMatched(source, order, command);
                if (isPriceMatched)
                {
                    quantityToFulfill -= source.Quantity;
                    results.Add(source);

                    if (quantityToFulfill < 1)
                    {
                        break;
                    }
                }
            }

            return results;
        }

        bool IsPriceMatched(Order source, Order order, string command)
        {
            switch (command)
            {
                case Sell:
                    return source.Price >= order.Price;
                case Buy:
                    return source.Price <= order.Price;
            }

            return false;
        }

        string MatchingIOC(string command, Order order)
        {
            var result = string.Empty;

            switch (command)
            {
                case Sell:
                    result = MatchingIOCCore(BuyList, SellList, order, command);
                    break;
                case Buy:
                    result = MatchingIOCCore(SellList, BuyList, order, command);
                    break;
            }

            return result;
        }

        string MatchingIOCCore(List<Order> matchingList, List<Order> otherList, Order order, string command)
        {
            var result = string.Empty;
            var matchingOrders = FindMatchingOrders(matchingList, order, command);

            if (matchingOrders.Any())
            {
                var resultList = new List<string>();
                foreach (var matchingOrder in matchingOrders)
                {
                    if(order.Quantity >= matchingOrder.Quantity)
                    {
                        order.Quantity -= matchingOrder.Quantity;
                        var matchedQuantity = matchingOrder.Quantity;
                        resultList.Add($"TRADE {matchingOrder.OrderID} {matchingOrder.Price} {matchedQuantity} {order.OrderID} {order.Price} {matchedQuantity}");
                        matchingList.Remove(matchingOrder);
                    }
                    else
                    {
                        // only partial matched
                        var matchedQuantity = order.Quantity;

                        order.Quantity = 0 ;
                        matchingOrder.Quantity -= order.Quantity;

                        resultList.Add($"TRADE {matchingOrder.OrderID} {matchingOrder.Price} {matchedQuantity} {order.OrderID} {order.Price} {matchedQuantity}");
                    }
                }
                result = string.Join("\r\n", resultList);
            }

            return result;
        }

        Order FindOrder(string orderID)
        {
            var foundOrder = BuyList.FirstOrDefault(buyOrder => buyOrder.OrderID == orderID);
            if (foundOrder == null)
            {
                foundOrder = SellList.FirstOrDefault(sellOrder => sellOrder.OrderID == orderID);
            }

            return foundOrder;
        }

        void Remove(string orderID)
        {
            var foundOrder = BuyList.FirstOrDefault(buyOrder => buyOrder.OrderID == orderID);
            if (foundOrder != null)
            {
                BuyList.Remove(foundOrder);
            }

            foundOrder = SellList.FirstOrDefault(sellOrder => sellOrder.OrderID == orderID);
            if (foundOrder != null)
            {
                SellList.Remove(foundOrder);
            }
        }

        bool Validate(Order order)
        {
            if (order.Quantity < 1)
            {
                return false;
            }

            if (order.Price < 1)
            {
                return false;
            }

            return true;
        }

        #region Implementation

        List<Order> BuyList;
        List<Order> SellList;

        const string Buy = "BUY";
        const string Sell = "SELL";
        const string Cancel = "CANCEL";
        const string Modify = "MODIFY";
        const string Print = "PRINT";

        const string IOC = "IOC";
        const string GFD = "GFD";

        class Order
        {
            public Order(string orderType, int price, int quantity, string orderID)
            {
                OrderType = orderType;
                Price = price;
                Quantity = quantity;
                OrderID = orderID;
            }

            public string OrderType {get; set;}
            public int Price {get; set;}
            public int Quantity {get; set;}

            public string OrderID {get;set;}

            //jk-todo: sequence ... need linked list
        }

        enum OrderType {IOC, GFD};

        #endregion
    }
}
