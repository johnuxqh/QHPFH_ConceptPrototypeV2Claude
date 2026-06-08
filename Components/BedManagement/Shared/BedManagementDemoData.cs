namespace QHPFH_ConceptPrototype.Components.BedManagement.Shared;

public static class BedManagementDemoData
{
    public const string StateId = "qld-statewide";
    public const string MetroSouthHhsId = "hhs-metro-south";
    public const string QchFacilityId = "facility-qch";
    public const string Ward8AId = "ward-qch-8a";

    public static BedManagementState State { get; } = CreateState();

    public static IReadOnlyList<BedManagementOption> GetHhsOptions() =>
        State.HhsNetworks.Select(hhs => new BedManagementOption(hhs.Id, hhs.Name)).ToArray();

    public static IReadOnlyList<BedManagementOption> GetFacilityOptions(string hhsId) =>
        GetHhs(hhsId)?.Facilities.Select(facility => new BedManagementOption(facility.Id, GetFacilityLabel(facility))).ToArray() ?? [];

    public static IReadOnlyList<BedManagementOption> GetWardOptions(string facilityId) =>
        GetFacility(facilityId)?.Wards.Select(ward => new BedManagementOption(ward.Id, ward.Name)).ToArray() ?? [];

    public static CapacitySnapshot GetStatewideSnapshot() =>
        BuildSnapshot(StateId, "Statewide", State.HhsNetworks.SelectMany(hhs => hhs.Facilities).SelectMany(facility => facility.Wards), "Statewide Watch", "Updated 3 min ago");

    public static CapacitySnapshot? GetHhsSnapshot(string hhsId)
    {
        var hhs = GetHhs(hhsId);
        return hhs is null
            ? null
            : BuildSnapshot(hhs.Id, hhs.Name, hhs.Facilities.SelectMany(facility => facility.Wards), GetEscalationStatus(hhs.Facilities.Select(facility => facility.EscalationStatus)), "Updated 3 min ago");
    }

    public static CapacitySnapshot? GetFacilitySnapshot(string facilityId)
    {
        var facility = GetFacility(facilityId);
        return facility is null
            ? null
            : BuildSnapshot(facility.Id, GetFacilityLabel(facility), facility.Wards, facility.EscalationStatus, "Updated 2 min ago");
    }

    public static CapacitySnapshot? GetWardSnapshot(string wardId)
    {
        var ward = GetWard(wardId);
        return ward is null
            ? null
            : BuildSnapshot(ward.Id, ward.Name, [ward], ward.EscalationStatus, ward.LastUpdatedLabel);
    }

    public static HhsNetwork? GetHhs(string hhsId) =>
        State.HhsNetworks.FirstOrDefault(hhs => hhs.Id == hhsId);

    public static Facility? GetFacility(string facilityId) =>
        State.HhsNetworks.SelectMany(hhs => hhs.Facilities).FirstOrDefault(facility => facility.Id == facilityId);

    public static Ward? GetWard(string wardId) =>
        State.HhsNetworks.SelectMany(hhs => hhs.Facilities).SelectMany(facility => facility.Wards).FirstOrDefault(ward => ward.Id == wardId);

    public static string GetHhsLabel(string hhsId) => GetHhs(hhsId)?.Name ?? hhsId;

    public static string GetFacilityLabel(string facilityId) =>
        GetFacility(facilityId) is { } facility ? GetFacilityLabel(facility) : facilityId;

    public static string GetWardLabel(string wardId) => GetWard(wardId)?.Name ?? wardId;

    public static string GetDefaultHhsId() => MetroSouthHhsId;

    public static string GetDefaultFacilityId(string hhsId) =>
        GetFacilityOptions(hhsId).FirstOrDefault()?.Value ?? string.Empty;

    public static string GetDefaultWardId(string facilityId) =>
        GetWardOptions(facilityId).FirstOrDefault()?.Value ?? string.Empty;

    private static string GetFacilityLabel(Facility facility) =>
        string.IsNullOrWhiteSpace(facility.ShortName) ? facility.Name : $"{facility.Name} / {facility.ShortName}";

