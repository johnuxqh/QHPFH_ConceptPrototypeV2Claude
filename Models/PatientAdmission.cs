namespace QHPFH_ConceptPrototype.Models;

public sealed record PatientAdmission(
    string PatientId,
    string FirstName,
    string LastName,
    int Age,
    string Sex,
    string SourceType,
    string CurrentHhs,
    string CurrentFacility,
    string CurrentWard,
    string TargetWard,
    string AdmissionStatus,
    string AdmissionTime,
    string EstimatedArrivalTime,
    string ProcedureOrReason,
    bool IsolationRequired,
    bool HighAcuity,
    bool DelayedDischarge,
    int NotesCount);
