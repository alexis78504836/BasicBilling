using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace BasicBilling.Test
{
    [TestFixture]
    public class Tests
    {
        private NBills nbills;
        
        [SetUp]
        public void Setup()
        {
            List<Client> clients = new List<Client>();
            clients.Add(new Client { clientId = "100", Name = "Pedro" });
            clients.Add(new Client { clientId = "200", Name = "Maria" });
            clients.Add(new Client { clientId = "300", Name = "Jose" });
            
            List<Bills> bills = new List<Bills>();
            bills.Add(new Bills { Category = "WATER", period = "202104", Monto = 150, Estado = "Pending", clientId = "100" });
            bills.Add(new Bills { Category = "ELECTRICITY", period = "202103", Monto = 250, Estado = "Pending", clientId = "200" });
            bills.Add(new Bills { Category = "WATER", period = "202104", Monto = 300, Estado = "Pending", clientId = "300" });
            bills.Add(new Bills { Category = "SEWER", period = "202103", Monto = 150, Estado = "Paid", clientId = "100" });
            bills.Add(new Bills { Category = "ELECTRICITY", period = "202104", Monto = 250, Estado = "Pending", clientId = "300" });
            bills.Add(new Bills { Category = "SEWER", period = "202103", Monto = 300, Estado = "Pending", clientId = "200" });

            var myDbMoq = new Mock<dbBasicBilling>();
            var clientsMock = new Mock<DbSet<Client>>();
            var billsMock = new Mock<DbSet<Bills>>();
            myDbMoq.Setup(p => p.Clients).Returns(DbContextMock.GetQueryableMockDbSet<Client>(clients));
            myDbMoq.Setup(p => p.Bills).Returns(DbContextMock.GetQueryableMockDbSet<Bills>(bills));


            nbills = new NBills(myDbMoq.Object);
        }

        [Test]
        public void GetPendingBills()
        {            
            var c = nbills.GetPendingBillsPerClient("100");
            Assert.AreEqual(1, c.Count());
        }
        [Test]
        public void GetAllHistoryPayments()
        {            
            var c = nbills.GetHistoryOfPayments();
            Assert.AreEqual(1, c.Count());
        }
        [Test]
        public void ValidatePaidBill()
        {
            Bills bills = new Bills();
            bills.clientId = "100";
            bills.Monto = 100;
            bills.period = "202103";
            bills.Category = "SEWER"; //SEWER
            string mensaje = string.Empty;

            var c = nbills.PayBill(bills, ref mensaje);
            Assert.IsFalse(c);
            Assert.AreEqual(mensaje, "The bill has already been paid.");
        }
        
        [Test]
        public void ValidatePayBill()
        {
            Bills bills = new Bills();
            bills.clientId = "200";
            bills.Monto = 300;
            bills.period = "202103";
            bills.Category = "SEWER";
            string mensaje = string.Empty;

            var c = nbills.PayBill(bills, ref mensaje);
            var payments = nbills.GetHistoryOfPaymentsPerClient("200");
            Assert.IsTrue(c);
            Assert.AreEqual(1, payments.Count());
        }
    }
}