    private static CapacitySnapshot BuildSnapshot(string id, string label, IEnumerable<Ward> wards, string escalationStatus, string lastUpdatedLabel)
    {
        var wardList = wards.ToArray();
        var beds = wardList.SelectMany(ward => ward.Beds).ToArray();
        var openBeds = beds.Count(bed => bed.IsOpen);
        var occupiedBeds = beds.Count(bed => bed.IsOccupied);

        return new CapacitySnapshot(
            id,
            label,
            beds.Count(bed => bed.IsPhysical),
            openBeds,
            occupiedBeds,
            beds.Count(bed => bed.IsOpen && !bed.IsOccupied && bed.Status == BedStatus.Available),
            beds.Count(bed => bed.Cleaning.Status is FlowStatus.Planned or FlowStatus.InProgress || bed.Status == BedStatus.PendingCleaning),
            beds.Count(bed => bed.Status == BedStatus.Maintenance || !string.Equals(bed.MaintenanceStatus, "Clear", StringComparison.OrdinalIgnoreCase)),
            beds.Count(bed => bed.IsClosed || bed.Status == BedStatus.Closed),
            wardList.Sum(ward => ward.ExpectedAdmissions),
            wardList.Sum(ward => ward.PlannedDischarges),
            wardList.Sum(ward => ward.TransferRequests),
            openBeds == 0 ? 0 : Math.Round((double)occupiedBeds / openBeds * 100, 1),
            escalationStatus,
            lastUpdatedLabel);
    }

    private static string GetEscalationStatus(IEnumerable<string> statuses)
    {
        var statusList = statuses.ToArray();
        if (statusList.Any(status => status.Contains("Tier 3", StringComparison.OrdinalIgnoreCase)))
        {
            return "Tier 3 Watch";
        }

        if (statusList.Any(status => status.Contains("Tier 2", StringComparison.OrdinalIgnoreCase)))
        {
            return "Tier 2 Active";
        }

        return statusList.Any(status => status.Contains("Watch", StringComparison.OrdinalIgnoreCase)) ? "Watch" : "BAU";
    }

    private static BedManagementState CreateState() => new()
    {
        Id = StateId,
        Name = "Statewide",
        HhsNetworks =
        [
            CreateMetroSouthHhs(),
            CreateMetroNorthHhs(),
            CreateGoldCoastHhs()
        ],
        Signals =
        [
            new InsightSignal { Id = "state-flow-watch", Label = "Flow pressure requires support", Detail = "Metro South has the strongest admission, transfer and bed turnaround pressure in the demo hierarchy.", Severity = "Watch", SupportedVersion = BedManagementVersion.V2Balanced },
            new InsightSignal { Id = "state-early-discharge", Label = "Early discharge opportunity", Detail = "Planned discharges should be protected to release capacity ahead of expected admissions.", Severity = "Opportunity", SupportedVersion = BedManagementVersion.V2Balanced },
            new InsightSignal { Id = "state-senior-review", Label = "Senior engagement focus", Detail = "Escalated services should confirm senior review and discharge barriers during the next coordination check.", Severity = "Review", SupportedVersion = BedManagementVersion.V2Balanced },
            new InsightSignal { Id = "state-transfer-focus", Label = "Transfer coordination focus", Detail = "Cross-facility transfer pressure is concentrated in Metro South demo data.", Severity = "Info", SupportedVersion = BedManagementVersion.V3Operational }
        ]
    };

    private static HhsNetwork CreateMetroSouthHhs() => new()
    {
        Id = MetroSouthHhsId,
        Name = "Metro South HHS",
        Facilities =
        [
            CreateQchFacility(),
            CreatePrincessAlexandraFacility(),
            CreateLoganFacility()
        ],
        Signals =
        [
            new InsightSignal { Id = "ms-qch-watch", Label = "QCH flow constraint", Detail = "Ward 8A has pending admissions, transfer delays and bed turnaround dependencies.", Severity = "Amber", SupportedVersion = BedManagementVersion.V2Balanced },
            new InsightSignal { Id = "ms-review-delay", Label = "Senior review visibility", Detail = "QCH high-acuity patients and delayed discharge barriers require confirmation at the next regional review.", Severity = "Review", SupportedVersion = BedManagementVersion.V2Balanced },
            new InsightSignal { Id = "ms-utilisation", Label = "Use available capacity", Detail = "Compare available beds across Metro South facilities before escalating constrained wards.", Severity = "Opportunity", SupportedVersion = BedManagementVersion.V2Balanced }
        ]
    };

