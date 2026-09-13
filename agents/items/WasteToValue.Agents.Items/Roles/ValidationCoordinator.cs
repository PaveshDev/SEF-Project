using System;
using System.Collections.Generic;
using System.Linq;
using WasteToValue.Agents.Items.Models;
using WasteToValue.Api.Modules.Items.Entities;

namespace WasteToValue.Agents.Items.Roles
{
    public class ValidationCoordinator
    {
        public List<string> Validate(AssessmentWorkflowState state, Item item)
        {
            var errors = new List<string>();

            if (state.FinalAssessmentDraft == null)
            {
                errors.Add("Final assessment draft is missing.");
                return errors;
            }

            var draft = state.FinalAssessmentDraft;

            if (draft.ItemId == Guid.Empty || draft.ItemId != item.Id)
                errors.Add("ItemId is invalid or does not match.");

            if (draft.AssessmentId == Guid.Empty || draft.AssessmentId != state.AssessmentId)
                errors.Add("AssessmentId is invalid or does not match.");

            if (draft.AssessmentVersion <= 0)
                errors.Add("AssessmentVersion is invalid.");

            if (string.IsNullOrWhiteSpace(draft.SuggestedCategory))
                errors.Add("SuggestedCategory is required.");

            if (string.IsNullOrWhiteSpace(draft.ConditionGrade))
                errors.Add("ConditionGrade is required.");

            if (draft.Confidence < 0.0 || draft.Confidence > 1.0)
                errors.Add("Confidence must be between 0.0 and 1.0.");

            if (draft.EvidenceReferences == null || !draft.EvidenceReferences.Any())
                errors.Add("Evidence references are missing.");
            else
            {
                foreach (var evidence in draft.EvidenceReferences)
                {
                    if (evidence.AssessmentId != draft.AssessmentId)
                        errors.Add("Evidence belongs to a different assessment.");

                    if (evidence.PhotoId.HasValue)
                    {
                        if (!item.Photos.Any(p => p.Id == evidence.PhotoId.Value))
                            errors.Add("Evidence photo does not belong to the same Item.");
                    }
                }
            }

            if (draft.Status == "Confirmed" || draft.Status == "Superseded")
                errors.Add("Assessment cannot bypass owner confirmation or be supersede initially.");

            // AI observations separated from owner-reported (ensure fields are populated)
            if (string.IsNullOrWhiteSpace(draft.VisibleObservations) && draft.EvidenceReferences != null && draft.EvidenceReferences.Any())
                errors.Add("VisibleObservations must be populated if evidence exists.");

            // Check forbidden tools
            var forbiddenTools = new[] { "ChangeOwner", "Delete", "ApproveRecovery", "RunSql" };
            if (state.ToolCalls.Any(t => forbiddenTools.Contains(t.ToolName)))
                errors.Add("Forbidden tool operation occurred.");

            return errors;
        }
    }
}
