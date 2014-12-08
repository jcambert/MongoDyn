using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Dynamic;

namespace MongoDyn.tests
{
    [TestClass]
    public class DynamicTest
    {
        private static readonly DynamicCollection<int, ICustomer> Customers = Dynamic.GetCollection<int, ICustomer>();
        private const long Records = 1000;

        [ClassInitialize]
        public static void Initialize(TestContext ctx)
        {
           
            Customers.DeleteAll(true);
            Assert.IsTrue(Customers.Count == 0);

           
        }


        [TestMethod]
        public void DynInserts()
        {
            for (int i = 0; i < Records; i++)
            {
                var customer = Customers.New();
                customer.Name = string.Format("Customer - {0} ",i);
                Customers.Upsert(customer);
            }
            Assert.IsTrue(Customers.Count == Records);
        }


        [TestMethod]
        public void DynFindAndUpdate()
        {
            for (int i = 1; i <= Records; i++)
            {
                var customer = Customers.GetByKey(i);
                customer.Name = string.Format("Customer - {0} ", i);
                Customers.Upsert(customer);
            }
            Assert.IsTrue(Customers.Count == Records);
        }

        [TestMethod]
        public void DynUpdateByKey()
        {
            for (int i = 1; i <= Records; i++)
            {
                dynamic customer = new ExpandoObject();
                customer.Id = i;
                customer.Name = string.Format("Customer - {0} ", i);
                Customers.UpsertImpromptu(customer);
            }
            Assert.IsTrue(Customers.Count == Records);
        }
    }
}
