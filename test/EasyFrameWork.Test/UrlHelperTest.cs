using Easy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyFrameWork.Test
{
    [TestClass]
    public class UrlHelperTest
    {
        [TestMethod]
        public void TestCombine()
        {
            Assert.AreEqual("~/home/index", Helper.Url.Combine("home", "index"));
        }
        [TestMethod]
        public void TestContainsScheme()
        {
            Assert.IsTrue(Helper.Url.ContainsScheme("http://www.zkea.net"));
            Assert.IsTrue(Helper.Url.ContainsScheme("https://www.zkea.net"));
            Assert.IsTrue(Helper.Url.ContainsScheme("ftp://www.zkea.net"));
            Assert.IsTrue(Helper.Url.ContainsScheme("file://www.zkea.net"));
        }
        [TestMethod]
        public void TestToVirtualPath()
        {
            Assert.AreEqual("~/home/index", Helper.Url.ToVirtualPath("home/index"));
            Assert.AreEqual("~/home/index", Helper.Url.ToVirtualPath("/home/index"));
            Assert.AreEqual("~/home/index", Helper.Url.ToVirtualPath("~/home/index"));
        }
        [TestMethod]
        public void TestToAbsolutePath()
        {
            Assert.AreEqual("/home/index", Helper.Url.ToAbsolutePath("home/index"));
            Assert.AreEqual("/home/index", Helper.Url.ToAbsolutePath("/home/index"));
            Assert.AreEqual("/home/index", Helper.Url.ToAbsolutePath("~/home/index"));
        }
    }
}
