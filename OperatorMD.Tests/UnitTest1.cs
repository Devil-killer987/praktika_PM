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

// Пространства имён основного проекта (если модели уже есть)
// using OperatorMD.Models;

namespace OperatorMD.Tests
{
    [TestFixture]
    public class UnitTest1
    {
        #region Модели для тестов (если в основном проекте ещё нет)
        // Если модели уже определены в OperatorMD.Models, удалите этот блок
        public class ActiveBatch
        {
            public int id { get; set; }
            public string batch_number { get; set; }
            public string product_name { get; set; }
            public string line { get; set; }
            public string status { get; set; }
            public string current_step { get; set; }
            public int current_step_progress { get; set; }
            public int total_steps { get; set; }
            public bool has_deviation { get; set; }
            public DateTime? start_time { get; set; }
        }

        public class BatchStep
        {
            public int id { get; set; }
            public int step_order { get; set; }
            public string step_name { get; set; }
            public string status { get; set; } // pending, in_progress, completed
            public decimal? planned_temp_c { get; set; }
            public decimal? planned_pressure_bar { get; set; }
            public int? planned_duration_min { get; set; }
            public decimal? actual_temp_c { get; set; }
            public decimal? actual_pressure_bar { get; set; }
            public int? actual_duration_min { get; set; }
            public bool deviation_flag { get; set; }
            public string operator_comment { get; set; }
            public decimal? temp_tolerance_max { get; set; }
            public decimal? pressure_tolerance_max { get; set; }
        }

        public class TelemetryData
        {
            public decimal current_temperature { get; set; }
            public decimal current_pressure { get; set; }
            public int current_rpm { get; set; }
            public string equipment_status { get; set; }
            public string last_update { get; set; }
        }
        #endregion

        #region Вспомогательные методы для проверки допусков

        private bool IsWithinTolerance(decimal actual, decimal planned, decimal? tolerance)
        {
            if (!tolerance.HasValue) return true;
            return Math.Abs(actual - planned) <= tolerance.Value;
        }

        private bool IsParameterOutOfTolerance(decimal actual, decimal planned, decimal? tolerance)
        {
            return !IsWithinTolerance(actual, planned, tolerance);
        }

        #endregion

        #region Тесты для проверки параметров шага (допуски)

        [Test]
        public void Temperature_WithinTolerance_ReturnsTrue()
        {
            decimal actual = 79.5m;
            decimal planned = 80m;
            decimal tolerance = 2m;
            bool result = IsWithinTolerance(actual, planned, tolerance);
            Assert.IsTrue(result);
        }

        [Test]
        public void Temperature_OutsideTolerance_ReturnsFalse()
        {
            decimal actual = 83.5m;
            decimal planned = 80m;
            decimal tolerance = 2m;
            bool result = IsWithinTolerance(actual, planned, tolerance);
            Assert.IsFalse(result);
        }

        [Test]
        public void Pressure_WithinTolerance_ReturnsTrue()
        {
            decimal actual = 3.1m;
            decimal planned = 3.0m;
            decimal tolerance = 0.3m;
            bool result = IsWithinTolerance(actual, planned, tolerance);
            Assert.IsTrue(result);
        }

        [Test]
        public void Pressure_OutsideTolerance_ReturnsFalse()
        {
            decimal actual = 3.5m;
            decimal planned = 3.0m;
            decimal tolerance = 0.3m;
            bool result = IsWithinTolerance(actual, planned, tolerance);
            Assert.IsFalse(result);
        }

        [Test]
        public void Deviation_DetectedWhenAnyParameterOutOfTolerance()
        {
            var step = new BatchStep
            {
                planned_temp_c = 80m,
                temp_tolerance_max = 2m,
                actual_temp_c = 83.5m,
                planned_pressure_bar = 3.0m,
                pressure_tolerance_max = 0.3m,
                actual_pressure_bar = 3.1m
            };
            bool tempDeviation = IsParameterOutOfTolerance(step.actual_temp_c.Value, step.planned_temp_c.Value, step.temp_tolerance_max);
            bool pressureDeviation = IsParameterOutOfTolerance(step.actual_pressure_bar.Value, step.planned_pressure_bar.Value, step.pressure_tolerance_max);
            bool hasDeviation = tempDeviation || pressureDeviation;
            Assert.IsTrue(hasDeviation);
        }

