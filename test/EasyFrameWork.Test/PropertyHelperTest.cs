/* http://www.zkea.net/ 
 * Copyright 2023 ZKEASOFT 
 * http://www.zkea.net/licenses */

using Easy;
using Easy.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace EasyFrameWork.Test
{
    [TestClass]
    public class PropertyHelperTest
    {
        class People
        {
            public int Age { get; set; }
            public string Name { get; set; }
            public List<string> Hobby { get; set; }
            public string[] FormerNames { get; set; }
            public Dictionary<string, string> Properties { get; set; }
            public People[] Children { get; set; }
        }
        private People CreateTestPeople()
        {
            return new People
            {
                Name = "Wayne",
                Age = 10,
                Hobby = new List<string> { "Basketball", "Football" },
                FormerNames = new string[] { "Ben" },
                Properties = new Dictionary<string, string> { { "City", "ShenZhen" }, { "Address", "Home" } },
                Children = new People[] { new People { Name = "Nancy", Hobby = new List<string> { "Run" } } }
            };
        }
        [TestMethod]
        public void TestGetPropertyValue()
        {
            var people = CreateTestPeople();
            Assert.AreEqual("Wayne", PropertyHelper.GetValue(people, "Name"));
            Assert.AreEqual(10, PropertyHelper.GetValue(people, "Age"));
            Assert.AreEqual(2, PropertyHelper.GetValue(people, "Hobby.Count"));
            Assert.AreEqual("Football", PropertyHelper.GetValue(people, "Hobby[1]"));
            Assert.AreEqual("ShenZhen", PropertyHelper.GetValue(people, "Properties[\"City\"]"));
            Assert.AreEqual("Run", PropertyHelper.GetValue(people, "Children[0].Hobby[0]"));

            Assert.AreEqual("A", PropertyHelper.GetValue(new string[] { "A" }, "[0]"));
            Assert.AreEqual("A", PropertyHelper.GetValue(new Dictionary<string,string> { { "A", "A" } }, "[\"A\"]"));
        }

        [TestMethod]
        public void TestSetPropertyValue()
        {
            var people = CreateTestPeople();

            PropertyHelper.SetValue(people, "Name", "Wayne Wei");
            Assert.AreEqual("Wayne Wei", people.Name);

            PropertyHelper.SetValue(people, "Hobby[0]", "Running");
            Assert.AreEqual("Running", people.Hobby[0]);

            PropertyHelper.SetValue(people, "FormerNames[0]", "OK");
            Assert.AreEqual("OK", people.FormerNames[0]);

            PropertyHelper.SetValue(people, "Properties[\"City\"]", "GuanZhou");
            Assert.AreEqual("GuanZhou", people.Properties["City"]);

            PropertyHelper.SetValue(people, "Children[0]", new People { Name = "New Nancy" });
            Assert.AreEqual("New Nancy", people.Children[0].Name);

            PropertyHelper.SetValue(people, "Children[0].Age", 123);
            Assert.AreEqual(123, people.Children[0].Age);
        }
    }
}
