﻿using System.ComponentModel.DataAnnotations;
using ApiaryAdmin.Data.DTOs;

namespace ApiaryAdmin.Data.Entities;

public class Inspection
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required DateTimeOffset Date { get; set; }
    public string Notes { get; set; }

    public Hive Hive { get; set; }

    public InspectionDto ToDto()
    {
        return new InspectionDto(Hive.Id, Id, Title, Date, Notes);
    }
}