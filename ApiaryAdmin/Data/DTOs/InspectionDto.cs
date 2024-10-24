using System.Data;
using FluentValidation;

namespace ApiaryAdmin.Data.DTOs;

public record InspectionDto(int HiveId, int InspectionId, string Title, DateTimeOffset Date, string Notes);

public record CreateInspectionDto(string Title, DateTime Date, string Notes)
{
    public class CreateInspectionDtoValidator : AbstractValidator<CreateInspectionDto>
    {
        public CreateInspectionDtoValidator()
        {
            RuleFor(x => x).NotNull();
            RuleFor(x => x.Title).NotEmpty().Length(min: 2, max: 100);
            RuleFor(x => x.Date).NotEmpty().NotNull();
            RuleFor(x => x.Notes).NotEmpty().Length(min: 1, max: 100);
        }
    }
};

public record UpdateInspectionDto(string Title, DateTime Date, string Notes)
{
    public class UpdateInspectionDtoValidator : AbstractValidator<UpdateInspectionDto>
    {
        public UpdateInspectionDtoValidator()
        {
            RuleFor(x => x).NotNull();
            RuleFor(x => x.Title).NotEmpty().Length(min: 2, max: 100);
            RuleFor(x => x.Date).NotEmpty().NotNull();
            RuleFor(x => x.Notes).NotEmpty().Length(min: 1, max: 100);
        }
    }
};