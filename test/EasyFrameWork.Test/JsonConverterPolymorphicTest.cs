/* http://www.zkea.net/ 
 * Copyright 2026 ZKEASOFT 
 * http://www.zkea.net/licenses */

using Easy;
using Easy.Serializer;

namespace EasyFrameWork.Test
{
    [TestClass]
    public class JsonConverterPolymorphicTest
    {
        class JsonIgnoreTestObject
        {
            public string Name { get; set; }

            [Newtonsoft.Json.JsonIgnore]
            public int Age { get; set; }
        }

        [TestMethod]
        public void TestSerializeDeserializePolymorphicIgnoresJsonIgnoreAttribute()
        {
            var obj = new JsonIgnoreTestObject
            {
                Name = "Wayne",
                Age = 99
            };

            var json = JsonConverter.SerializePolymorphic(obj);
            Assert.Contains("\"Age\":99", json);

            var deserialized = JsonConverter.DeserializePolymorphic<JsonIgnoreTestObject>(json);
            Assert.AreEqual(99, deserialized.Age);
            Assert.AreEqual("Wayne", deserialized.Name);
        }

        class Animal
        {
            public string Name { get; set; }
        }

        class Dog : Animal
        {
            public string Breed { get; set; }
        }

        class Cat : Animal
        {
            public int Lives { get; set; }
        }

        [TestMethod]
        public void TestSerializeDeserializePolymorphicWithDerivedType()
        {
            Animal dog = new Dog { Name = "Rex", Breed = "German Shepherd" };

            var json = JsonConverter.SerializePolymorphic(dog);
            Assert.Contains("\"$type\"", json);
            Assert.Contains("\"Breed\":\"German Shepherd\"", json);

            var deserialized = JsonConverter.DeserializePolymorphic<Animal>(json);
            Assert.IsInstanceOfType(deserialized, typeof(Dog));
            Assert.AreEqual("Rex", deserialized.Name);
            Assert.AreEqual("German Shepherd", ((Dog)deserialized).Breed);
        }

        [TestMethod]
        public void TestSerializeDeserializePolymorphicWithDerivedList()
        {
            var animals = new System.Collections.Generic.List<Animal>
            {
                new Dog { Name = "Rex", Breed = "German Shepherd" },
                new Cat { Name = "Mittens", Lives = 9 }
            };

            var json = JsonConverter.SerializePolymorphic(animals);
            Assert.Contains("\"$type\"", json);
            Assert.Contains("German Shepherd", json);
            Assert.Contains("Mittens", json);

            var deserializedList = JsonConverter.DeserializePolymorphic<System.Collections.Generic.List<Animal>>(json);
            Assert.HasCount(2, deserializedList);
            Assert.IsInstanceOfType(deserializedList[0], typeof(Dog));
            Assert.IsInstanceOfType(deserializedList[1], typeof(Cat));
            Assert.AreEqual("Rex", deserializedList[0].Name);
            Assert.AreEqual("Mittens", deserializedList[1].Name);
            Assert.AreEqual("German Shepherd", ((Dog)deserializedList[0]).Breed);
            Assert.AreEqual(9, ((Cat)deserializedList[1]).Lives);
        }
    }
}
