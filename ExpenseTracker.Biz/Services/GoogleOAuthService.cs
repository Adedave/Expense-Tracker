using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpenseTracker.Biz.Services
{
    public class GoogleOAuthService : IGoogleOAuthService
    {
        private readonly IGoogleOAuthRepository _googleOAuthRepository;

        public GoogleOAuthService(IGoogleOAuthRepository googleOAuthRepository)
        {
            _googleOAuthRepository = googleOAuthRepository;
        }
        public void AddGoogleAuth(GoogleAuth googleAuth)
        {
            _googleOAuthRepository.Insert(googleAuth);
        }

        public void DeleteGoogleAuth(GoogleAuth googleAuth)
        {
            _googleOAuthRepository.Delete(googleAuth);
        }

        public GoogleAuth GetById(int id)
        {
            return _googleOAuthRepository.GetById(id);
        }

        public List<GoogleAuth> GetGoogleAuths(string userId)
        {
            return _googleOAuthRepository.GetAll(userId).ToList();
        }

        public GoogleAuth GetGoogleOAuthByEmail(string emailAddress)
        {
            return _googleOAuthRepository.GetGoogleOAuthByEmail(emailAddress);
        }

        public void UpdateGoogleAuth(GoogleAuth googleAuth)
        {
            _googleOAuthRepository.Update(googleAuth);
        }
    }
}
