﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SportStore.Domain.Entities;
using System.Linq;
using Moq;
using SportStore.Domain.Abstract;
using SportStore.WebUI.Controllers;
using System.Web.Mvc;
using SportStore.WebUI.Models;


namespace SportStore.UnitTest
{
    [TestClass]
    public class CartTest
    {
        [TestMethod]
        public void Can_Add_To_Cart()
        {
            // Arrange - create the mock repository
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product {ProductID = 1, Name = "P1", Category = "Apples"},
            }.AsQueryable());
            // Arrange - create a Cart
            Cart cart = new Cart();
            // Arrange - create the controller
            CartController target = new CartController(mock.Object, null);
            // Act - add a product to the cart
            target.AddToCart(cart, 1, null);
            // Assert
            Assert.AreEqual(cart.Lines.Count(), 1);
            Assert.AreEqual(cart.Lines.ToArray()[0].Product.ProductID, 1);
        }

        [TestMethod]
        public void Adding_Product_To_Cart_Goes_To_Cart_Screen()
        {
            // Arrange - create the mock repository
            Mock<IProductRepository> mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[] {
                new Product {ProductID = 1, Name = "P1", Category = "Apples"},
            }.AsQueryable());
            // Arrange - create a Cart
            Cart cart = new Cart();
            // Arrange - create the controller
            CartController target = new CartController(mock.Object, null);
            // Act - add a product to the cart
            RedirectToRouteResult result = target.AddToCart(cart, 2, "myUrl");
            // Assert
            Assert.AreEqual(result.RouteValues["action"], "Index");
            Assert.AreEqual(result.RouteValues["returnUrl"], "myUrl");
        }

        [TestMethod]
        public void Can_View_Cart_Contents()
        {
            // Arrange - create a Cart
            Cart cart = new Cart();
            // Arrange - create the controller
            CartController target = new CartController(null, null);
            // Act - call the Index action method
            CartIndexViewModel result
                = (CartIndexViewModel)target.Index(cart, "myUrl").ViewData.Model;

            // Assert
            Assert.AreSame(result.Cart, cart);
            Assert.AreEqual(result.ReturnUrl, "myUrl");
        }

        [TestMethod]
        public void Cannot_Checkout_Empty_Cart()
        {
            // Arrange - create a mock order processor
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();
            // Arrange - create an empty cart
            Cart cart = new Cart();
            // Arrange - create shipping details
            ShippingDetails shippingDetails = new ShippingDetails();
            // Arrange - create an instance of the controller
            CartController target = new CartController(null, mock.Object);

            // Act
            ViewResult result = target.Checkout(cart, shippingDetails);

            // Assert - check that the order hasn't been passed on to the processor
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()),
                Times.Never());
            // Assert - check that the method is returning the default view
            Assert.AreEqual("", result.ViewName);
            // Assert - check that I am passing an invalid model to the view
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);

        }
        [TestMethod]
        public void Cannot_Checkout_Invalid_ShippingDetails()
        {
            // Arrenge - create a mock order processor
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();

            // Arrange - create a cart with an item
            Cart cart = new Cart();
            cart.AddItem(new Product(), 1);

            // Arrenge - create an instance of controller
            CartController target = new CartController(null, mock.Object);
            // Arrenge - add an error to the model
            target.ModelState.AddModelError("error", "error");

            // Act - try to checkout
            ViewResult result = target.Checkout(cart, new ShippingDetails());

            // Assert - check that the order hasn't been passed on to the processor
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()),
                Times.Never);

            // Assert - check that the method is returning the default view
            Assert.AreEqual("", result.ViewName);
            //Assert - check that I am passing an invalid model to the view
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void  Can_Checkout_And_Submit_Order()
        {
            //Arrenge - create a mock order processor
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();
            //Arrenge - create a cart with an item
            Cart cart = new Cart();
            cart.AddItem(new Product(), 1);
            //Arrenge - create an instance of controller
            CartController target = new CartController(null, mock.Object);

            // Act - try to checkout
            ViewResult result = target.Checkout(cart, new ShippingDetails());

            // Assert - check that order has been passed on to the processor
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()),
                Times.Once);
            //Assert - check that method is returning the Complete view
            Assert.AreEqual("Completed", result.ViewName);
            // Assert - check that I am passing a valid model to the view
            Assert.AreEqual(true, result.ViewData.ModelState.IsValid);

        }
    }
}