    private static HhsNetwork CreateMetroNorthHhs() => new()
    {
        Id = "hhs-metro-north",
        Name = "Metro North HHS",
        Facilities =
        [
            CreateSimpleFacility("facility-rbwh", "Royal Brisbane and Women's Hospital", "RBWH", "hhs-metro-north", "Ward 9A", "Ward 8B"),
            CreateSimpleFacility("facility-tpch", "The Prince Charles Hospital", "TPCH", "hhs-metro-north", "Cardiac Stepdown", "Ward 2A")
        ],
        Signals =
        [
            new InsightSignal { Id = "mn-flow-stable", Label = "Metro North flow stable", Detail = "Demo capacity remains suitable for comparison against Metro South.", Severity = "Info" }
        ]
    };

    private static HhsNetwork CreateGoldCoastHhs() => new()
    {
        Id = "hhs-gold-coast",
        Name = "Gold Coast HHS",
        Facilities =
        [
            CreateSimpleFacility("facility-gcuh", "Gold Coast University Hospital", "GCUH", "hhs-gold-coast", "Ward C6", "Ward D5"),
            CreateSimpleFacility("facility-robina", "Robina Hospital", "Robina", "hhs-gold-coast", "Ward M1", "Ward S2")
        ],
        Signals =
        [
            new InsightSignal { Id = "gc-comparison", Label = "Gold Coast comparison baseline", Detail = "Lower demo pressure creates contrast for future statewide screens.", Severity = "Info" }
        ]
    };

