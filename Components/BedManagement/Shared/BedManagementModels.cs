namespace QHPFH_ConceptPrototype.Components.BedManagement.Shared;

public enum BedManagementLevel
{
    Statewide,
    Hhs,
    Facility,
    Ward
}

public enum BedManagementVersion
{
    V1DataInsights,
    V2Balanced,
    V3Operational
}

public enum BedStatus
{
    Available,
    Occupied,
    Closed,
    PendingCleaning,
    PendingTransfer,
    Maintenance
}

public enum FlowStatus
{
    NotStarted,
    Planned,
    InProgress,
    Ready,
    Delayed,
    Complete
}

public sealed record BedManagementOption(string Value, string Label);

public sealed record BedManagementReference(
    BedManagementLevel Level,
    BedManagementVersion Version,
    string Purpose,
    string ReferenceFile);

public sealed class BedManagementState
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<HhsNetwork> HhsNetworks { get; init; } = [];
    public IReadOnlyList<InsightSignal> Signals { get; init; } = [];
    public int TotalPhysicalBeds => HhsNetworks.Sum(hhs => hhs.TotalPhysicalBeds);
    public int OpenBeds => HhsNetworks.Sum(hhs => hhs.OpenBeds);
    public int OccupiedBeds => HhsNetworks.Sum(hhs => hhs.OccupiedBeds);
    public int AvailableBeds => HhsNetworks.Sum(hhs => hhs.AvailableBeds);
    public double OccupancyPercentage => OpenBeds == 0 ? 0 : Math.Round((double)OccupiedBeds / OpenBeds * 100, 1);
}

public sealed class HhsNetwork
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<Facility> Facilities { get; init; } = [];
    public IReadOnlyList<InsightSignal> Signals { get; init; } = [];
    public int TotalPhysicalBeds => Facilities.Sum(facility => facility.TotalPhysicalBeds);
    public int OpenBeds => Facilities.Sum(facility => facility.OpenBeds);
    public int OccupiedBeds => Facilities.Sum(facility => facility.OccupiedBeds);
    public int AvailableBeds => Facilities.Sum(facility => facility.AvailableBeds);
    public double OccupancyPercentage => OpenBeds == 0 ? 0 : Math.Round((double)OccupiedBeds / OpenBeds * 100, 1);
}

public sealed class Facility
{
    public string Id { get; init; } = string.Empty;
    public string HhsId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string ShortName { get; init; } = string.Empty;
    public string EdPressureIndicator { get; init; } = "Normal";
    public string DischargePressureIndicator { get; init; } = "Normal";
    public string TransferPressureIndicator { get; init; } = "Normal";
    public string EscalationStatus { get; init; } = "BAU";
    public IReadOnlyList<Ward> Wards { get; init; } = [];
    public IReadOnlyList<OperationalEvent> OperationalEvents { get; init; } = [];
    public IReadOnlyList<InsightSignal> Signals { get; init; } = [];
    public int TotalPhysicalBeds => Wards.Sum(ward => ward.PhysicalBeds);
    public int OpenBeds => Wards.Sum(ward => ward.OpenBeds);
    public int OccupiedBeds => Wards.Sum(ward => ward.OccupiedBeds);
    public int AvailableBeds => Wards.Sum(ward => ward.AvailableBeds);
    public double OccupancyPercentage => OpenBeds == 0 ? 0 : Math.Round((double)OccupiedBeds / OpenBeds * 100, 1);
}

