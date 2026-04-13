using System;
using System.Collections.Generic;
using System.Text;

namespace BB.Domain.DTO
{
    public class PaginatedResponse<T> where T : class
    {
        public IEnumerable<T> Items { get; set; } = [];
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
