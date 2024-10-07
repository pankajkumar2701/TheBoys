using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TheBoys.Models
{
#pragma warning disable
    /// <summary> 
    /// Represents a tokeninfo entity with essential details
    /// </summary>
    public class TokenInfo
    {
        /// <summary>
        /// Required field RefreshToken of the TokenInfo 
        /// </summary>
        [Required]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Required field Token of the TokenInfo 
        /// </summary>
        [Required]
        public string Token { get; set; }

        /// <summary>
        /// Required field TokenType of the TokenInfo 
        /// </summary>
        [Required]
        public string TokenType { get; set; }
    }
}