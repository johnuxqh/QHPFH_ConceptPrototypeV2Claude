using QHPFH_ConceptPrototype.Models;

namespace QHPFH_ConceptPrototype.Data;

public static class DemoReferenceData
{
    public const string All = "All";

    public static readonly IReadOnlyList<HhsRecord> HhsRecords =
    [
        new("Cairns and Hinterland", ["Cairns Hospital", "Atherton Hospital", "Innisfail Hospital", "Mareeba Hospital", "Gordonvale Hospital", "Herberton Hospital", "Mossman Multipurpose Health Service", "Babinda Multipurpose Health Centre", "Tully Hospital"]),
        new("Central Queensland", ["Rockhampton Hospital", "Gladstone Hospital", "Emerald Hospital", "Biloela Hospital", "Blackwater Multipurpose Health Service", "Moura Hospital", "Mount Morgan Multipurpose Health Service", "Woorabinda Multipurpose Health Service", "Capricorn Coast Hospital and Health Service"]),
        new("Central West", ["Longreach Hospital", "Blackall Hospital", "Barcaldine Multipurpose Health Service", "Winton Multipurpose Health Service", "Alpha Multipurpose Health Service", "Aramac Primary Health Care", "Bedourie Primary Health Care Centre", "Boulia Primary Health Centre and Well-being Centre", "Birdsville Primary Health Centre", "Isisford Primary Health Centre"]),
        new("Children’s Health Queensland", ["Queensland Children's Hospital", "Ellen Barron Family Centre", "Jacaranda Place"]),
        new("Darling Downs", ["Toowoomba Hospital", "Dalby Hospital", "Warwick Hospital", "Stanthorpe Hospital", "Chinchilla Hospital", "Miles Hospital", "Goondiwindi Hospital", "Cherbourg Health Service", "Inglewood Multipurpose Health Service", "Baillie Henderson Hospital"]),
        new("Gold Coast", ["Gold Coast University Hospital", "Robina Hospital", "Varsity Lakes Day Hospital", "Helensvale Community Health Centre", "Southport Health Precinct"]),
        new("Mackay", ["Mackay Base Hospital", "Bowen Hospital", "Sarina Hospital", "Proserpine Hospital", "Moranbah Hospital", "Clermont Multipurpose Health Service", "Collinsville Multipurpose Health Service", "Dysart Hospital"]),
        new("Metro North", ["Royal Brisbane and Women's Hospital", "The Prince Charles Hospital", "Redcliffe Hospital", "Caboolture Hospital", "Kilcoy Hospital", "STARS", "Bribie Island Satellite Health Centre (Yarun)", "Brighton Health Campus", "Chermside Community Health Centre"]),
        new("Metro South", ["Princess Alexandra Hospital", "Queen Elizabeth II Jubilee Hospital", "Logan Hospital", "Beaudesert Hospital", "Bayside Addiction and Mental Health Centre", "Eight Mile Plains Community Health Centre", "Wynnum-Manly Community Health Centre (Gundu Pa)", "Redland Hospital", "Browns Plains Community Health Centre"]),
        new("North West", ["Mount Isa Hospital", "Cloncurry Multipurpose Health Service", "Doomadgee Hospital and Community Health Centre", "Mornington Island Hospital and Aboriginal Community Health Centre", "Julia Creek Multipurpose Health Service", "Normanton Hospital", "McKinlay Primary Health Clinic", "Burketown Primary Health Clinic"]),
        new("South West", ["Roma Hospital", "Charleville Hospital", "St George Hospital", "Mitchell Multipurpose Health Service", "Injune Multipurpose Health Service", "Quilpie Multipurpose Health Service", "Cunnamulla Multipurpose Health Service", "Dirranbandi Multipurpose Health Service", "Augathella Multipurpose Health Service"]),
        new("Sunshine Coast", ["Sunshine Coast University Hospital", "Nambour General Hospital", "Caloundra Health Service", "Maleny Soldiers Memorial Hospital", "Gympie Hospital", "Glenbrook Residential Aged Care Nambour"]),
        new("Torres and Cape", ["Thursday Island Hospital", "Cooktown Multipurpose Health Service", "Bamaga Hospital", "Weipa Integrated Health Service", "Coen Primary Health Care Centre", "Aurukun Primary Health Care Centre", "Hope Vale Primary Health Care Centre", "Saibai Primary Health Care Centre", "Boigu Island Primary Health Care Centre"]),
        new("Townsville", ["Townsville University Hospital", "Ayr Hospital", "Home Hill Hospital", "Charters Towers Health Services", "Ingham Health Services", "Hughenden Multipurpose Health Service", "Palm Island health services", "Magnetic Island Health Service Centre"]),
        new("West Moreton", ["Ipswich Hospital", "Gatton Hospital", "Esk Hospital", "Boonah Hospital", "Ripley Satellite Health Centre", "Goodna Community Health", "Gailes Community Care Unit", "Brisbane Youth Detention Centre"]),
        new("Wide Bay", ["Bundaberg Hospital", "Hervey Bay Hospital", "Maryborough Hospital", "Gayndah Hospital", "Gin Gin Hospital", "Monto Hospital", "Mundubbera Multipurpose Health Service", "Childers Multipurpose Health Service", "Biggenden Multipurpose Health Service"])
    ];

    public static IReadOnlyDictionary<string, List<string>> FacilitiesByHhs => HhsRecords.ToDictionary(x => x.Name, x => x.Facilities.Distinct().Order().ToList());
    public static IReadOnlyList<string> HhsOptions => [All, ..HhsRecords.Select(x => x.Name)];
}
