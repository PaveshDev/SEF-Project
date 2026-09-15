using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WasteToValue.Agents.Items.Models;
using WasteToValue.Api.Modules.Items.DTOs;
using WasteToValue.Api.Modules.Items.Entities;

namespace WasteToValue.Agents.Items.Tools
{
    public interface IInspectImagesTool
    {
        Task<List<EvidenceReference>> InspectImagesAsync(Guid assessmentId, List<ItemPhoto> photos);
    }

    public interface IGetCategoryChecklistTool
    {
        Task<List<string>> GetChecklistAsync(string category);
    }

    public interface IReadItemAnswersTool
    {
        Task<List<string>> ReadOwnerAnswersAsync(List<ItemConditionAnswer> answers);
    }

    public interface IRequestClarificationTool
    {
        Task RequestClarificationAsync(Guid itemId, Guid? assessmentId, string questionCode, string question, string reason);
    }

    public interface ISaveAssessmentDraftTool
    {
        Task SaveDraftAsync(AssessmentDraft draft);
    }
}
