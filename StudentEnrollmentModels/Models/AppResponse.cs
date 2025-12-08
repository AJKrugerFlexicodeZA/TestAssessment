using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MIDTIER.Models
{
    public class AppResponse
    {
        public int Code { get; set; }
        public string? Message { get; set; }
        public bool Success { get; set; }
        public dynamic? Data { get; set; }
        public dynamic? Error { get; set; }
    }

    public class AppResponse<T>
    {
        public int Code { get; set; }
        public string? Message { get; set; }
        public bool Success { get; set; }
        public T? Data { get; set; }
        public object? Error { get; set; }
    }
}
