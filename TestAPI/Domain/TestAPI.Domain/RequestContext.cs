using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace TestAPI.Domain
{
    public static class RequestContext
    {
        private static readonly AsyncLocal<int?> _userId = new();
        private static readonly AsyncLocal<string?> _email = new();
        private static readonly AsyncLocal<bool> _isAuthenticated = new();

        public static int? UserId
        {
            get => _userId.Value;
            set => _userId.Value = value;
        }

        public static string? Email
        {
            get => _email.Value;
            set => _email.Value = value;
        }

        public static bool IsAuthenticated
        {
            get => _isAuthenticated.Value;
            set => _isAuthenticated.Value = value;
        }
    }
}