    private static Facility CreateQchFacility() => new()
    {
        Id = QchFacilityId,
        HhsId = MetroSouthHhsId,
        Name = "Queensland Children’s Hospital",
        ShortName = "QCH",
        EdPressureIndicator = "High",
        DischargePressureIndicator = "High",
        TransferPressureIndicator = "Amber",
        EscalationStatus = "Tier 2 Active",
        Wards =
        [
            CreateWard8A(),
            CreateQchWard("ward-qch-8b", "Ward 8B", "Paediatric medicine", 2, 2, 1,
            [
                CreateBed("qch-8b-01", "8B-01", "ward-qch-8b", BedStatus.Occupied, patient: Patient("pt-velma", "Velma Dinkley", "General medicine", "Moderate"), discharge: Discharge(FlowStatus.Planned, "14:30", "Awaiting medicines", "Home"), next: "Confirm medicines-to-bedside", note: "Planned discharge supports afternoon capacity."),
                CreateBed("qch-8b-02", "8B-02", "ward-qch-8b", BedStatus.Available, next: "Hold for ED admission", note: "Single room not required."),
                CreateBed("qch-8b-03", "8B-03", "ward-qch-8b", BedStatus.Occupied, patient: Patient("pt-shaggy", "Norville Rogers", "General medicine", "Low"), next: "Routine ward review", note: "No bed constraint."),
                CreateBed("qch-8b-04", "8B-04", "ward-qch-8b", BedStatus.PendingCleaning, isOpen: true, cleaning: Cleaning(FlowStatus.InProgress, "Terminal clean", "12:20"), next: "Release after terminal clean", note: "Environmental clean underway."),
                CreateBed("qch-8b-05", "8B-05", "ward-qch-8b", BedStatus.Occupied, patient: Patient("pt-daphne", "Daphne Blake", "General medicine", "Moderate"), transfer: Transfer(FlowStatus.Planned, "Outbound", "Ward 8A", "15:00"), next: "Check 8A capacity before transfer", note: "Transfer depends on Ward 8A discharge."),
                CreateBed("qch-8b-06", "8B-06", "ward-qch-8b", BedStatus.Available, isolation: true, next: "Reserve isolation-capable bed", note: "Suitable for respiratory admission.")
            ]),
            CreateQchWard("ward-qch-picu", "PICU", "Paediatric intensive care", 1, 1, 2,
            [
                CreateBed("qch-picu-01", "PICU-01", "ward-qch-picu", BedStatus.Occupied, patient: Patient("pt-miles", "Miles Morales", "PICU", "High"), next: "Senior review", note: "High acuity demo patient."),
                CreateBed("qch-picu-02", "PICU-02", "ward-qch-picu", BedStatus.Occupied, patient: Patient("pt-gwen", "Gwen Stacy", "PICU", "High"), transfer: Transfer(FlowStatus.InProgress, "Step-down", "Ward 8A", "16:00"), next: "Step-down once Ward 8A bed clear", note: "Contributes to 8A pressure."),
                CreateBed("qch-picu-03", "PICU-03", "ward-qch-picu", BedStatus.Available, isolation: true, next: "Keep open for escalation", note: "Isolation-capable critical care bed."),
                CreateBed("qch-picu-04", "PICU-04", "ward-qch-picu", BedStatus.Occupied, patient: Patient("pt-hiccup", "Hiccup Haddock", "PICU", "High"), next: "Ventilation review", note: "No transfer today."),
                CreateBed("qch-picu-05", "PICU-05", "ward-qch-picu", BedStatus.Maintenance, isOpen: false, maintenance: "Biomedical check", next: "Await biomedical clearance", note: "Maintenance hold."),
                CreateBed("qch-picu-06", "PICU-06", "ward-qch-picu", BedStatus.Occupied, patient: Patient("pt-astrid", "Astrid Hofferson", "PICU", "Moderate"), discharge: Discharge(FlowStatus.Planned, "18:00", "Step-down bed pending", "Ward 8B"), next: "Confirm step-down", note: "Potential evening movement.")
            ]),
            CreateQchWard("ward-qch-edss", "ED Short Stay", "Emergency short stay", 3, 2, 1,
            [
                CreateBed("qch-edss-01", "EDSS-01", "ward-qch-edss", BedStatus.Occupied, patient: Patient("pt-luna", "Luna Lovegood", "Emergency", "Moderate"), admission: Admission(FlowStatus.InProgress, "ED", "13:00", "Await ward bed"), next: "Allocate ward bed", note: "Likely Ward 8A admission."),
                CreateBed("qch-edss-02", "EDSS-02", "ward-qch-edss", BedStatus.Occupied, patient: Patient("pt-neville", "Neville Longbottom", "Emergency", "Low"), discharge: Discharge(FlowStatus.Ready, "12:00", "Transport booked", "Home"), next: "Confirm departure", note: "Short stay discharge ready."),
                CreateBed("qch-edss-03", "EDSS-03", "ward-qch-edss", BedStatus.Occupied, patient: Patient("pt-katara", "Katara Watertribe", "Emergency", "Moderate"), admission: Admission(FlowStatus.Planned, "ED", "14:00", "Needs isolation-capable bed"), next: "Find isolation bed", note: "Isolation capability required."),
                CreateBed("qch-edss-04", "EDSS-04", "ward-qch-edss", BedStatus.PendingCleaning, cleaning: Cleaning(FlowStatus.Planned, "Routine clean", "12:45"), next: "Clean after discharge", note: "Could reopen for surge.")
            ])
        ],
        OperationalEvents =
        [
            new OperationalEvent { Id = "qch-event-ed", TimeLabel = "11:45", Type = "ED pressure", Summary = "ED short stay has two likely ward admissions awaiting bed decisions.", Severity = "Amber" },
            new OperationalEvent { Id = "qch-event-cleaning", TimeLabel = "12:05", Type = "Cleaning", Summary = "Three QCH beds have cleaning dependencies before release.", Severity = "Info" }
        ],
        Signals =
        [
            new InsightSignal { Id = "qch-8a-flow", Label = "Ward 8A is blocking flow", Detail = "Admissions, transfer delays, cleaning turnaround and discharge timing converge on Ward 8A.", Severity = "Amber", SupportedVersion = BedManagementVersion.V2Balanced },
            new InsightSignal { Id = "qch-early-discharge", Label = "Early discharge opportunities", Detail = "Prioritise discharge-ready patients and resolve medicines, transport and home oxygen barriers.", Severity = "Opportunity", SupportedVersion = BedManagementVersion.V2Balanced },
            new InsightSignal { Id = "qch-senior-review", Label = "Senior review required", Detail = "High-acuity and delayed-flow patients need visible senior review before the next capacity check.", Severity = "Review", SupportedVersion = BedManagementVersion.V2Balanced }
        ]
    };

