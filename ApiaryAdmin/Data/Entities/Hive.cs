using System.ComponentModel.DataAnnotations;
using ApiaryAdmin.Data.DTOs;

namespace ApiaryAdmin.Data.Entities;

public class Hive
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; }

    public Apiary Apiary { get; set; }

    public HiveDto ToDto()
    {
        return new HiveDto(this.Apiary.Id, this.Id, this.Name, this.Description);
    }
    
}