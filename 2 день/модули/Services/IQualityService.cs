using api_work2.DTOs;
using api_work2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace api_work2.Services
{
    public interface IQualityService
    {
        Task<List<quality_tests>> GetPendingTests();
        Task<quality_tests> CreateTest(int batchId, string sampleType);
        Task<test_results> AddResult(TestResultInputDto result);
        Task<quality_tests> MakeDecision(DecisionDto decision);
    }
}