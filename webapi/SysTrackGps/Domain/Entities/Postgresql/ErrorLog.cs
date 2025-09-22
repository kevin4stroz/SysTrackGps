using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SysTrackGps.Domain.Entities.Postgresql;

[Table("error_log", Schema = "public")]
public class ErrorLog
{
    [Key]
    public required Guid id_error_log { get; set; }
    public required string controller_name { get; set; }
    public required string method_name { get; set; }
    public required string stack_trace { get; set; }
    public required DateTime created_date { get; set; }
    public required string code { get; set; }
}