        #endregion

        #region Тесты для обязательности комментария при отклонении

        [Test]
        public void DeviationRecord_WithoutComment_ThrowsValidationError()
        {
            bool hasDeviation = true;
            string comment = "";
            bool isValid = !(hasDeviation && string.IsNullOrWhiteSpace(comment));
            Assert.IsFalse(isValid);
        }

        [Test]
        public void DeviationRecord_WithComment_IsValid()
        {
            bool hasDeviation = true;
            string comment = "Давление вышло за пределы";
            bool isValid = !(hasDeviation && string.IsNullOrWhiteSpace(comment));
            Assert.IsTrue(isValid);
        }

        [Test]
        public void NoDeviation_CommentNotRequired_IsValid()
        {
            bool hasDeviation = false;
            string comment = "";
            bool isValid = !(hasDeviation && string.IsNullOrWhiteSpace(comment));
            Assert.IsTrue(isValid);
        }

        #endregion

        #region Тесты для сериализации моделей

        [Test]
        public void ActiveBatch_SerializationRoundtrip_PreservesData()
        {
            var original = new ActiveBatch
            {
                id = 1,
                batch_number = "B-001",
                product_name = "Гербицид",
                line = "Линия №1",
                status = "running",
                current_step = "Экструзия",
                current_step_progress = 2,
                total_steps = 4,
                has_deviation = false,
                start_time = new DateTime(2025, 5, 15, 8, 0, 0)
            };
            var json = JsonConvert.SerializeObject(original);
            var restored = JsonConvert.DeserializeObject<ActiveBatch>(json);
            Assert.AreEqual(original.id, restored.id);
            Assert.AreEqual(original.batch_number, restored.batch_number);
            Assert.AreEqual(original.current_step, restored.current_step);
            Assert.AreEqual(original.has_deviation, restored.has_deviation);
        }

        [Test]
        public void BatchStep_SerializationRoundtrip_PreservesData()
        {
            var original = new BatchStep
            {
                id = 10,
                step_order = 2,
                step_name = "Смешивание",
                status = "in_progress",
                planned_temp_c = 45m,
                actual_temp_c = 44.8m,
                deviation_flag = false,
                operator_comment = "OK"
            };
            var json = JsonConvert.SerializeObject(original);
            var restored = JsonConvert.DeserializeObject<BatchStep>(json);
            Assert.AreEqual(original.id, restored.id);
            Assert.AreEqual(original.step_name, restored.step_name);
            Assert.AreEqual(original.status, restored.status);
            Assert.AreEqual(original.actual_temp_c, restored.actual_temp_c);
        }

        [Test]
        public void TelemetryData_SerializationRoundtrip_PreservesData()
        {
            var original = new TelemetryData
            {
                current_temperature = 78.3m,
                current_pressure = 2.9m,
                current_rpm = 1780,
                equipment_status = "Работает",
                last_update = "14:30:25"
            };
            var json = JsonConvert.SerializeObject(original);
            var restored = JsonConvert.DeserializeObject<TelemetryData>(json);
            Assert.AreEqual(original.current_temperature, restored.current_temperature);
            Assert.AreEqual(original.current_pressure, restored.current_pressure);
            Assert.AreEqual(original.current_rpm, restored.current_rpm);
            Assert.AreEqual(original.equipment_status, restored.equipment_status);
        }

        #endregion

        #region Тесты для фильтрации и сортировки списков

        [Test]
        public void FilterRunningBatches_ReturnsOnlyRunning()
        {
            var batches = new List<ActiveBatch>
            {
                new ActiveBatch { status = "running" },
                new ActiveBatch { status = "paused" },
                new ActiveBatch { status = "running" },
                new ActiveBatch { status = "completed" }
            };
            var running = batches.Where(b => b.status == "running").ToList();
            Assert.AreEqual(2, running.Count);
        }

