using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TestAPI.Domain
{
    public abstract class DomainEntity
    {
        [Key]
        public long EntityKey { get; set; }

        /// <summary>
        /// UTC timestamp when the entity was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// User ID who created the entity
        /// </summary>
        public long? CreatedBy { get; set; }

        /// <summary>
        /// UTC timestamp when the entity was last modified
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// User ID who last modified the entity
        /// </summary>
        public long? UpdatedBy { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// UTC timestamp when the entity was soft deleted
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// User ID who soft deleted the entity
        /// </summary>
        public long? DeletedBy { get; set; }

        public abstract string EntityType { get; }
    }
}
