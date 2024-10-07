using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TheBoys.Models
{
#pragma warning disable
    /// <summary> 
    /// Represents a actionstatus entity with essential details
    /// </summary>
    public class ActionStatus
    {
        /// <summary>
        /// Required field Status of the ActionStatus 
        /// </summary>
        [Required]
        public bool Status { get; set; }
    }
}