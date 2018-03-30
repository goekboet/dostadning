using System;
using System.Collections.Generic;

namespace dostadning.domain.ourdata
{
    public class Auction
    {
        public int Id { get; set; }
        public string InputKey { get; set; }

        public List<Request> Requests { get; set; }
    }

    public class Request
    {
        public int Id { get; set; }

        public DateTimeOffset Created { get; set; }
        public List<Update> Updates { get; set; }
    }

    public class Update
    {
        public DateTimeOffset Created { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
    }
}