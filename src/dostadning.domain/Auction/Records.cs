using System;
using System.Collections.Generic;

namespace dostadning.domain.auction.record
{
    public class Auction
    {
        public int Id { get; set; }
        public List<Request> Requests { get; set; }
    }

    public class Request
    {
        public long TimeStamp {get;set;}
        public int Id { get;set;}
        public string Type {get;set;}
        public string Status {get;set;}
    }
}