using System;
using System.Linq;
using System.Threading.Tasks;
using WasteToValue.Agents.Items.Models;
using WasteToValue.Agents.Items.Roles;
using WasteToValue.Agents.Items.Tools;
using WasteToValue.Api.Modules.Items.Entities;

namespace WasteToValue.Agents.Items.Workflow
{
    public class AssessmentWorkflowRunner
    {
        private readonly AssessmentPlanner _planner;
        private readonly EvidenceInspectionAgent _evidenceAgent;
        private readonly AssessmentAgent _assessmentAgent;
        private readonly ValidationCoordinator _validationCoordinator;
        private readonly IRequestClarificationTool _clarificationTool;
        private readonly ISaveAssessmentDraftTool _saveTool;

        public AssessmentWorkflowRunner(
            AssessmentPlanner planner,
            EvidenceInspectionAgent evidenceAgent,
            AssessmentAgent assessmentAgent,
            ValidationCoordinator validationCoordinator,
            IRequestClarificationTool clarificationTool,
            ISaveAssessmentDraftTool saveTool)
        {
            _planner = planner;
            _evidenceAgent = evidenceAgent;
            _assessmentAgent = assessmentAgent;
            _validationCoordinator = validationCoordinator;
            _clarificationTool = clarificationTool;
            _saveTool = saveTool;
        }

        public async Task<AssessmentWorkflowState> RunWorkflowAsync(Item item, AssessmentWorkflowState? existingState = null)
        {
            var state = existingState ?? _planner.CreatePlan(item.Id, Guid.NewGuid(), 1);

            if (state.WorkflowStatus == "Failed" || state.WorkflowStatus == "PendingConfirmation")
                return state;

            try
            {
                // Resume logic: if we were waiting for information, check if clarification is answered
                if (state.WorkflowStatus == "AwaitingInformation")
                {
                    // Check item clarifications. If answered, resume.
                    var pendingClarification = item.Assessments
                        ?.SelectMany(a => a.Clarifications)
                        ?.FirstOrDefault(c => state.ClarificationReferences.Contains(c.Id) && c.Status != "Answered");
                    
                    if (pendingClarification != null)
                    {
                        // Still waiting
                        return state;
                    }
                    state.WorkflowStatus = "Running";
                    state.CurrentStep = "BuildAssessmentDraft";
                }

                // Steps Execution
                var evidences = await _evidenceAgent.InspectImagesAsync(state.AssessmentId, item.Photos?.ToList() ?? new List<ItemPhoto>());
                
                var draft = await _assessmentAgent.BuildAssessmentDraftAsync(state, item, evidences);

                // Check missing info
                if (!string.IsNullOrWhiteSpace(draft.MissingInformation) && state.WorkflowStatus != "AwaitingInformation" && !state.ClarificationReferences.Any())
                {
                    state.CurrentStep = "RequestClarificationIfRequired";
                    await _clarificationTool.RequestClarificationAsync(item.Id, state.AssessmentId, "MISSING_INFO", draft.MissingInformation, "Required for accurate assessment.");
                    
                    // Simulate a clarification id being stored
                    state.ClarificationReferences.Add(Guid.NewGuid());
                    state.WorkflowStatus = "AwaitingInformation";
                    return state;
                }

                state.CurrentStep = "BuildAssessmentDraft";
                state.FinalAssessmentDraft = draft;

                state.CurrentStep = "ValidateAssessment";
                var validationErrors = _validationCoordinator.Validate(state, item);

                if (validationErrors.Any())
                {
                    state.ValidationResults = validationErrors;
                    state.WorkflowStatus = "Failed";
                    return state;
                }

                // Validation succeeds
                state.CurrentStep = "RequestOwnerConfirmation";
                draft.Status = "PendingConfirmation";
                await _saveTool.SaveDraftAsync(draft);
                
                state.WorkflowStatus = "PendingConfirmation";
                state.UpdatedAt = DateTime.UtcNow;

                return state;
            }
            catch (Exception ex)
            {
                state.WorkflowStatus = "Failed";
                state.ValidationResults.Add($"Exception: {ex.Message}");
                return state;
            }
        }
    }
}