        [Test]
        public void SortStepsByOrder_ReturnsAscending()
        {
            var steps = new List<BatchStep>
            {
                new BatchStep { step_order = 3, step_name = "Шаг 3" },
                new BatchStep { step_order = 1, step_name = "Шаг 1" },
                new BatchStep { step_order = 2, step_name = "Шаг 2" }
            };
            var sorted = steps.OrderBy(s => s.step_order).ToList();
            Assert.AreEqual(1, sorted[0].step_order);
            Assert.AreEqual(2, sorted[1].step_order);
            Assert.AreEqual(3, sorted[2].step_order);
        }

        [Test]
        public void DetectBatchesWithDeviation_ReturnsCorrectFlag()
        {
            var batches = new List<ActiveBatch>
            {
                new ActiveBatch { has_deviation = true },
                new ActiveBatch { has_deviation = false },
                new ActiveBatch { has_deviation = true }
            };
            var withDeviation = batches.Count(b => b.has_deviation);
            Assert.AreEqual(2, withDeviation);
        }

        #endregion

        #region Тесты для HTTP-клиента и мокирования ApiClient

        public interface IOperatorApiClient
        {
            Task<List<ActiveBatch>> GetActiveBatchesAsync();
            Task<BatchStep> StartStepAsync(int stepId);
            Task<BatchStep> CompleteStepAsync(int stepId, CompleteStepRequest request);
            Task<TelemetryData> GetTelemetryAsync();
            Task<object> ReportProblemAsync(ReportProblemRequest request);
        }

        public class CompleteStepRequest
        {
            public decimal? ActualTempC { get; set; }
            public int? ActualDurationMin { get; set; }
            public decimal? ActualPressureBar { get; set; }
            public string OperatorComment { get; set; }
            public string Severity { get; set; }
        }

        public class ReportProblemRequest
        {
            public int batch_id { get; set; }
            public string deviation_type { get; set; }
            public string description { get; set; }
            public string severity { get; set; }
        }

        [Test]
        public async Task MockedApiClient_GetActiveBatches_ReturnsMockData()
        {
            var mock = new Mock<IOperatorApiClient>();
            var expected = new List<ActiveBatch>
            {
                new ActiveBatch { id = 1, batch_number = "B-001", status = "running" }
            };
            mock.Setup(x => x.GetActiveBatchesAsync()).ReturnsAsync(expected);

            var result = await mock.Object.GetActiveBatchesAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("B-001", result[0].batch_number);
        }

        [Test]
        public async Task MockedApiClient_StartStep_ReturnsUpdatedStep()
        {
            var mock = new Mock<IOperatorApiClient>();
            var expectedStep = new BatchStep { id = 5, status = "in_progress" };
            mock.Setup(x => x.StartStepAsync(5)).ReturnsAsync(expectedStep);

            var result = await mock.Object.StartStepAsync(5);
            Assert.AreEqual("in_progress", result.status);
        }

        [Test]
        public async Task MockedApiClient_CompleteStep_ReturnsCompletedStep()
        {
            var mock = new Mock<IOperatorApiClient>();
            var request = new CompleteStepRequest { ActualTempC = 79.5m, OperatorComment = "OK" };
            var expected = new BatchStep { id = 5, status = "completed", deviation_flag = false };
            mock.Setup(x => x.CompleteStepAsync(5, request)).ReturnsAsync(expected);

            var result = await mock.Object.CompleteStepAsync(5, request);
            Assert.AreEqual("completed", result.status);
            Assert.IsFalse(result.deviation_flag);
        }

        [Test]
        public async Task MockedHttpHandler_SimulateTelemetryResponse_Success()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            var telemetry = new TelemetryData
            {
                current_temperature = 78.5m,
                current_pressure = 3.0m,
                current_rpm = 1800,
                equipment_status = "Работает"
            };
            var responseJson = JsonConvert.SerializeObject(telemetry);
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
                });

            var client = new HttpClient(mockHandler.Object);
            var response = await client.GetAsync("http://fake/api/telemetry");
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<TelemetryData>(json);
            Assert.IsTrue(response.IsSuccessStatusCode);
            Assert.AreEqual(78.5m, data.current_temperature);
            Assert.AreEqual(1800, data.current_rpm);
        }

        #endregion

        #region Тесты для HTTP-статусов

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
    }
}