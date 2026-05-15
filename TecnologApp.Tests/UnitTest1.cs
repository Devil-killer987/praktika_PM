using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using TecnologApp.Models;

namespace TecnologApp.Tests
{
    [TestFixture]
    public class UnitTest1
    {
        #region Тесты для моделей (проверка свойств)

        [Test]
        public void Product_Properties_SetAndGetCorrectly()
        {
            var product = new Product
            {
                id = 1,
                code = "P001",
                name = "Гербицид",
                product_type = "herbicide",
                release_form = "liquid",
                status = "active"
            };
            Assert.AreEqual(1, product.id);
            Assert.AreEqual("P001", product.code);
            Assert.AreEqual("Гербицид", product.name);
            Assert.AreEqual("herbicide", product.product_type);
            Assert.AreEqual("liquid", product.release_form);
            Assert.AreEqual("active", product.status);
        }

        [Test]
        public void Batch_Properties_SetAndGetCorrectly()
        {
            var batch = new Batch
            {
                id = 10,
                batch_number = "B-123",
                product_name = "Инсектицид",
                status = "running",
                start_time = new DateTime(2025, 5, 15, 8, 0, 0),
                actual_quantity_kg = 1500.75m,
                deviation_count = 2
            };
            Assert.AreEqual(10, batch.id);
            Assert.AreEqual("B-123", batch.batch_number);
            Assert.AreEqual("Инсектицид", batch.product_name);
            Assert.AreEqual("running", batch.status);
            Assert.AreEqual(new DateTime(2025, 5, 15, 8, 0, 0), batch.start_time);
            Assert.AreEqual(1500.75m, batch.actual_quantity_kg);
            Assert.AreEqual(2, batch.deviation_count);
        }

        [Test]
        public void RecipeComponent_Properties_SetAndGetCorrectly()
        {
            var comp = new RecipeComponent
            {
                id = 5,
                material_id = 3,
                material_name = "Активное вещество",
                percentage = 45.5m,
                load_order = 1
            };
            Assert.AreEqual(5, comp.id);
            Assert.AreEqual(3, comp.material_id);
            Assert.AreEqual("Активное вещество", comp.material_name);
            Assert.AreEqual(45.5m, comp.percentage);
            Assert.AreEqual(1, comp.load_order);
        }

        #endregion

        #region Тесты для сериализации/десериализации JSON

        [Test]
        public void Product_SerializationRoundtrip_PreservesData()
        {
            var original = new Product
            {
                id = 99,
                code = "TEST",
                name = "Тестовый продукт",
                product_type = "fungicide",
                release_form = "powder",
                status = "draft"
            };
            var json = JsonConvert.SerializeObject(original);
            var restored = JsonConvert.DeserializeObject<Product>(json);
            Assert.AreEqual(original.id, restored.id);
            Assert.AreEqual(original.code, restored.code);
            Assert.AreEqual(original.name, restored.name);
            Assert.AreEqual(original.product_type, restored.product_type);
            Assert.AreEqual(original.release_form, restored.release_form);
            Assert.AreEqual(original.status, restored.status);
        }

        [Test]
        public void Batch_SerializationRoundtrip_PreservesData()
        {
            var original = new Batch
            {
                id = 7,
                batch_number = "B-777",
                product_name = "Продукт X",
                status = "completed",
                start_time = new DateTime(2025, 5, 10),
                actual_quantity_kg = 2000m,
                deviation_count = 0
            };
            var json = JsonConvert.SerializeObject(original);
            var restored = JsonConvert.DeserializeObject<Batch>(json);
            Assert.AreEqual(original.id, restored.id);
            Assert.AreEqual(original.batch_number, restored.batch_number);
            Assert.AreEqual(original.product_name, restored.product_name);
            Assert.AreEqual(original.status, restored.status);
            Assert.AreEqual(original.start_time, restored.start_time);
            Assert.AreEqual(original.actual_quantity_kg, restored.actual_quantity_kg);
            Assert.AreEqual(original.deviation_count, restored.deviation_count);
        }

        [Test]
        public void RecipeComponent_SerializationRoundtrip_PreservesData()
        {
            var original = new RecipeComponent
            {
                id = 12,
                material_id = 8,
                material_name = "Растворитель",
                percentage = 30.25m,
                load_order = 2
            };
            var json = JsonConvert.SerializeObject(original);
            var restored = JsonConvert.DeserializeObject<RecipeComponent>(json);
            Assert.AreEqual(original.id, restored.id);
            Assert.AreEqual(original.material_id, restored.material_id);
            Assert.AreEqual(original.material_name, restored.material_name);
            Assert.AreEqual(original.percentage, restored.percentage);
            Assert.AreEqual(original.load_order, restored.load_order);
        }

