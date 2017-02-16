using System;

namespace Fluent.Core.Database
{
    /// <summary>
    /// Base class for Entity Framework entities
    /// </summary>
    public interface IBaseEntity
    {
        /// <summary>
        /// Unique reference of the entity
        /// </summary>
        Guid Id { get; set; }
        /// <summary>
        /// Date the entity was created
        /// </summary>
        /// <returns></returns>
        DateTime Created { get; set; }
        /// <summary>
        /// Date the entity was last modified
        /// </summary>
        /// <returns></returns>
        DateTime? Modified { get; set; }
        /// <summary>
        /// Has this entity been removed
        /// </summary>
        /// <returns></returns>
        bool IsDeleted { get; set; }
        /// <summary>
        /// Was this entity created by the system
        /// </summary>
        /// <returns></returns>
        bool IsSystemDefined { get; set; }
    }
}