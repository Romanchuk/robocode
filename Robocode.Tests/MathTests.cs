using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Romanchuk.Helpers;

namespace Robocode.Tests
{
    [TestClass]
    public class MathTests
    {
        [TestMethod]
        public void BearingDegrees()
        {
            //const double GunHeading = 360;

            const int x1 = 100;
            const int x2 = 100;
            const int y1 = 100;
            const int y2 = 900;

            var degrees = MathHelpers.AbsoluteBearingDegrees(x1, y1, x2, y2);

            Assert.AreEqual(degrees, 0);
        }

        [TestMethod]
        public void GunTurnAngle()
        {
            const double GunHeading = 19;
            const double EnemyAbsBearing = 358;

            var angleToTurn = MathHelpers.TurnRightOptimalAngle(GunHeading, EnemyAbsBearing);

            Assert.AreEqual(-21, angleToTurn);

            const double GunHeading2 = 32;
            const double EnemyAbsBearing2 = 3;

            var angleToTurn2 = MathHelpers.TurnRightOptimalAngle(GunHeading2, EnemyAbsBearing2);

            Assert.AreEqual(-29, angleToTurn2);
        }


    }
}
