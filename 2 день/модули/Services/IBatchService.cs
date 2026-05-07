using api_work2.DTOs;
using api_work2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace api_work2.Services
{
    public interface IBatchService
    {
        Task<List<batches>> GetActiveBatches();
        Task<batches> GetBatchById(int id);
        Task<batch_steps> StartStep(int batchId, int stepId);
        Task<batch_steps> CompleteStep(int stepId, StepCompleteDto data);
        Task<List<batch_steps>> GetBatchSteps(int batchId);
    }
}