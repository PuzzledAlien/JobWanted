using System.Collections.Generic;

namespace JobWanted.Dto
{
    public class ZlResponse
    {
        public int Code { get; set; }
    }

    public class ZlResponse<T> : ZlResponse
    {
        public T Data { get; set; }
    }

    public class ZlSouResponse
    {
        public List<ZlSouResult> Results { get; set; }
    }

    public class ZlSouResult
    {
        public string Number { get; set; }
        public string JobName { get; set; }
        public ZlCompany Company { get; set; }
        public string Salary { get; set; }
        public ZlCity City { get; set; }
        public string BusinessArea { get; set; }
        public string UpdateDate { get; set; }
        public string PositionURL { get; set; }
    }

    public class ZlCompany
    {
        public string Name { get; set; }
    }

    public class ZlCity
    {
        public string Display { get; set; }
    }
}
