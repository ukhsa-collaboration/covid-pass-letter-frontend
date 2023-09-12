namespace CovidLetter.Frontend.Search
{
    public interface ISearchPatientResultVisitor<out T>
    {
        T Visit(SearchPatientResult.SuccessWithoutAccessibility result);
        T Visit(SearchPatientResult.SuccessWithAccessibility result);
        T Visit(SearchPatientResult.ImmediatePass result);
        T Visit(SearchPatientResult.SuccessWithMobile result);
        T Visit(SearchPatientResult.SuccessWithEmail result);
        T Visit(SearchPatientResult.MoreThanOneMatch result);
        T Visit(SearchPatientResult.NoMatches result);
        T Visit(SearchPatientResult.TooManyRequests result);
        T Visit(SearchPatientResult.MatchedButDeceased result);
        T Visit(SearchPatientResult.NoContactDetailsFound result);
        T Visit(SearchPatientResult.NoValidNameDetailsFound result);
        T Visit(SearchPatientResult.MatchedButNoNhsNumber result);
        T Visit(SearchPatientResult.MatchFoundButNoContactDetails result);
        T Visit(SearchPatientResult.SuccessWithMobileAndEmail result);
    }
}
