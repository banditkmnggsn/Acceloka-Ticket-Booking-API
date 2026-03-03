using System;
using System.Collections.Generic;

namespace Acceloka.Api.Infrastructure.Persistence.Entities
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