    private static Ward CreateWard8A() => new()
    {
        Id = Ward8AId,
        FacilityId = QchFacilityId,
        Name = "Ward 8A",
        Specialty = "Paediatric respiratory and general medicine",
        ExpectedAdmissions = 5,
        PlannedDischarges = 4,
        TransferRequests = 3,
        EscalationStatus = "Tier 2 Active",
        LastUpdatedLabel = "Updated 2 min ago",
        Beds =
        [
            CreateBed("qch-8a-01", "8A-01", Ward8AId, BedStatus.Occupied, isolation: true, patient: Patient("pt-peter", "Peter Parker", "Respiratory", "Moderate"), discharge: Discharge(FlowStatus.Planned, "13:30", "Awaiting discharge script", "Home"), next: "Expedite discharge script", note: "Priority discharge unlocks respiratory admission."),
            CreateBed("qch-8a-02", "8A-02", Ward8AId, BedStatus.Occupied, patient: Patient("pt-wanda", "Wanda Maximoff", "General medicine", "Moderate"), transfer: Transfer(FlowStatus.Planned, "Outbound", "Ward 8B", "15:30"), next: "Confirm Ward 8B receiving bed", note: "Transfer request affects 8A capacity."),
            CreateBed("qch-8a-03", "8A-03", Ward8AId, BedStatus.PendingCleaning, cleaning: Cleaning(FlowStatus.InProgress, "Terminal clean", "12:35"), next: "Release bed after cleaning", note: "Next ED admission is waiting."),
            CreateBed("qch-8a-04", "8A-04", Ward8AId, BedStatus.Available, isolation: true, next: "Hold for isolation admission", note: "Only open isolation-capable 8A bed."),
            CreateBed("qch-8a-05", "8A-05", Ward8AId, BedStatus.Occupied, patient: Patient("pt-tony", "Tony Stark", "Respiratory", "High"), discharge: Discharge(FlowStatus.Delayed, "16:00", "Home oxygen confirmation", "Home"), next: "Escalate oxygen confirmation", note: "Discharge delay has high flow impact."),
            CreateBed("qch-8a-06", "8A-06", Ward8AId, BedStatus.Occupied, patient: Patient("pt-steve", "Steve Rogers", "General medicine", "Low"), discharge: Discharge(FlowStatus.Ready, "12:15", "Family pickup", "Home"), next: "Confirm family pickup", note: "Ready discharge; bed can release after clean."),
            CreateBed("qch-8a-07", "8A-07", Ward8AId, BedStatus.Occupied, patient: Patient("pt-natasha", "Natasha Romanoff", "Respiratory", "Moderate"), transfer: Transfer(FlowStatus.InProgress, "Inbound", "PICU", "16:00"), next: "Prepare for PICU step-down", note: "Step-down patient expected this afternoon."),
            CreateBed("qch-8a-08", "8A-08", Ward8AId, BedStatus.Maintenance, isOpen: false, maintenance: "Curtain track repair", next: "Facilities review", note: "Closed for minor maintenance."),
            CreateBed("qch-8a-09", "8A-09", Ward8AId, BedStatus.Occupied, isolation: true, patient: Patient("pt-bruce", "Bruce Banner", "Respiratory", "High"), next: "Senior respiratory review", note: "Isolation precautions in place."),
            CreateBed("qch-8a-10", "8A-10", Ward8AId, BedStatus.Occupied, patient: Patient("pt-carol", "Carol Danvers", "General medicine", "Moderate"), admission: Admission(FlowStatus.Complete, "Theatre", "09:20", ""), next: "Post-admission observations", note: "New admission this morning."),
            CreateBed("qch-8a-11", "8A-11", Ward8AId, BedStatus.Closed, isOpen: false, isClosed: true, next: "Nurse unit manager review", note: "Closed due to staffing constraint."),
            CreateBed("qch-8a-12", "8A-12", Ward8AId, BedStatus.PendingTransfer, patient: Patient("pt-diana", "Diana Prince", "General medicine", "Low"), transfer: Transfer(FlowStatus.Ready, "Outbound", "Transit lounge", "12:30"), next: "Move to transit lounge", note: "Ready transfer could free bed soon.")
        ],
        OperationalEvents =
        [
            new OperationalEvent { Id = "8a-clean", TimeLabel = "12:05", Type = "Bed turnaround", Summary = "8A-03 terminal clean in progress for the next ED admission.", Severity = "Info" },
            new OperationalEvent { Id = "8a-discharge", TimeLabel = "12:10", Type = "Early discharge", Summary = "8A-06 is discharge ready and awaiting family pickup.", Severity = "Green" },
            new OperationalEvent { Id = "8a-maintenance", TimeLabel = "12:20", Type = "Utilisation constraint", Summary = "8A-08 remains closed for facilities review.", Severity = "Amber" },
            new OperationalEvent { Id = "8a-senior-review", TimeLabel = "12:25", Type = "Senior review", Summary = "8A-09 requires senior respiratory review before the next journey decision.", Severity = "Review" }
        ],
        OperationalSignals =
        [
            new OperationalSignal { Id = "8a-next-bed", Label = "Flow: next bed decision", Detail = "Prioritise 8A-03 clean completion and 8A-06 early discharge movement.", Owner = "Bed manager", DueLabel = "Next 30 min", Severity = "Amber" },
            new OperationalSignal { Id = "8a-senior-review", Label = "Senior review", Detail = "Confirm senior respiratory review for 8A-09 before the next journey decision.", Owner = "Clinical team", DueLabel = "By 13:00", Severity = "Review" },
            new OperationalSignal { Id = "8a-discharge-barrier", Label = "Discharge barrier", Detail = "Resolve home oxygen confirmation for 8A-05 to protect discharge potential.", Owner = "Flow coordinator", DueLabel = "By 14:00", Severity = "Amber" },
            new OperationalSignal { Id = "8a-transfer-delay", Label = "Transfer delay", Detail = "Move 8A-12 to the transit lounge and confirm the PICU step-down plan.", Owner = "Bed manager", DueLabel = "Next 30 min", Severity = "Watch" },
            new OperationalSignal { Id = "8a-isolation", Label = "Utilisation: isolation constraint", Detail = "Only one open isolation-capable bed is currently available in Ward 8A.", Owner = "NUM", DueLabel = "Now", Severity = "Watch" }
        ]
    };

