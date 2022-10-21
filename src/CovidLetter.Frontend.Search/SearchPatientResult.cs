using System.Collections.Generic;

namespace CovidLetter.Frontend.Search
{
    public abstract class SearchPatientResult
    {
        public abstract T Accept<T>(ISearchPatientResultVisitor<T> visitor);

        public class SuccessWithoutAccessibility : SearchPatientResult
        {
            public UserDetail UserDetail { get; }
            public IEnumerable<string> Mobiles { get; }
            public IEnumerable<string> Emails { get; }
            public IEnumerable<Address> Addresses { get; }

            public SuccessWithoutAccessibility(
                UserDetail userDetail,
                IEnumerable<string> mobiles,
                IEnumerable<string> emails,
                IEnumerable<Address> addresses)
            {
                UserDetail = userDetail;
                Mobiles = mobiles;
                Emails = emails;
                Addresses = addresses;
            }

            public override T Accept<T>(ISearchPatientResultVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class SuccessWithAccessibility: SearchPatientResult
        {
            public UserDetail UserDetail { get; }
            public IEnumerable<string> Mobiles { get; }
            public IEnumerable<string> Emails { get; }
            public IEnumerable<Address> Addresses { get; }

            public SuccessWithAccessibility(
                UserDetail userDetail,
                IEnumerable<string> mobiles,
                IEnumerable<string> emails,
                IEnumerable<Address> addresses)
            {
                UserDetail = userDetail;
                Mobiles = mobiles;
                Emails = emails;
                Addresses = addresses;
            }

            public override T Accept<T>(ISearchPatientResultVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class ImmediatePass : SearchPatientResult
        {
            public UserDetail UserDetail { get; }
            public IEnumerable<string> Mobiles { get; }
            public IEnumerable<string> Emails { get; }
            public IEnumerable<Address> Addresses { get; }

            public ImmediatePass(
                UserDetail userDetail,
                IEnumerable<string> mobiles,
                IEnumerable<string> emails,
                IEnumerable<Address> addresses)
            {
                UserDetail = userDetail;
                Mobiles = mobiles;
                Emails = emails;
                Addresses = addresses;
            }

            public override T Accept<T>(ISearchPatientResultVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class SuccessWithMobile : SearchPatientResult
        {
            public UserDetail UserDetail { get; }
            public IEnumerable<string> Mobiles { get; }
            public IEnumerable<string> Emails { get; }
            public IEnumerable<Address> Addresses { get; }

            public SuccessWithMobile(
                UserDetail userDetail,
                IEnumerable<string> mobiles,
                IEnumerable<string> emails,
                IEnumerable<Address> addresses)
            {
                UserDetail = userDetail;
                Mobiles = mobiles;
                Emails = emails;
                Addresses = addresses;
            }

            public override T Accept<T>(ISearchPatientResultVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class SuccessWithEmail : SearchPatientResult
        {
            public UserDetail UserDetail { get; }
            public IEnumerable<string> Mobiles { get; }
            public IEnumerable<string> Emails { get; }
            public IEnumerable<Address> Addresses { get; }

            public SuccessWithEmail(
                UserDetail userDetail,
                IEnumerable<string> mobiles,
                IEnumerable<string> emails,
                IEnumerable<Address> addresses)
            {
                UserDetail = userDetail;
                Mobiles = mobiles;
                Emails = emails;
                Addresses = addresses;
            }

            public override T Accept<T>(ISearchPatientResultVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class SuccessWithMobileAndEmail : SearchPatientResult
        {
            public UserDetail UserDetail { get; }
            public IEnumerable<string> Mobiles { get; }
            public IEnumerable<string> Emails { get; }
            public IEnumerable<Address> Addresses { get; }

            public SuccessWithMobileAndEmail(
                UserDetail userDetail,
                IEnumerable<string> mobiles,
                IEnumerable<string> emails,
                IEnumerable<Address> addresses)
            {
                UserDetail = userDetail;
                Mobiles = mobiles;
                Emails = emails;
                Addresses = addresses;
            }

            public override T Accept<T>(ISearchPatientResultVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class MoreThanOneMatch : SearchPatientResult
        {
            public override T Accept<T>(ISearchPatientResultVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class NoMatches : SearchPatientResult
        {
            public override T Accept<T>(ISearchPatientResultVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class MatchFoundButNoContactDetails : SearchPatientResult
        {
            public override T Accept<T>(ISearchPatientResultVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class MatchFoundButWelshPostcode : SearchPatientResult
        {
            public override T Accept<T>(ISearchPatientResultVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class MatchedButDeceased : SearchPatientResult
        {
            public override T Accept<T>(ISearchPatientResultVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class NoContactDetailsFound : SearchPatientResult
        {
            public override T Accept<T>(ISearchPatientResultVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }
        
        public class NoValidNameDetailsFound : SearchPatientResult
        {
            public override T Accept<T>(ISearchPatientResultVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }

        public class MatchedButNoNhsNumber : SearchPatientResult
        {
            public override T Accept<T>(ISearchPatientResultVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }
        
        public class TooManyRequests : SearchPatientResult
        {
            public override T Accept<T>(ISearchPatientResultVisitor<T> visitor)
            {
                return visitor.Visit(this);
            }
        }
    }
}
