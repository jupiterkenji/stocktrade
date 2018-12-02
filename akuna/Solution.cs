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

            commander = new Commander(this);
        }

        public string Process(string input)
        {
            var inputAsArray = input.Split(' ');
            return commander.Process(inputAsArray);
        }

        #region Implementation

        public List<Order> BuyList {get; private set;}
        public List<Order> SellList {get; private set;}

        Commander commander;

        #region Helper

        static class Helper
        {
            public static void Remove(string orderID, List<Order> buyList, List<Order> sellList)
            {
                var foundOrder = buyList.FirstOrDefault(buyOrder => buyOrder.OrderID == orderID);
                if (foundOrder != null)
                {
                    buyList.Remove(foundOrder);
                }
                else
                {
                    foundOrder = sellList.FirstOrDefault(sellOrder => sellOrder.OrderID == orderID);
                    if (foundOrder != null)
                    {
                        sellList.Remove(foundOrder);
                    }
                }
            }
        }

        #endregion

        #region Matcher

        class MatcherProvider
        {
            public MatcherProvider(Solution2 solution, string orderType)
            {
                switch(orderType)
                {
                    case GFDMatcher.Code:
                        matcher = new GFDMatcher(solution);
                        break;
                    case IOCMatcher.Code:
                        matcher = new IOCMatcher(solution);
                        break;
                }
            }

            public string Matching(string command, Order order)
            {
                return matcher?.Matching(command, order) ?? string.Empty;
            }

            MatcherBase matcher;
        }

        abstract class MatcherBase
        {
            public MatcherBase(Solution2 solution)
            {
                this.solution = solution;
            }

            protected Solution2 solution;

            public string Matching(string command, Order order)
            {
                var result = string.Empty;

                switch (command)
                {
                    case SellCommand.SellCode:
                        result = MatchingCore(solution.BuyList, solution.SellList, order, command);
                        break;
                    case BuyCommand.BuyCode:
                        result = MatchingCore(solution.SellList, solution.BuyList, order, command);
                        break;
                }

                return result;
            }

            abstract protected string MatchingCore(List<Order> matchingList, List<Order> otherList, Order order, string command);

            protected IEnumerable<Order> FindMatchingOrders(IEnumerable<Order> sourceList, Order order, string command)
            {
                var results = new List<Order>();
                var quantityToFulfill = order.Quantity;
                var sortedList = GetSortedList(sourceList, command);

                foreach (var source in sortedList)
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

             IEnumerable<Order> GetSortedList(IEnumerable<Order> sourceList, string command)
             {
                var sortedList = Enumerable.Empty<Order>();
                switch (command)
                {
                    case BuyCommand.BuyCode:
                        sortedList = sourceList.OrderBy(sourceOrder => sourceOrder.Price);
                        break;
                    case SellCommand.SellCode:
                        sortedList = sourceList.OrderByDescending(sourceOrder => sourceOrder.Price);
                        break;
                }

                return sortedList;
             }

            bool IsPriceMatched(Order source, Order order, string command)
            {
                switch (command)
                {
                    case BuyCommand.BuyCode:
                        return source.Price <= order.Price;
                    case SellCommand.SellCode:
                        return source.Price >= order.Price;
                }

                return false;
            }
        }

        class GFDMatcher: MatcherBase
        {
            public GFDMatcher(Solution2 solution) : base (solution) {}

            public const string Code = "GFD";

            protected override string MatchingCore(List<Order> matchingList, List<Order> otherList, Order order, string command)
            {
                var result = string.Empty;
                var matchingOrders = FindMatchingOrders(matchingList, order, command);

                if (matchingOrders.Any())
                {
                    var resultList = new List<string>();
                    foreach (var matchingOrder in matchingOrders)
                    {
                        var isAllQuantityMatched = order.Quantity >= matchingOrder.Quantity;

                        if(isAllQuantityMatched)
                        {
                            var matchedQuantity = matchingOrder.Quantity;
                            resultList.Add($"TRADE {matchingOrder.OrderID} {matchingOrder.Price} {matchedQuantity} {order.OrderID} {order.Price} {matchedQuantity}");
                            matchingList.Remove(matchingOrder);
                            order.Quantity -= matchedQuantity;
                        }
                        else
                        {
                            var matchedQuantity = order.Quantity;
                            resultList.Add($"TRADE {matchingOrder.OrderID} {matchingOrder.Price} {matchedQuantity} {order.OrderID} {order.Price} {matchedQuantity}");
                            matchingOrder.Quantity -= matchedQuantity;
                            order.Quantity -= matchedQuantity;
                            break;
                        }
                    }
                    result = string.Join("\r\n", resultList);
                }

                if (order.Quantity > 0)
                {
                    otherList.Add(order);
                }

                return result;
            }
        }

        class IOCMatcher: MatcherBase
        {
            public IOCMatcher(Solution2 solution) : base (solution) {}

            public const string Code = "IOC";

            protected override string MatchingCore(List<Order> matchingList, List<Order> otherList, Order order, string command)
            {
                var result = string.Empty;
                var matchingOrders = FindMatchingOrders(matchingList, order, command);

                if (matchingOrders.Any())
                {
                    var resultList = new List<string>();
                    foreach (var matchingOrder in matchingOrders)
                    {
                        var isAllQuantityMatched = order.Quantity >= matchingOrder.Quantity;
                        if(isAllQuantityMatched)
                        {
                            var matchedQuantity = matchingOrder.Quantity;
                            resultList.Add($"TRADE {matchingOrder.OrderID} {matchingOrder.Price} {matchedQuantity} {order.OrderID} {order.Price} {matchedQuantity}");
                            matchingList.Remove(matchingOrder);
                            order.Quantity -= matchedQuantity;
                        }
                        else
                        {
                            var matchedQuantity = order.Quantity;
                            resultList.Add($"TRADE {matchingOrder.OrderID} {matchingOrder.Price} {matchedQuantity} {order.OrderID} {order.Price} {matchedQuantity}");
                            matchingOrder.Quantity -= matchedQuantity;
                            order.Quantity -= matchedQuantity;
                            break;
                        }
                    }
                    result = string.Join("\r\n", resultList);
                }

                return result;
            }
        }

        #endregion

        #region Commands

        class Commander
        {
            public Commander(Solution2 solution)
            {
                commandList = new List<CommandBase>();
                commandList.Add(new SellCommand(solution));
                commandList.Add(new BuyCommand(solution));
                commandList.Add(new CancelCommand(solution));
                commandList.Add(new ModifyCommand(solution));
                commandList.Add(new PrintCommand(solution));
            }

            public string Process(string[] inputAsArray)
            {
                var command = commandList.FirstOrDefault(c => c.ShouldProcess(inputAsArray));
                return command.Process(inputAsArray);
            }

            List<CommandBase> commandList;
        }

        abstract class CommandBase
        {
            public CommandBase (Solution2 solution)
            {
                this.solution = solution;
            }

            public bool ShouldProcess(string[] inputAsArray)
            {
                return inputAsArray.Length > 0 && inputAsArray[0] == Code; 
            }

            public abstract string Code {get;}
            public abstract string Process(string[] inputAsArray);
            protected Solution2 solution;
        }

        abstract class BuySellCommand: CommandBase
        {
            public BuySellCommand (Solution2 solution): base (solution) {}

            public override string Process(string[] inputAsArray)
            {
                if (inputAsArray.Length == 5)
                {
                    var command = inputAsArray[0];
                    var orderType = inputAsArray[1];
                    var price = int.Parse(inputAsArray[2]);
                    var quantity = int.Parse(inputAsArray[3]);
                    var orderID = inputAsArray[4];
                    var order = new Order(orderType, price, quantity, orderID);

                    if (order.Validate())
                    {
                        var matcherProvider = new MatcherProvider(solution, orderType);
                        return matcherProvider.Matching(command, order);
                    }
                }

                return string.Empty;
            }
        }

        class BuyCommand: BuySellCommand
        {
            public BuyCommand (Solution2 solution): base (solution) {}

            public override string Code => BuyCode;

            public const string BuyCode = "BUY";
        }

        class SellCommand: BuySellCommand
        {
            public SellCommand (Solution2 solution): base (solution) {}

            public override string Code => SellCode;

            public const string SellCode = "SELL";
        }

        class CancelCommand: CommandBase
        {
            public CancelCommand (Solution2 solution): base (solution) {}

            public override string Code => "CANCEL";

            public override string Process(string[] inputAsArray)
            {
                if (inputAsArray.Length == 2)
                {
                    var orderID = inputAsArray[1];
                    Helper.Remove(orderID, solution.BuyList, solution.SellList);
                }

                return string.Empty;
            }
        }

        class ModifyCommand: CommandBase
        {
            public ModifyCommand (Solution2 solution): base (solution) {}

            public override string Code => "MODIFY";

            public override string Process(string[] inputAsArray)
            {
                if (inputAsArray.Length == 5)
                {
                    var orderID = inputAsArray[1];

                    var existingOrder = FindOrder(orderID);
                    if (existingOrder != null)
                    {
                        Helper.Remove(orderID, solution.BuyList, solution.SellList);

                        var newCommand = inputAsArray[2];

                        var orderType = existingOrder.OrderType;
                        var price = int.Parse(inputAsArray[3]);
                        var quantity = int.Parse(inputAsArray[4]);

                        var order = new Order(orderType, price, quantity, orderID);

                        if (order.Validate())
                        {
                            var matcherProvider = new MatcherProvider(solution, orderType);
                            return matcherProvider.Matching(newCommand, order);
                        }
                    }
                }
                
                return string.Empty;
            }

            Order FindOrder(string orderID)
            {
                var foundOrder = solution.BuyList.FirstOrDefault(buyOrder => buyOrder.OrderID == orderID);
                if (foundOrder == null)
                {
                    foundOrder = solution.SellList.FirstOrDefault(sellOrder => sellOrder.OrderID == orderID);
                }

                return foundOrder;
            }
        }

        class PrintCommand: CommandBase
        {
            public PrintCommand (Solution2 solution): base (solution) {}

            public override string Code => "PRINT";

            public override string Process(string[] inputAsArray)
            {
                var result = new List<string>();

                result.Add("SELL:");
                foreach (var sellOrder in solution.SellList.GroupBy(tempOrder => tempOrder.Price).OrderByDescending(order => order.Key))
                {
                    var quantity = 0;
                    foreach (var tempOrder in sellOrder.OrderByDescending(order => order.Quantity))
                    {
                        quantity += tempOrder.Quantity;
                    }
                    result.Add($"{sellOrder.Key} {quantity}");
                }

                result.Add("BUY:");
                foreach (var buyOrder in solution.BuyList.GroupBy(tempOrder => tempOrder.Price).OrderByDescending(order => order.Key))
                {
                    var quantity = 0;
                    foreach (var tempOrder in buyOrder)
                    {
                        quantity += tempOrder.Quantity;
                    }
                    result.Add($"{buyOrder.Key} {quantity}");
                }

                return string.Join("\r\n", result);
            }
        }

        #endregion

        public class Order
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

            public bool Validate()
            {
                if (Quantity < 1)
                {
                    return false;
                }

                if (Price < 1)
                {
                    return false;
                }

                return true;
            }
        }

        enum OrderType {IOC, GFD};

        #endregion
    }
}
