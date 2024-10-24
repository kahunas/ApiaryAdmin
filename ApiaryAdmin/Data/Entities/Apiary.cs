﻿using System.ComponentModel.DataAnnotations;
using ApiaryAdmin.Data.DTOs;

namespace ApiaryAdmin.Data.Entities;

public class Apiary
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string Location { get; set; }
    public string Description { get; set; }
    public DateTimeOffset CreationDate { get; set; }
    
    public ApiaryDto ToDto()
    {
        return new ApiaryDto(Id, Name, Location, Description);
    }
}