using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Dynamic;
using System.ComponentModel;

namespace MongoDyn.tests
{
    [TestClass]
    public class DynamicTest
    {
        
        private static  DynamicCollection<int, ICustomer> Customers ;
        private const long Records = 10;

        [ClassInitialize]
        public static void Initialize(TestContext ctx)
        {
           // Dynamic.Audit = true;
            Dynamic.NotifyPropertyChanged = true;
            Customers = Dynamic.GetCollection<int, ICustomer>();
            Customers.DeleteAll(true);
            Assert.IsTrue(Customers.Count == 0);

           
        }

        [TestMethod]
        public void TestClassInitializer()
        {
            var customer = Customers.New();

            ((INotifyPropertyChanged)customer).PropertyChanged += DynamicTest_PropertyChanged;
            customer.Name = "toto";
            Customers.Upsert(customer);
            Assert.IsNotNull(customer);
        }

        void DynamicTest_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
           
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
            for (int i = 0; i < Records; i++)
            {
                var customer = Customers.GetByKey(i+1);
                customer.Name = string.Format("Customer changed- {0} ", i);
                Customers.Upsert(customer);
            }
            Assert.IsTrue(Customers.Count == Records);
        }

        [TestMethod]
        public void DynUpdateByKey()
        {
            for (int i = 0; i < Records; i++)
            {
                dynamic customer = new ExpandoObject();
                customer.Id = i+1;
                customer.Name = string.Format("Customer Expendo- {0} ", i);
                Customers.UpsertImpromptu(customer);
            }
            Assert.IsTrue(Customers.Count == Records);
        }
    }
}
