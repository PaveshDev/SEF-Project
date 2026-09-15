using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WasteToValue.Agents.Items.Models;
using WasteToValue.Agents.Items.Tools;
using WasteToValue.Api.Modules.Items.Entities;

namespace WasteToValue.Agents.Items.Roles
{
    public class EvidenceInspectionAgent
    {
        private readonly IInspectImagesTool _tool;

        public EvidenceInspectionAgent(IInspectImagesTool tool)
        {
            _tool = tool;
        }

        public async Task<List<EvidenceReference>> InspectImagesAsync(Guid assessmentId, List<ItemPhoto> photos)
        {
            // The agent uses the allowed tool to inspect images
            var evidences = await _tool.InspectImagesAsync(assessmentId, photos);

            // Here, an LLM would normally enforce the rule to not invent facts.
            // For this implementation, we rely on the tool returning only visible observations.
            return evidences;
        }
    }
}