        #endregion

        #region Тесты для LINQ и бизнес-логики (без внешних помощников)

        [Test]
        public void FilterActiveProducts_ReturnsOnlyActive()
        {
            var products = new List<Product>
            {
                new Product { status = "active" },
                new Product { status = "draft" },
                new Product { status = "active" },
                new Product { status = "archived" }
            };
            var active = products.Where(p => p.status == "active").ToList();
            Assert.AreEqual(2, active.Count);
        }

        [Test]
        public void OrderBatchesByStartTime_ReturnsChronological()
        {
            var batches = new List<Batch>
            {
                new Batch { start_time = new DateTime(2025, 5, 20) },
                new Batch { start_time = new DateTime(2025, 5, 10) },
                new Batch { start_time = new DateTime(2025, 5, 15) }
            };
            var ordered = batches.OrderBy(b => b.start_time).ToList();
            Assert.AreEqual(new DateTime(2025, 5, 10), ordered[0].start_time);
            Assert.AreEqual(new DateTime(2025, 5, 15), ordered[1].start_time);
            Assert.AreEqual(new DateTime(2025, 5, 20), ordered[2].start_time);
        }

        [Test]
        public void SumOfComponentPercentages_CalculatedCorrectly()
        {
            var components = new List<RecipeComponent>
            {
                new RecipeComponent { percentage = 40m },
                new RecipeComponent { percentage = 35.5m },
                new RecipeComponent { percentage = 24.5m }
            };
            var sum = components.Sum(c => c.percentage);
            Assert.AreEqual(100m, sum);
        }

        [Test]
        public void Batch_CanBeStarted_WhenStatusIsPlanned()
        {
            var batch = new Batch { status = "planned" };
            Assert.IsTrue(batch.status == "planned");
        }

        [Test]
        public void Batch_CannotBeStarted_WhenStatusIsRunningOrCompleted()
        {
            var batchRunning = new Batch { status = "running" };
            var batchCompleted = new Batch { status = "completed" };
            Assert.IsFalse(batchRunning.status == "planned");
            Assert.IsFalse(batchCompleted.status == "planned");
        }

        #endregion

        #region Тесты для HTTP-статусов и обработки ошибок

        [Test]
        public void UnauthorizedStatusCode_IndicatesAuthFailure()
        {
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Test]
        public void NotFoundStatusCode_IndicatesMissingResource()
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public void InternalServerError_IndicatesServerProblem()
        {
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        #endregion

        #region Демонстрационные тесты для мокирования ApiClient (через интерфейс)

        // Пример интерфейса, который можно ввести для тестирования
        public interface IApiClient
        {
            Task<T> GetAsync<T>(string endpoint);
            Task<T> PostAsync<T>(string endpoint, object data);
        }

        [Test]
        public async Task MockedApiClient_GetAsync_ReturnsMockData()
        {
            var mock = new Mock<IApiClient>();
            var expected = new List<Product> { new Product { id = 1, name = "MockProduct" } };
            mock.Setup(x => x.GetAsync<List<Product>>("products")).ReturnsAsync(expected);

            var result = await mock.Object.GetAsync<List<Product>>("products");
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("MockProduct", result[0].name);
        }

        [Test]
        public async Task MockedApiClient_PostAsync_ReturnsCreatedObject()
        {
            var mock = new Mock<IApiClient>();
            var created = new Product { id = 42, name = "NewProduct" };
            mock.Setup(x => x.PostAsync<Product>("products", It.IsAny<object>())).ReturnsAsync(created);

            var result = await mock.Object.PostAsync<Product>("products", new { name = "NewProduct" });
            Assert.AreEqual(42, result.id);
            Assert.AreEqual("NewProduct", result.name);
        }

        // Пример мока HttpMessageHandler (без изменения основного кода)
        [Test]
        public async Task MockedHttpHandler_ReturnsSuccessResponse()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            var expectedJson = "{\"id\":1,\"name\":\"Test\"}";
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedJson, Encoding.UTF8, "application/json")
                });

            var client = new HttpClient(mockHandler.Object);
            var response = await client.GetAsync("http://fake/api/test");
            var content = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(expectedJson, content);
        }

        #endregion
    }
}