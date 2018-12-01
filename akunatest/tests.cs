using System;
using Xunit;
using akuna;

namespace akunatest
{
    public class TestClass1
    {
        #region Example Tests

        [Fact]
        public void Example1Test()
        {
            var solution = new Solution2();

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
            var solution = new Solution2();

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
            var solution = new Solution2();

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
            var solution = new Solution2();

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
            var solution = new Solution2();

            solution.Process("BUY GFD 1000 10 order1");
            solution.Process("BUY GFD 1010 10 order2");
            var output = solution.Process("SELL GFD 1000 15 order3");

            var expectedOutput =
@"TRADE order2 1010 10 order3 1000 10
TRADE order1 1000 5 order3 1000 5";
            Assert.Equal(expectedOutput, output);
        }

        #endregion

        #region IOC

        [Fact]
        public void ModifyIOCTest()
        {
            var solution = new Solution2();

            solution.Process("BUY IOC 1000 10 order1");
            var output = solution.Process("PRINT");
            var expectedOutput =
@"SELL:
BUY:";
            Assert.Equal(expectedOutput, output);

            solution.Process("SELL GFD 1000 30 order2");
            output = solution.Process("PRINT");
            expectedOutput =
@"SELL:
1000 30
BUY:";
            Assert.Equal(expectedOutput, output);

            output = solution.Process("BUY IOC 900 10 order3");
            Assert.Equal(string.Empty, output);

            output = solution.Process("PRINT");
            expectedOutput =
@"SELL:
1000 30
BUY:";
            Assert.Equal(expectedOutput, output);

            output = solution.Process("BUY IOC 1001 10 order4");
            expectedOutput = @"TRADE order2 1000 10 order4 1001 10";
            Assert.Equal(expectedOutput, output);

            output = solution.Process("PRINT");
            expectedOutput =
@"SELL:
1000 20
BUY:";
            Assert.Equal(expectedOutput, output);

            output = solution.Process("BUY IOC 10000 20 order5");
            expectedOutput = @"TRADE order2 1000 20 order5 10000 20";
            Assert.Equal(expectedOutput, output);

            output = solution.Process("PRINT");
            expectedOutput =
@"SELL:
BUY:";
            Assert.Equal(expectedOutput, output);
        }

        [Fact]
        public void ModifyIOCTest_Simple()
        {
            var solution = new Solution2();

            solution.Process("BUY IOC 1000 10 order1");
            solution.Process("BUY GFD 1000 10 order2");
            solution.Process("MODIFY order1 BUY 1000 20"); // IOC cannot be modified
            var output = solution.Process("SELL GFD 900 20 order3");

            var expectedOutput = @"TRADE order2 1000 10 order3 900 10";
            Assert.Equal(expectedOutput, output);
        }

        [Fact]
        public void BuyIOCTest()
        {
            var solution = new Solution2();

            solution.Process("BUY IOC 1000 30 order1");

            var output = solution.Process("SELL GFD 900 20 order2");
            Assert.Equal(string.Empty, output);

            output = solution.Process("PRINT");
            var expectedOutput =
@"SELL:
900 20
BUY:";
            Assert.Equal(expectedOutput, output);
        }

        #endregion

        #region Others tests

        [Fact]
        public void TradeOrderTest()
        {
            var solution = new Solution2();

            solution.Process("SELL GFD 900 10 order2");
            var output = solution.Process("BUY GFD 1000 10 order1");

            var expectedOutput = @"TRADE order2 900 10 order1 1000 10";
            Assert.Equal(expectedOutput, output);
        }

        [Fact]
        public void FairTest()
        {
            var solution = new Solution2();

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
            var solution = new Solution2();

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
        public void NegativeTest()
        {
            var solution = new Solution2();

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

        [Fact]
        public void CancelTest()
        {
            var solution = new Solution2();

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

        #endregion

        [Fact]
        public void CompletePartialTrade_Sell()
        {
            var solution = new Solution2();

            solution.Process("BUY GFD 1000 30 order1");

            var output = solution.Process("SELL GFD 900 10 order2");
            var expectedOutput =@"TRADE order1 1000 10 order2 900 10";
            Assert.Equal(expectedOutput, output);

            output = solution.Process("SELL GFD 905 5 order3");
            expectedOutput =@"TRADE order1 1000 5 order3 905 5";
            Assert.Equal(expectedOutput, output);

            output = solution.Process("SELL GFD 2000 5 order4");
            Assert.Equal(string.Empty, output);

            output = solution.Process("SELL GFD 1000 5 order5");
            expectedOutput =@"TRADE order1 1000 5 order5 1000 5";
            Assert.Equal(expectedOutput, output);
            output = solution.Process("PRINT");
            expectedOutput =
@"SELL:
2000 5
BUY:
1000 10";
            Assert.Equal(expectedOutput, output);

            output = solution.Process("SELL GFD 909 10 order6");
            expectedOutput =@"TRADE order1 1000 10 order6 909 10";
            Assert.Equal(expectedOutput, output);

            output = solution.Process("PRINT");
            expectedOutput =
@"SELL:
2000 5
BUY:";
            Assert.Equal(expectedOutput, output);
       }

       [Fact]
        public void CompletePartialTrade_Buy()
        {
            var solution = new Solution2();

            solution.Process("SELL GFD 1000 30 order1");

            var output = solution.Process("BUY GFD 2000 10 order2");
            var expectedOutput =@"TRADE order1 1000 10 order2 2000 10";
            Assert.Equal(expectedOutput, output);

            output = solution.Process("BUY GFD 3000 5 order3");
            expectedOutput =@"TRADE order1 1000 5 order3 3000 5";
            Assert.Equal(expectedOutput, output);

            output = solution.Process("BUY GFD 20 5 order4");
            Assert.Equal(string.Empty, output);

            output = solution.Process("BUY GFD 4000 5 order5");
            expectedOutput =@"TRADE order1 1000 5 order5 4000 5";
            Assert.Equal(expectedOutput, output);
            output = solution.Process("PRINT");
            expectedOutput =
@"SELL:
1000 10
BUY:
20 5";
            Assert.Equal(expectedOutput, output);

            output = solution.Process("BUY GFD 5000 10 order6");
            expectedOutput =@"TRADE order1 1000 10 order6 5000 10";
            Assert.Equal(expectedOutput, output);

            output = solution.Process("PRINT");
            expectedOutput =
@"SELL:
BUY:
20 5";
            Assert.Equal(expectedOutput, output);
       }
    }
}