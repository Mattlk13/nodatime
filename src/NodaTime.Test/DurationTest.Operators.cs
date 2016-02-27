// Copyright 2009 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using System;
using NUnit.Framework;
using System.Numerics;

namespace NodaTime.Test
{
    partial class DurationTest
    {
        private readonly Duration threeMillion = Duration.FromNanoseconds(3000000L);
        private readonly Duration negativeFiftyMillion = Duration.FromNanoseconds(-50000000L);

        #region operator +
        [Test]
        public void OperatorPlus_Zero_IsNeutralElement()
        {
            Assert.AreEqual(0L, (Duration.Zero + Duration.Zero).ToInt64Nanoseconds(), "0 + 0");
            Assert.AreEqual(1L, (Duration.Epsilon + Duration.Zero).ToInt64Nanoseconds(), "1 + 0");
            Assert.AreEqual(1L, (Duration.Zero + Duration.Epsilon).ToInt64Nanoseconds(), "0 + 1");
        }

        [Test]
        public void OperatorPlus_NonZero()
        {
            Assert.AreEqual(3000001L, (threeMillion + Duration.Epsilon).ToInt64Nanoseconds(), "3,000,000 + 1");
            Assert.AreEqual(0L, (Duration.Epsilon + Duration.FromNanoseconds(-1)).ToInt64Nanoseconds(), "1 + (-1)");
            Assert.AreEqual(-49999999L, (negativeFiftyMillion + Duration.Epsilon).ToInt64Nanoseconds(), "-50,000,000 + 1");
        }

        [Test]
        public void OperatorPlus_MethodEquivalents()
        {
            Duration x = Duration.FromNanoseconds(100);
            Duration y = Duration.FromNanoseconds(200);
            Assert.AreEqual(x + y, Duration.Add(x, y));
            Assert.AreEqual(x + y, x.Plus(y));
        }
        #endregion

        #region operator -
        [Test]
        public void OperatorMinus_Zero_IsNeutralElement()
        {
            Assert.AreEqual(0L, (Duration.Zero - Duration.Zero).ToInt64Nanoseconds(), "0 - 0");
            Assert.AreEqual(1L, (Duration.Epsilon - Duration.Zero).ToInt64Nanoseconds(), "1 - 0");
            Assert.AreEqual(-1L, (Duration.Zero - Duration.Epsilon).ToInt64Nanoseconds(), "0 - 1");
        }

        [Test]
        public void OperatorMinus_NonZero()
        {
            Duration negativeEpsilon = Duration.FromNanoseconds(-1L);
            Assert.AreEqual(2999999L, (threeMillion - Duration.Epsilon).ToInt64Nanoseconds(), "3,000,000 - 1");
            Assert.AreEqual(2L, (Duration.Epsilon - negativeEpsilon).ToInt64Nanoseconds(), "1 - (-1)");
            Assert.AreEqual(-50000001L, (negativeFiftyMillion - Duration.Epsilon).ToInt64Nanoseconds(), "-50,000,000 - 1");
        }

        [Test]
        public void OperatorMinus_MethodEquivalents()
        {
            Duration x = Duration.FromNanoseconds(100);
            Duration y = Duration.FromNanoseconds(200);
            Assert.AreEqual(x - y, Duration.Subtract(x, y));
            Assert.AreEqual(x - y, x.Minus(y));
        }
        #endregion

        #region operator /
        [Test]
        public void OperatorDivision_ByNonZero()
        {
            Assert.AreEqual(1000, (threeMillion / 3000).ToInt64Nanoseconds(), "3000000 / 3000");
        }

        [Test]
        public void OperatorDivision_ByZero_Throws()
        {
            Assert.Throws<DivideByZeroException>(() => (threeMillion / 0).ToString(), "3000000 / 0");
        }

        [Test]
        public void OperatorDivision_Truncates()
        {
            Assert.AreEqual(1L, (threeMillion / 2000000).ToInt64Nanoseconds(), "3000000 / 2000000");
            Assert.AreEqual(-1L, (threeMillion / -2000000).ToInt64Nanoseconds(), "3000000 / -2000000");
        }

        [Test]
        public void OperatorDivision_MethodEquivalent()
        {
            Assert.AreEqual(threeMillion / 2000000, Duration.Divide(threeMillion, 2000000));
        }
        #endregion

        #region operator *
        [Test]
        // "Old" non-zero non-one test cases for posterity's sake.
        [TestCase(0, 3000, 1000)]
        [TestCase(0, 50000, -1000)]
        [TestCase(0, -50000, 1000)]
        [TestCase(0, -3000, -1000)]
        // Zero
        [TestCase(0, 0, 0)]
        [TestCase(0, 1, 0)]
        [TestCase(0, 3000000, 0)]
        [TestCase(0, -50000000, 0)]
        [TestCase(1, 1, 0)]
        [TestCase(0, 0, 10)]
        [TestCase(0, 0, -10)]
        // One
        [TestCase(0, 3000000, 1)]
        [TestCase(0, 0, 1)]
        [TestCase(0, -5000000, 1)]
        // More interesting cases - explore the boundaries of the fast path.
        // This currently assumes that we're optimizing on multiplying "less than 100 days"
        // by less than "about a thousand". There's a comment in the code near that constant
        // to indicate that these tests would need to change if that constant changes.
        [TestCase(-99, 10000, 800)]
        [TestCase(-101, 10000, 800)]
        [TestCase(-99, 10000, 1234)]
        [TestCase(-101, 10000, 1234)]
        [TestCase(-99, 10000, -800)]
        [TestCase(-101, 10000, -800)]
        [TestCase(-99, 10000, -1234)]
        [TestCase(-101, 10000, -1234)]
        [TestCase(99, 10000, 800)]
        [TestCase(101, 10000, 800)]
        [TestCase(99, 10000, 1234)]
        [TestCase(101, 10000, 1234)]
        [TestCase(99, 10000, -800)]
        [TestCase(101, 10000, -800)]
        [TestCase(99, 10000, -1234)]
        [TestCase(101, 10000, -1234)]
        public void OperatorMultiplication(int days, long nanos, long rightOperand)
        {
            // Rather than expressing an expected answer, just do a "long-hand" version
            // using ToBigIntegerNanoseconds and FromNanoseconds, trusting those two operations
            // to be correct.
            var duration = Duration.FromDays(days) + Duration.FromNanoseconds(nanos);
            var actual = duration * rightOperand;

            var expected = Duration.FromNanoseconds(duration.ToBigIntegerNanoseconds() * rightOperand);
            Assert.AreEqual(expected, actual);
        }
        
        [Test]
        public void Commutation()
        {
            Assert.AreEqual(threeMillion * 5, 5 * threeMillion);
        }

        [Test]
        public void OperatorMultiplication_MethodEquivalents()
        {
            Assert.AreEqual(Duration.FromNanoseconds(-50000) * 1000, Duration.Multiply(Duration.FromNanoseconds(-50000), 1000));
            Assert.AreEqual(1000 * Duration.FromNanoseconds(-50000), Duration.Multiply(1000, Duration.FromNanoseconds(-50000)));
        }
        #endregion

        #region Unary operator -
        public static void UnaryMinus()
        {
            Assert.AreEqual(Duration.FromNanoseconds(-5000), -Duration.FromNanoseconds(5000));
        }

        public static void Negate()
        {
            Assert.AreEqual(Duration.FromNanoseconds(-5000), Duration.Negate(Duration.FromNanoseconds(5000)));
        }
        #endregion
    }
}
