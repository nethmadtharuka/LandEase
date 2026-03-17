namespace LandEase.Infrastructure;

public static class ScoringEngine
{
    public static (int score, string note) ScoreEducation(string level)
    {
        return level switch
        {
            "PhD"      => (25, "Excellent — doctoral qualification is highly valued"),
            "Master"   => (22, "Strong — postgraduate qualification"),
            "Bachelor" => (18, "Good — bachelor degree meets standard requirements"),
            "Diploma"  => (12, "Moderate — vocational qualification"),
            _          => (5,  "Weak — higher education strengthens applications"),
        };
    }

    public static (int score, string note) ScoreWorkExperience(int years, bool hasJobOffer)
    {
        var base_ = years switch
        {
            >= 10 => 20,
            >= 7  => 17,
            >= 5  => 14,
            >= 3  => 10,
            >= 1  => 6,
            _     => 2
        };
        if (hasJobOffer) base_ += 5;
        return (Math.Min(base_, 25),
            hasJobOffer ? "Strong — job offer significantly boosts eligibility"
                        : "Note: a job offer would strengthen this category");
    }

    public static (int score, string note) ScoreLanguage(string test, decimal score_)
    {
        if (test == "None") return (0, "Weak — language test required for most visa types");
        var band = test == "IELTS" ? (int)(score_ * 2.5m)
                 : test == "PTE"   ? (int)(score_ / 10)
                 : (int)(score_ / 12);
        return band switch
        {
            >= 20 => (20, "Excellent language proficiency"),
            >= 16 => (16, "Good language score"),
            >= 12 => (12, "Moderate — consider retaking for higher score"),
            _     => (6,  "Weak — improving language score will help significantly"),
        };
    }

    public static (int score, string note) ScoreFinancials(decimal savings)
    {
        return savings switch
        {
            >= 100000 => (15, "Excellent financial standing"),
            >= 50000  => (12, "Good financial evidence"),
            >= 20000  => (8,  "Moderate — more savings would strengthen the application"),
            >= 5000   => (4,  "Weak — financial evidence needs to be stronger"),
            _         => (1,  "Insufficient — financial proof is critical"),
        };
    }

    public static (int score, string note) ScorePersonal(
        int age, string marital, bool hasFamily, bool hasPriorRefusal, bool hasCriminal)
    {
        var s     = 15;
        var notes = new List<string>();

        // FIX 3: marital was received but never used — now actually applied
        if (marital == "Married")
        {
            s += 2;
            notes.Add("Married status is a positive personal factor");
        }
        if (age is < 18 or > 55)
        {
            s -= 4;
            notes.Add("Age outside the optimal 18-55 range");
        }
        if (hasPriorRefusal)
        {
            s -= 5;
            notes.Add("Prior visa refusal is a significant risk factor");
        }
        if (hasCriminal)
        {
            s -= 8;
            notes.Add("Criminal record severely impacts eligibility");
        }
        if (hasFamily)
        {
            s += 2;
            notes.Add("Family ties in destination country is a positive factor");
        }

        return (
            Math.Max(s, 0),
            notes.Any() ? string.Join("; ", notes) : "Personal profile is clear"
        );
    }
}