    private static Facility CreatePrincessAlexandraFacility() => new()
    {
        Id = "facility-pah",
        HhsId = MetroSouthHhsId,
        Name = "Princess Alexandra Hospital",
        ShortName = "PAH",
        EdPressureIndicator = "Moderate",
        DischargePressureIndicator = "Moderate",
        TransferPressureIndicator = "Normal",
        EscalationStatus = "Watch",
        Wards =
        [
            CreateAdultWard("ward-pah-4e", "facility-pah", "Ward 4E"),
            CreateAdultWard("ward-pah-5d", "facility-pah", "Ward 5D")
        ]
    };

    private static Facility CreateLoganFacility() => new()
    {
        Id = "facility-logan",
        HhsId = MetroSouthHhsId,
        Name = "Logan Hospital",
        ShortName = "Logan",
        EdPressureIndicator = "Moderate",
        DischargePressureIndicator = "Normal",
        TransferPressureIndicator = "Normal",
        EscalationStatus = "BAU",
        Wards =
        [
            CreateAdultWard("ward-logan-3a", "facility-logan", "Ward 3A"),
            CreateAdultWard("ward-logan-4b", "facility-logan", "Ward 4B")
        ]
    };

    private static Facility CreateSimpleFacility(string id, string name, string shortName, string hhsId, string firstWardName, string secondWardName) => new()
    {
        Id = id,
        HhsId = hhsId,
        Name = name,
        ShortName = shortName,
        EdPressureIndicator = "Normal",
        DischargePressureIndicator = "Normal",
        TransferPressureIndicator = "Normal",
        EscalationStatus = "BAU",
        Wards =
        [
            CreateAdultWard($"ward-{id}-1", id, firstWardName),
            CreateAdultWard($"ward-{id}-2", id, secondWardName)
        ]
    };

    private static Ward CreateQchWard(string id, string name, string specialty, int expectedAdmissions, int plannedDischarges, int transferRequests, IReadOnlyList<Bed> beds) => new()
    {
        Id = id,
        FacilityId = QchFacilityId,
        Name = name,
        Specialty = specialty,
        ExpectedAdmissions = expectedAdmissions,
        PlannedDischarges = plannedDischarges,
        TransferRequests = transferRequests,
        EscalationStatus = expectedAdmissions > plannedDischarges ? "Watch" : "BAU",
        LastUpdatedLabel = "Updated 4 min ago",
        Beds = beds
    };

