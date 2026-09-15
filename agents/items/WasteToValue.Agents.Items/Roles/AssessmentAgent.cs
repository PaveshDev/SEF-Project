using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WasteToValue.Agents.Items.Models;
using WasteToValue.Agents.Items.Tools;
using WasteToValue.Api.Modules.Items.Entities;

namespace WasteToValue.Agents.Items.Roles
{
    public class AssessmentAgent
    {
        private readonly IGetCategoryChecklistTool _checklistTool;
        private readonly IReadItemAnswersTool _answersTool;

        public AssessmentAgent(IGetCategoryChecklistTool checklistTool, IReadItemAnswersTool answersTool)
        {
            _checklistTool = checklistTool;
            _answersTool = answersTool;
        }

        public async Task<AssessmentDraft> BuildAssessmentDraftAsync(
            AssessmentWorkflowState state, 
            Item item, 
            List<EvidenceReference> evidences)
        {
            var checklist = await _checklistTool.GetChecklistAsync(item.Category ?? "Unknown");
            var ownerAnswers = await _answersTool.ReadOwnerAnswersAsync(item.ConditionAnswers?.ToList() ?? new List<ItemConditionAnswer>());

            // In a real LLM scenario, the agent would evaluate condition, missing info, etc.
            // For deterministic testing, we use basic heuristics to satisfy the requirements safely.
            var draft = new AssessmentDraft
            {
                ItemId = item.Id,
                AssessmentId = state.AssessmentId,
                AssessmentVersion = state.AssessmentVersion,
                SuggestedCategory = item.Category,
                ConditionGrade = evidences.Any() ? "Moderate" : "Good",
                ConditionSummary = $"Owner provided {ownerAnswers.Count} answers. Visual inspection found {evidences.Count} observations.",
                VisibleObservations = evidences.Any() ? string.Join("; ", evidences.Select(e => e.Observation)) : "No visible issues.",
                OwnerReportedFunctionality = string.Join("; ", ownerAnswers),
                MissingInformation = null, // Set if needed
                Confidence = evidences.Any() ? 0.9 : 0.6,
                EvidenceReferences = evidences,
                Status = "Draft" // Must not be Confirmed
            };

            // Heuristic missing information
            if (string.IsNullOrWhiteSpace(item.Description))
            {
                draft.MissingInformation = "Item description is missing.";
                draft.Confidence = 0.3; // Low confidence
            }

            return draft;
        }
    }
}
