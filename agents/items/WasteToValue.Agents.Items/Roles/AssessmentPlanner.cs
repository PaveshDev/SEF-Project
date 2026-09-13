using System;
using System.Collections.Generic;
using WasteToValue.Agents.Items.Models;

namespace WasteToValue.Agents.Items.Roles
{
    public class AssessmentPlanner
    {
        public AssessmentWorkflowState CreatePlan(Guid itemId, Guid assessmentId, int assessmentVersion)
        {
            return new AssessmentWorkflowState
            {
                WorkflowId = Guid.NewGuid(),
                ItemId = itemId,
                AssessmentId = assessmentId,
                AssessmentVersion = assessmentVersion,
                Steps = new List<string>
                {
                    "ReadItemAnswers",
                    "InspectImages",
                    "GetCategoryChecklist",
                    "EvaluateCondition",
                    "CheckMissingInformation",
                    "RequestClarificationIfRequired",
                    "BuildAssessmentDraft",
                    "ValidateAssessment",
                    "RequestOwnerConfirmation"
                },
                CurrentStep = "ReadItemAnswers",
                WorkflowStatus = "Running"
            };
        }
    }
}
