using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Data.Domain.Models
{
    public class GoogleAuth
    {
        public int Id { get; set; }

        //
        // Summary:
        //     Gets or sets the access token issued by the authorization server.
        public string AccessToken { get; set; }
        //
        // Summary:
        //     Gets or sets the token type as specified in http://tools.ietf.org/html/rfc6749#section-7.1.
        public string TokenType { get; set; }
        //
        // Summary:
        //     Gets or sets the lifetime in seconds of the access token.
        public long? ExpiresInSeconds { get; set; }
        //
        // Summary:
        //     Gets or sets the refresh token which can be used to obtain a new access token.
        //     For example, the value "3600" denotes that the access token will expire in one
        //     hour from the time the response was generated.
        public string RefreshToken { get; set; }
        //
        // Summary:
        //     Gets or sets the scope of the access token as specified in http://tools.ietf.org/html/rfc6749#section-3.3.
        public string Scope { get; set; }
        //
        // Summary:
        //     Gets or sets the id_token, which is a JSON Web Token (JWT) as specified in http://tools.ietf.org/html/draft-ietf-oauth-json-web-token
        public string IdToken { get; set; }
        
        //
        // Summary:
        //     The date and time that this token was issued, expressed in UTC.
        //
        // Remarks:
        //     This should be set by the CLIENT after the token was received from the server.
        public DateTime IssuedUtc { get; set; }

        public string AppUserId { get; set; }
        public string Email { get; set; }

        //added account number because acc no is more unique to a bank account than email address
        public string AccountNumber { get; set; }

        public long LargestUID { get; set; }

        public long UIDValidity { get; set; }

    }
}
