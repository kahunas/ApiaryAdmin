using FluentValidation;

namespace ApiaryAdmin.Data.DTOs;

public record HiveDto(int ApiaryId, int HiveId, string Name, string Description);

public record CreateHiveDto(string Name, string Description)
{
    public class CreateHiveDtoValidator : AbstractValidator<CreateHiveDto>
    {
        public CreateHiveDtoValidator()
        {
            RuleFor(x => x).NotNull();
            RuleFor(x => x.Name).NotEmpty().Length(min: 2, max: 100);
            RuleFor(x => x.Description).NotEmpty().Length(min: 1, max: 100);
        }
    }
};

public record UpdateHiveDto(string Name, string Description)
{
    public class UpdateHiveDtoValidator : AbstractValidator<UpdateHiveDto>
    {
        public UpdateHiveDtoValidator()
        {
            RuleFor(x => x).NotNull();
            RuleFor(x => x.Name).NotEmpty().Length(min: 2, max: 100);
            RuleFor(x => x.Description).NotEmpty().Length(min: 1, max: 100);
        }
    }
};