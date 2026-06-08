using QHPFH_ConceptPrototype.Models;

namespace QHPFH_ConceptPrototype.Data;

public static class DemoHubData
{
    public static List<PatientAdmission> BuildPatientAdmissions()
    {
        var names = new[]
        {
            ("Bruce","Wayne"),("Clark","Kent"),("Diana","Prince"),("Barry","Allen"),("Arthur","Curry"),("Victor","Stone"),("Selina","Kyle"),("Kara","Danvers"),
            ("Peter","Parker"),("Natasha","Romanoff"),("Wanda","Maximoff"),("Stephen","Strange"),("Jean","Grey"),("Logan","Howlett"),("Kitty","Pryde"),("Matt","Murdock"),("Reed","Richards"),("Peggy","Carter"),("Loki","Laufeyson"),("Rocket","Raccoon"),
            ("Leia","Organa"),("Din","Djarin"),("Luke","Skywalker"),("Han","Solo"),("Sabine","Wren"),("Ahsoka","Tano"),
            ("Harry","Potter"),("Hermione","Granger"),("Luna","Lovegood"),("Draco","Malfoy"),("Neville","Longbottom"),
            ("Frodo","Baggins"),("Aragorn","Elessar"),("Galadriel","Noldor"),("Eowyn","Rohan"),
            ("Jean-Luc","Picard"),("Ellen","Ripley"),("Sarah","Connor"),("Dana","Scully"),("Ellen","Tigh"),("Amos","Burton")
        };

        var rand = new Random(84);
        var statuses = new[] { "Queued", "In Transit", "Admitted", "Complete" };
        var specialties = new[] { "General Surgery", "Cardiology", "General Medicine", "Respiratory", "Orthopaedics", "Neurology" };
        var referral = new[] { "ED", "Elective", "IHT", "OPD" };
        var reasons = new[]
        {
            "Post-op bed request","Abdominal pain escalation","Chest pain pathway","Respiratory compromise","Falls assessment","Sepsis review",
            "Telemetry monitoring needed","Isolation requirement","Discharge delay transfer"
        };

        return Enumerable.Range(1, 140).Select(i =>
        {
            var n = names[i % names.Length];
            return new PatientAdmission(
                $"PT-{i:0000}",
                n.Item1,
                n.Item2,
                rand.Next(4, 92),
                rand.NextDouble() > 0.52 ? "F" : "M",
                referral[rand.Next(referral.Length)],
                "Metro North",
                "Royal Brisbane and Women's Hospital",
                specialties[rand.Next(specialties.Length)],
                rand.NextDouble() > 0.7 ? "ICU" : "Surgical",
                statuses[rand.Next(statuses.Length)],
                DateTime.UtcNow.AddHours(-rand.Next(1, 24)).ToString("HH:mm"),
                DateTime.UtcNow.AddMinutes(rand.Next(15, 320)).ToString("HH:mm"),
                reasons[rand.Next(reasons.Length)],
                rand.NextDouble() > 0.74,
                rand.NextDouble() > 0.81,
                rand.NextDouble() > 0.86,
                rand.Next(0, 5)
            );
        }).ToList();
    }
}
