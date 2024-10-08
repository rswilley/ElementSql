﻿using System.ComponentModel.DataAnnotations;
using ElementSql.Attributes;
using ElementSql.Interfaces;

namespace ElementSql.Example.Data.PersonRepository
{
    [Table(TableConstants.Person)]
    public class Person : EntityBase<long>
    {
        [Key]
        public override long Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string EmailAddress { get; set; } = null!;
        public Guid UniqueId { get; set; } = Guid.NewGuid();
    }
}
