using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReflectionExtensions.Tests
{
    [TestClass]
    public class TypeMixerTest
    {
        public class Foo
        {
            public string Name { get; set; }
        }
        public interface IClientId
        {
            int ClientId { get; set; }
        }
        public interface IBarId
        {
            int BarId { get; set; }
        }
        public interface ITypeTest
        {
            int MyInt { get; set; }
            float MyFloat { get; set; }
            string MyString { get; set; }
            byte[] MyByteArray { get; set; }
            DateTime MyDateTime { get; set; }
            int? MyNullableInt { get; set; }
            DateTimeOffset MyDateTimeOffset { get; set; }
            IEnumerable<int> MyIEnumerableInt { get; set; }
            List<int> MyListInt { get; set; }
            ICollection<int> MyCollectionInt { get; set; }
            List<IBarId> MyListBarId { get; set; }
            Dictionary<string, int> MyDictionaryStringInt { get; set; }
            Dictionary<string, IClientId> MyDictionaryStringClientId { get; set; }
        }
        [TestMethod]
        public void TypeMixerTest_ExtendWith_AddInterfaceToObjectWorks()
        {
            var obj = new object();
            var newobj = obj.ExtendWith<IClientId>();
            newobj.ClientId = 10;
            Assert.AreEqual(10, newobj.ClientId, "Expected the property to keep the value set");
        }
        [TestMethod]
        public void TypeMixerTest_ExtendWith_AddInterfaceToCustomObjectTwiceWorks()
        {
            var objA = new Foo { Name = "Bob" };
            var objB = new Foo { Name = "Fred" };
            var newobjA = objA.ExtendWith<IClientId>();
            newobjA.ClientId = 10;
            var newobjB = objB.ExtendWith<IClientId>();
            newobjB.ClientId = 11;
            Assert.AreEqual(10, newobjA.ClientId, "Expected the property to keep the value set");
            Assert.AreEqual("Bob", (newobjA as Foo)?.Name, "Expected the property to keep the value set");
            Assert.AreEqual(11, newobjB.ClientId, "Expected the property to keep the value set");
            Assert.AreEqual("Fred", (newobjB as Foo)?.Name, "Expected the property to keep the value set");
        }

        [TestCategory("Performance")]
        [TestCategory("Slow")]
        [TestMethod]
        public void TypeMixerTest_ExtendWith_PerfTest()
        {
            const int loopCount = 10000000;
            // add a stopwatch here
            var x = Enumerable.Range(1, loopCount)
                .AsParallel()
                .Select(i =>
                {
                    TypeMixerTest_ExtendWith_AddInterfaceToObjectWorks();
                    return i;
                })
                .Count();
            // add a non-parallel here, with stopwatch
            Assert.AreEqual(loopCount, x, $"Expected loop to run {loopCount} times");
            // add assert that parallel is better than non-parallel by expected margin
            // if not, we may have added non-thread friendly code somewhere.
        }

        [TestMethod]
        public void TypeMixerTest_ExtendWith_AddChainedInterfacesWorks()
        {
            var obj = new Foo { Name = "Bob" };
            var newobj = obj
                .ExtendWith<IClientId>()
                .ExtendWith<IBarId>();
            (newobj as IClientId).ClientId = 10;
            newobj.BarId = 5;
            Assert.AreEqual(10, (newobj as IClientId).ClientId, "Expected the property to keep the value set");
            Assert.AreEqual("Bob", (newobj as Foo)?.Name, "Expected the property to keep the value set");
            Assert.AreEqual(5, newobj.BarId, "Expected the property to keep the value set");
        }

        [TestMethod]
        public void TypeMixerTest_ExtendWith_ITypeTestWorks()
        {
            Assert.Fail("Implement this test");
        }
    }
}
