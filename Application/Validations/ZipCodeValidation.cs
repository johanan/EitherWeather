using LanguageExt;

namespace EitherWeather.Application.Validations;

public static class ZipCodeValidation
{
    public static Either<Exception, string> NotEmpty(string zipCode) => string.IsNullOrWhiteSpace(zipCode) 
        ? Either<Exception, string>.Left(new ArgumentException("Zip Code is blank", "ZipCode"))
        : Either<Exception, string>.Right(zipCode);
    
    public static Either<Exception, string> ValidateZipCode(string zipCode) => zipCode == "46601"
            ? Either<Exception, string>.Left(new ArgumentException("Zip Code cannot be South Bend", "ZipCode"))
            : Either<Exception, string>.Right(zipCode);
    
    public static Either<Exception, string> IntroduceProblems(string zipCode)
    {
        Random rand = new Random();
        bool introduceProblems = rand.Next(2) == 1;
        return introduceProblems
            ? Either<Exception, string>.Left(new Exception("Just not your time"))
            : Either<Exception, string>.Right(zipCode);
    }
}