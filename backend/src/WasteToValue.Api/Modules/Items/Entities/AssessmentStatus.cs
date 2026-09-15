namespace WasteToValue.Api.Modules.Items.Entities;

public enum AssessmentStatus
{
    Draft,
    AwaitingInformation,
    PendingConfirmation,
    Confirmed,
    Rejected,
    ReassessmentRequested,
    Superseded,
    Failed
}
