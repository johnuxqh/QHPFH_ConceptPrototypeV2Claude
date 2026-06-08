using QHPFH_ConceptPrototype.Models;

namespace QHPFH_ConceptPrototype.Data;

public static class DemoWardData
{
    public static List<WardRecord> BuildWardRecords()
    {
        var wards = new[] { ("ED","Emergency Department","Emergency"), ("GEN","General Medicine","Medical"), ("SUR","Surgical","Surgical"), ("ICU","Intensive Care","Critical Care"), ("MAT","Maternity","Maternity"), ("MAU","Medical Assessment Unit","Assessment") };
        var specials = new[] { ("PICU","PICU","Critical Care"),("NICU","NICU","Critical Care"),("12A","12A Surgical & Orthopaedics","Surgical"),("11A","11A General Medicine","Medical"),("11B","11B Cardiology","Cardiology"),("10A","10A Respiratory","Respiratory"),("10B","10B Neurology","Neurology"),("9A","9A Oncology","Oncology"),("8A","8A Adolescent Medicine","Adolescent"),("ESS","Emergency Short Stay","Short Stay")};
        var reasons = new[]{"Platform matches activity","Insufficient staffing","Maintenance / infrastructure","Safety risks / acuity","Infection control","Internal emergencies","Seasonal adjustments / financial / reconfiguration"};
        var list = new List<WardRecord>();
        var rand = new Random(42);

        foreach (var hhs in DemoReferenceData.FacilitiesByHhs)
        foreach (var facility in hhs.Value)
        {
            var set = facility == "Queensland Children’s Hospital" ? specials : wards;
            foreach (var w in set)
            {
                var physical = rand.Next(16, 46);
                var nonOperational = rand.Next(0, 6);
                var open = Math.Max(physical - nonOperational, 6);
                var occupied = rand.Next((int)(open * 0.7), open + 2);
                var available = Math.Max(open - occupied, -2);
                var occupancy = open == 0 ? 0 : Math.Round((decimal)occupied / open * 100, 1);
                var pressure = occupancy < 80 ? "Low" : occupancy < 90 ? "Medium" : occupancy < 95 ? "High" : "Critical";
                list.Add(new(hhs.Key, facility, w.Item1, w.Item2, $"{w.Item2} operational unit", w.Item3, physical, open, occupied, available, occupancy, pressure, reasons[rand.Next(reasons.Length)], rand.Next(0, 14), rand.Next(0, 10), rand.Next(0, 6), rand.Next(0, 12), rand.Next(0, 11), rand.Next(0, 7), rand.Next(0, 6), nonOperational, rand.Next(-4, 5), DateTime.UtcNow.AddMinutes(-rand.Next(2, 60)).ToString("HH:mm")));
            }
        }

        return list;
    }
}
