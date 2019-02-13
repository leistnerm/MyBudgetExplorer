using System;

namespace MyBudgetExplorer.Models.YNAB
{
    public class ApiResultException : Exception
    {
        public ApiResultException() : base() { }

        public ApiResultException(string message) : base(message) { }

        public ApiResultException(string message, Exception innerException) : base(message, innerException) { }
    }
}
