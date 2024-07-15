﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gate2.Watch.DataAccess
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Objects;
    using System.Data.Objects.DataClasses;
    using System.Linq;
    
    public partial class GateEntities : DbContext
    {
        public GateEntities()
            : base("name=GateEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Turnstile> Turnstiles { get; set; }
        public DbSet<TurnStileServer> TurnStileServers { get; set; }
    
        public virtual ObjectResult<Nullable<int>> UpdateTicketby_No(string qr)
        {
            var qrParameter = qr != null ?
                new ObjectParameter("qr", qr) :
                new ObjectParameter("qr", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("UpdateTicketby_No", qrParameter);
        }
    }
}
