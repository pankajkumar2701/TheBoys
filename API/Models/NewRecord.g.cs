using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TheBoys.Models
{
#pragma warning disable
    /// <summary> 
    /// Represents a newrecord entity with essential details
    /// </summary>
    public class NewRecord
    {
        /// <summary>
        /// Id of the NewRecord 
        /// </summary>
        public Guid? Id { get; set; }
    }
}