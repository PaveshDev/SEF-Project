namespace WasteToValue.Api.Modules.Items.Entities;

public enum ItemStatus
{
    Draft,
    Submitted,
    Assessing,
    AwaitingOwnerConfirmation,
    Confirmed,
    ReassessmentRequested,
    AvailableForRecovery,
    Withdrawn,
    Completed
}
