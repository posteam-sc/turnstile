//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gate4.Watch.DataAccess
{
    using System;
    using System.Collections.Generic;
    
    public partial class Turnstile
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public int SerialNo { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public int door { get; set; }
        public bool status { get; set; }
        public int ServerID { get; set; }
        public string UserDefinded { get; set; }
        public bool onoff { get; set; }
        public string PlaceStatus { get; set; }
        public Nullable<bool> IsAcceptChild { get; set; }
    
        public virtual TurnStileServer TurnStileServer { get; set; }
    }
}
