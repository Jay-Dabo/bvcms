using System.Collections.Generic;

namespace PushPay.Exceptions
{
    public class UserExceptionCollection : UserException
    {
        public List<UserException> Exceptions = new List<UserException>();
    }
}