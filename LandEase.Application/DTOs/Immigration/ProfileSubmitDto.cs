namespace LandEase.Application.DTOs.Immigration;

public class ProfileSubmitDto
{
    public string VisaType { get; set; } = string.Empty;
    public string DestinationCountry { get; set; } = string.Empty;
    public string EducationLevel { get; set; } = string.Empty;
    public int YearsOfWorkExperience { get; set; }
    public string? JobTitle { get; set; }
    public bool HasJobOffer { get; set; }
    public string LanguageTest { get; set; } = string.Empty;
    public decimal LanguageScore { get; set; }
    public decimal AnnualIncome { get; set; }
    public decimal SavingsAmount { get; set; }
    public string MaritalStatus { get; set; } = string.Empty;
    public bool HasFamilyInDestination { get; set; }
    public int Age { get; set; }
    public bool HasPriorVisaRefusal { get; set; }
    public bool HasCriminalRecord { get; set; }
}
