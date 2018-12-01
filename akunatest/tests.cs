using System;
using Xunit;
using akuna;

namespace akunatest
{
    public class TestClass1
    {
        [Fact]
        public void Example1Test()
        {
            var solution = new Solution();

            solution.Process("BUY GFD 1000 10 order1");
            var output = solution.Process("PRINT");

            var expectedOutput =
@"SELL:
BUY:
1000 10";
            Assert.Equal(expectedOutput, output);
        }

        [Fact]
        public void Example2Test()
        {
            var solution = new Solution();

            solution.Process("BUY GFD 1000 10 order1");
            solution.Process("BUY GFD 1000 20 order2");
            var output = solution.Process("PRINT");

            var expectedOutput =
@"SELL:
BUY:
1000 30";
            Assert.Equal(expectedOutput, output);
        }

        
        [Fact]
        public void Example3Test()
        {
            var solution = new Solution();

            solution.Process("BUY GFD 1000 10 order1");
            solution.Process("BUY GFD 1001 20 order2");
            var output = solution.Process("PRINT");

            var expectedOutput =
@"SELL:
BUY:
1001 20
1000 10";
            Assert.Equal(expectedOutput, output);
        }

        [Fact]
        public void Example4Test()
        {
            var solution = new Solution();

            solution.Process("BUY GFD 1000 10 order1");

            var output = solution.Process("SELL GFD 900 20 order2");
            Assert.Equal("TRADE order1 1000 10 order2 900 10", output);

            output = solution.Process("PRINT");
            var expectedOutput =
@"SELL:
900 10
BUY:";
            Assert.Equal(expectedOutput, output);
        }

        [Fact]
        public void Example5Test()
        {
            var solution = new Solution();

            solution.Process("BUY GFD 1000 10 order1");
            solution.Process("BUY GFD 1010 10 order2");
            var output = solution.Process("SELL GFD 1000 15 order3");

            var expectedOutput =
@"TRADE order2 1010 10 order3 1000 10
TRADE order1 1000 5 order3 1000 5";
            Assert.Equal(expectedOutput, output);
        }

        [Fact]
        public void TradeOrderTest()
        {
            var solution = new Solution();

            solution.Process("SELL GFD 900 10 order2");
            var output = solution.Process("BUY GFD 1000 10 order1");

            var expectedOutput = @"TRADE order2 900 10 order1 1000 10";
            Assert.Equal(expectedOutput, output);
        }

        [Fact]
        public void FairTest()
        {
            var solution = new Solution();

            solution.Process("BUY GFD 1000 10 order1");
            solution.Process("BUY GFD 1000 10 order2");
            var output = solution.Process("SELL GFD 900 20 order3");

            var expectedOutput =
@"TRADE order1 1000 10 order3 900 10
TRADE order2 1000 10 order3 900 10";
            Assert.Equal(expectedOutput, output);
        }

        [Fact]
        public void ModifyTest()
        {
            var solution = new Solution();

            solution.Process("BUY GFD 1000 10 order1");
            solution.Process("BUY GFD 1000 10 order2");
            solution.Process("MODIFY order1 BUY 1000 20");
            var output = solution.Process("SELL GFD 900 20 order3");

            var expectedOutput =
@"TRADE order2 1000 10 order3 900 10
TRADE order1 1000 10 order3 900 10";
            Assert.Equal(expectedOutput, output);
        }

        [Fact]
        public void ModifyIOCTest()
        {
            var solution = new Solution();

            solution.Process("BUY IOC 1000 10 order1");
            solution.Process("BUY GFD 1000 10 order2");
            solution.Process("MODIFY order1 BUY 1000 20"); // IOC cannot be modified
            var output = solution.Process("SELL GFD 900 20 order3");

            var expectedOutput = @"TRADE order2 1000 10 order3 900 10";
            Assert.Equal(expectedOutput, output);
        }

        public void NegativeTest()
        {
            var solution = new Solution();

            solution.Process("BUY GFD -1000 10 order1");
            var output = solution.Process("PRINT");
            var expectedOutput = @"SELL:
BUY:";
            Assert.Equal(expectedOutput, output);

            output = solution.Process("SELL GFD 900 -20 order2");
            Assert.Equal(string.Empty, output);

            solution.Process("MODIFY order1 BUY -1000 20"); // IOC cannot be modified
            output = solution.Process("PRINT");
            expectedOutput = @"SELL:
BUY:";
            Assert.Equal(expectedOutput, output);
        }

        public void CancelTest()
        {
            var solution = new Solution();

            solution.Process("BUY GFD 1000 10 order1");
            solution.Process("CANCEL order1");

            var output = solution.Process("SELL GFD 900 20 order2");
            Assert.Equal(string.Empty, output);

            output = solution.Process("PRINT");
            var expectedOutput =
@"SELL:
900 20
BUY:";
            Assert.Equal(expectedOutput, output);
        }

        public void BuyIOCTest()
        {
            var solution = new Solution();

            solution.Process("BUY IOC 1000 30 order1");

            var output = solution.Process("SELL GFD 900 20 order2");
            Assert.Equal("TRADE order1 900 30 order2 900 20", output);

            output = solution.Process("PRINT");
            var expectedOutput =
@"SELL:
BUY:";
            Assert.Equal(expectedOutput, output);
        }
    }
}