    private static Ward CreateAdultWard(string id, string facilityId, string name) => new()
    {
        Id = id,
        FacilityId = facilityId,
        Name = name,
        Specialty = "General medicine",
        ExpectedAdmissions = 1,
        PlannedDischarges = 1,
        TransferRequests = 0,
        EscalationStatus = "BAU",
        LastUpdatedLabel = "Updated 8 min ago",
        Beds =
        [
            CreateBed($"{id}-01", "01", id, BedStatus.Occupied, patient: Patient($"pt-{id}-a", "Clark Kent", "General medicine", "Low"), discharge: Discharge(FlowStatus.Planned, "15:00", "Routine paperwork", "Home"), next: "Routine discharge planning", note: "Stable comparison bed."),
            CreateBed($"{id}-02", "02", id, BedStatus.Occupied, patient: Patient($"pt-{id}-b", "Lois Lane", "General medicine", "Moderate"), next: "Continue care", note: "Occupied comparison bed."),
            CreateBed($"{id}-03", "03", id, BedStatus.Available, next: "Available for allocation", note: "Open bed for comparison."),
            CreateBed($"{id}-04", "04", id, BedStatus.PendingCleaning, cleaning: Cleaning(FlowStatus.Planned, "Routine clean", "13:10"), next: "Clean before allocation", note: "Cleaning dependency."),
            CreateBed($"{id}-05", "05", id, BedStatus.Occupied, patient: Patient($"pt-{id}-c", "Barbara Gordon", "General medicine", "Low"), next: "Routine observations", note: "No active constraint."),
            CreateBed($"{id}-06", "06", id, BedStatus.Available, isolation: true, next: "Keep available", note: "Isolation-capable comparison bed.")
        ]
    };

    private static Bed CreateBed(
        string id,
        string label,
        string wardId,
        BedStatus status,
        bool isOpen = true,
        bool isClosed = false,
        bool isolation = false,
        string maintenance = "Clear",
        DemoPatient? patient = null,
        AdmissionFlow? admission = null,
        DischargeFlow? discharge = null,
        TransferFlow? transfer = null,
        CleaningFlow? cleaning = null,
        string next = "",
        string note = "") => new()
    {
        Id = id,
        Label = label,
        WardId = wardId,
        Status = status,
        IsOpen = isOpen && status is not BedStatus.Closed and not BedStatus.Maintenance,
        IsOccupied = patient is not null || status is BedStatus.Occupied or BedStatus.PendingTransfer,
        IsClosed = isClosed || status is BedStatus.Closed,
        IsPending = status is BedStatus.PendingCleaning or BedStatus.PendingTransfer,
        HasIsolationCapability = isolation,
        Cleaning = cleaning ?? Cleaning(status == BedStatus.PendingCleaning ? FlowStatus.Planned : FlowStatus.NotStarted, status == BedStatus.PendingCleaning ? "Awaiting clean" : "Not required", string.Empty),
        MaintenanceStatus = status == BedStatus.Maintenance && maintenance == "Clear" ? "Maintenance hold" : maintenance,
        AssignedPatient = patient,
        Admission = admission,
        Discharge = discharge,
        Transfer = transfer,
        NextExpectedAction = next,
        OperationalNote = note
    };

    private static DemoPatient Patient(string id, string displayName, string careTeam, string acuity) => new()
    {
        Id = id,
        DisplayName = displayName,
        CareTeam = careTeam,
        AcuityLabel = acuity,
        PatientNote = "Fictional demo patient only; no real identifiers are used."
    };

    private static AdmissionFlow Admission(FlowStatus status, string source, string expectedTime, string constraint) => new()
    {
        Status = status,
        Source = source,
        ExpectedTime = expectedTime,
        Constraint = constraint
    };

    private static DischargeFlow Discharge(FlowStatus status, string plannedTime, string barrier, string destination) => new()
    {
        Status = status,
        PlannedTime = plannedTime,
        Barrier = barrier,
        Destination = destination
    };

    private static TransferFlow Transfer(FlowStatus status, string direction, string targetWard, string requestedTime) => new()
    {
        Status = status,
        Direction = direction,
        TargetWard = targetWard,
        RequestedTime = requestedTime
    };

    private static CleaningFlow Cleaning(FlowStatus status, string cleaningStatus, string readyBy) => new()
    {
        Status = status,
        CleaningStatus = cleaningStatus,
        ReadyBy = readyBy
    };
}
