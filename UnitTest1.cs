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
        private NClientes clientes;
        
        [SetUp]
        public void Setup()
        {
            List<Clients> clients = new List<Clients>();
            clients.Add(new Clients { ClientsId = "100", Name = "Pedro" });
            clients.Add(new Clients { ClientsId = "200", Name = "Maria" });
            clients.Add(new Clients { ClientsId = "300", Name = "Jose" });
            List<Services> services = new List<Services>();
            services.Add(new Services { ServicesId = 1, Name = "Basic"});
            services.Add(new Services { ServicesId = 2, Name = "Intermediate" });
            services.Add(new Services { ServicesId = 3, Name = "Advanced" });
            List<Bills> bills = new List<Bills>();
            bills.Add(new Bills { ServicesId = 1, Periodo = "202104", Monto = 150, Estado = "Pending", ClientsId = "100" });
            bills.Add(new Bills { ServicesId = 2, Periodo = "202103", Monto = 250, Estado = "Pending", ClientsId = "200" });
            bills.Add(new Bills { ServicesId = 3, Periodo = "202104", Monto = 300, Estado = "Pending", ClientsId = "300" });
            bills.Add(new Bills { ServicesId = 1, Periodo = "202103", Monto = 150, Estado = "Paid", ClientsId = "100" });
            bills.Add(new Bills { ServicesId = 2, Periodo = "202104", Monto = 250, Estado = "Pending", ClientsId = "300" });
            bills.Add(new Bills { ServicesId = 3, Periodo = "202103", Monto = 300, Estado = "Pending", ClientsId = "200" });

            var myDbMoq = new Mock<dbBasicBilling>();
            var clientsMock = new Mock<DbSet<Clients>>();
            var serviceMock = new Mock<DbSet<Services>>();
            var billsMock = new Mock<DbSet<Bills>>();
            myDbMoq.Setup(p => p.Clients).Returns(DbContextMock.GetQueryableMockDbSet<Clients>(clients));
            myDbMoq.Setup(p => p.Services).Returns(DbContextMock.GetQueryableMockDbSet<Services>(services));
            myDbMoq.Setup(p => p.Bills).Returns(DbContextMock.GetQueryableMockDbSet<Bills>(bills));


            clientes = new NClientes(myDbMoq.Object);
        }

        [Test]
        public void GetPendingBills()
        {            
            var c = clientes.GetPendingBillsPerClient("100");
            Assert.AreEqual(1, c.Count());
        }
        [Test]
        public void GetAllHistoryPayments()
        {            
            var c = clientes.GetHistoryOfPayments();
            Assert.AreEqual(1, c.Count());
        }
        [Test]
        public void ValidatePaidBill()
        {
            Bills bills = new Bills();
            bills.ClientsId = "100";
            bills.Monto = 100;
            bills.Periodo = "202103";
            bills.ServicesId = 1;
            string mensaje = string.Empty;

            var c = clientes.PayBill(bills, ref mensaje);
            Assert.IsFalse(c);
            Assert.AreEqual(mensaje, "The bill has already been paid.");
        }
        [Test]
        public void ValidateAmountToPayBill()
        {
            Bills bills = new Bills();
            bills.ClientsId = "100";
            bills.Monto = 100;
            bills.Periodo = "202104";
            bills.ServicesId = 1;
            string mensaje = string.Empty;

            var c = clientes.PayBill(bills, ref mensaje);
            Assert.IsFalse(c);
            Assert.IsTrue(mensaje.Contains("Please enter the amount of "));
        }
        [Test]
        public void ValidatePayBill()
        {
            Bills bills = new Bills();
            bills.ClientsId = "100";
            bills.Monto = 150;
            bills.Periodo = "202104";
            bills.ServicesId = 1;
            string mensaje = string.Empty;

            var c = clientes.PayBill(bills, ref mensaje);
            var payments = clientes.GetHistoryOfPaymentsPerClient("100");
            Assert.IsTrue(c);
            Assert.AreEqual(2, payments.Count());
        }
    }
}