public sealed class Ward
{
    public string Id { get; init; } = string.Empty;
    public string FacilityId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Specialty { get; init; } = string.Empty;
    public int ExpectedAdmissions { get; init; }
    public int PlannedDischarges { get; init; }
    public int TransferRequests { get; init; }
    public string EscalationStatus { get; init; } = "BAU";
    public string LastUpdatedLabel { get; init; } = "Updated 5 min ago";
    public IReadOnlyList<Bed> Beds { get; init; } = [];
    public IReadOnlyList<OperationalEvent> OperationalEvents { get; init; } = [];
    public IReadOnlyList<OperationalSignal> OperationalSignals { get; init; } = [];
    public int PhysicalBeds => Beds.Count(bed => bed.IsPhysical);
    public int OpenBeds => Beds.Count(bed => bed.IsOpen);
    public int OccupiedBeds => Beds.Count(bed => bed.IsOccupied);
    public int AvailableBeds => Beds.Count(bed => bed.IsOpen && !bed.IsOccupied && bed.Status == BedStatus.Available);
    public int PendingCleaning => Beds.Count(bed => bed.Cleaning.Status is FlowStatus.Planned or FlowStatus.InProgress || bed.Status == BedStatus.PendingCleaning);
    public int PendingMaintenance => Beds.Count(bed => bed.Status == BedStatus.Maintenance || !string.Equals(bed.MaintenanceStatus, "Clear", StringComparison.OrdinalIgnoreCase));
    public int ClosedBeds => Beds.Count(bed => bed.IsClosed || bed.Status == BedStatus.Closed);
}

public sealed class Bed
{
    public string Id { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string WardId { get; init; } = string.Empty;
    public BedStatus Status { get; init; }
    public bool IsPhysical { get; init; } = true;
    public bool IsOpen { get; init; } = true;
    public bool IsOccupied { get; init; }
    public bool IsClosed { get; init; }
    public bool IsPending { get; init; }
    public bool HasIsolationCapability { get; init; }
    public CleaningFlow Cleaning { get; init; } = new();
    public string MaintenanceStatus { get; init; } = "Clear";
    public DemoPatient? AssignedPatient { get; init; }
    public AdmissionFlow? Admission { get; init; }
    public DischargeFlow? Discharge { get; init; }
    public TransferFlow? Transfer { get; init; }
    public string NextExpectedAction { get; init; } = string.Empty;
    public string OperationalNote { get; init; } = string.Empty;
}

public sealed class DemoPatient
{
    public string Id { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string CareTeam { get; init; } = string.Empty;
    public string AcuityLabel { get; init; } = string.Empty;
    public string PatientNote { get; init; } = string.Empty;
}

public sealed class AdmissionFlow
{
    public FlowStatus Status { get; init; } = FlowStatus.NotStarted;
    public string Source { get; init; } = string.Empty;
    public string ExpectedTime { get; init; } = string.Empty;
    public string Constraint { get; init; } = string.Empty;
}

public sealed class DischargeFlow
{
    public FlowStatus Status { get; init; } = FlowStatus.NotStarted;
    public string PlannedTime { get; init; } = string.Empty;
    public string Barrier { get; init; } = string.Empty;
    public string Destination { get; init; } = string.Empty;
}

public sealed class TransferFlow
{
    public FlowStatus Status { get; init; } = FlowStatus.NotStarted;
    public string Direction { get; init; } = string.Empty;
    public string TargetWard { get; init; } = string.Empty;
    public string RequestedTime { get; init; } = string.Empty;
}

public sealed class CleaningFlow
{
    public FlowStatus Status { get; init; } = FlowStatus.NotStarted;
    public string CleaningStatus { get; init; } = "Not required";
    public string ReadyBy { get; init; } = string.Empty;
}

public sealed class OperationalEvent
{
    public string Id { get; init; } = string.Empty;
    public string TimeLabel { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Severity { get; init; } = "Info";
}

public sealed record CapacitySnapshot(
    string Id,
    string Label,
    int PhysicalBeds,
    int OpenBeds,
    int OccupiedBeds,
    int AvailableBeds,
    int PendingCleaning,
    int PendingMaintenance,
    int ClosedBeds,
    int ExpectedAdmissions,
    int PlannedDischarges,
    int TransferRequests,
    double OccupancyPercentage,
    string EscalationStatus,
    string LastUpdatedLabel);

public sealed class InsightSignal
{
    public string Id { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
    public string Severity { get; init; } = "Info";
    public BedManagementVersion SupportedVersion { get; init; } = BedManagementVersion.V2Balanced;
}

public sealed class OperationalSignal
{
    public string Id { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
    public string Owner { get; init; } = string.Empty;
    public string DueLabel { get; init; } = string.Empty;
    public string Severity { get; init; } = "Info";
}

public sealed record PerformanceRow(string Label, string Value, string Status = "Info", string Detail = "");
