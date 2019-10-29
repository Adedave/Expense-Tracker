using ExpenseTracker.Data.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Biz.IServices
{
    public interface IGoogleOAuthService
    {
        List<GoogleAuth> GetGoogleAuths(string userId);
        GoogleAuth GetGoogleOAuthByEmail(string emailAddress);
        GoogleAuth GetById(int id);
        void AddGoogleAuth(GoogleAuth googleAuth);
        void UpdateGoogleAuth(GoogleAuth googleAuth);
        void DeleteGoogleAuth(GoogleAuth googleAuth);
    }
}
