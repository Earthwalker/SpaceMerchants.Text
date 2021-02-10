//-----------------------------------------------------------------------
// <copyright file="ShipTests.cs" company="Leamware">
//     Copyright (c) Leamware. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SpaceMerchants.Server.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SpaceMerchants.Server;

    [TestClass()]
    public class ShipTests
    {
        [TestMethod()]
        public void ShipTest()
        {
            // empty name
            new Ship(string.Empty, ShipClass.LIGHT);

            string name = "name";
            Assert.AreEqual(new Ship(name, ShipClass.LIGHT).Name, name, "Name was not set.");
        }

        [TestMethod()]
        public void AddCargoTest()
        {
            // create test item
            var item = new Item("item", ItemType.Other, Game.MaxWeightMultiplier);

            // create small ship
            var smallShip = new Ship("SmallShip", ShipClass.LIGHT);
            Assert.IsTrue(smallShip.AddCargo(item, 1), "Small ship correct weight");
            Assert.IsFalse(smallShip.AddCargo(item, 2), "Small ship incorrect weight");
        }
    }
}