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

        #region Implementation

        public List<Order> BuyList {get; private set;}
        public List<Order> SellList {get; private set;}

        Commander commander;

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
                            matchingOrder.Quantity -= matchedQuantity;

                            resultList.Add($"TRADE {matchingOrder.OrderID} {matchingOrder.Price} {matchedQuantity} {order.OrderID} {order.Price} {matchedQuantity}");
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
                var result = string.Empty;

                var command = inputAsArray[0];
                var orderType = inputAsArray[1];
                var price = int.Parse(inputAsArray[2]);
                var quantity = int.Parse(inputAsArray[3]);
                var orderID = inputAsArray[4];
                var order = new Order(orderType, price, quantity, orderID);

                if (order.Validate())
                {
                    var matcherProvider = new MatcherProvider(solution, orderType);
                    result = matcherProvider.Matching(command, order);
                }

                return result;
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
                    solution.Remove(orderID);
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
                var result = string.Empty;

                if (inputAsArray.Length == 5)
                {
                    var orderID = inputAsArray[1];

                    var foundOrder = FindOrder(orderID);
                    if (foundOrder != null)
                    {
                        solution.Remove(orderID);

                        var newCommand = inputAsArray[2];

                        var orderType = foundOrder.OrderType;
                        var price = int.Parse(inputAsArray[3]);
                        var quantity = int.Parse(inputAsArray[4]);
                        var order = new Order(orderType, price, quantity, orderID);
                        if (order.Validate())
                        {
                            var matcherProvider = new MatcherProvider(solution, orderType);
                            result = matcherProvider.Matching(newCommand, order);
                        }
                    }
                }
                
                return result;
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
                var printResult = new List<string>();

                printResult.Add("SELL:");
                foreach (var sellOrder in solution.SellList.GroupBy(tempOrder => tempOrder.Price).OrderByDescending(order => order.Key))
                {
                    var quantity = 0;
                    foreach (var tempOrder in sellOrder.OrderByDescending(order => order.Quantity))
                    {
                        quantity += tempOrder.Quantity;
                    }
                    printResult.Add($"{sellOrder.Key} {quantity}");
                }

                printResult.Add("BUY:");
                foreach (var buyOrder in solution.BuyList.GroupBy(tempOrder => tempOrder.Price).OrderByDescending(order => order.Key))
                {
                    var quantity = 0;
                    foreach (var tempOrder in buyOrder)
                    {
                        quantity += tempOrder.Quantity;
                    }
                    printResult.Add($"{buyOrder.Key} {quantity}");
                }

                return string.Join("\r\n", printResult);
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
