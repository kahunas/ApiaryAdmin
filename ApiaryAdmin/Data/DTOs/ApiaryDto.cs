using FluentValidation;

namespace ApiaryAdmin.Data.DTOs;

public record ApiaryDto(int ApiaryId, string Name, string Location, string Description);

public record CreateApiaryDto(string Name, string Location, string Description)
{
    public class CreateApiaryDtoValidator : AbstractValidator<CreateApiaryDto>
    {
        public CreateApiaryDtoValidator()
        {
            RuleFor(x => x).NotNull();
            RuleFor(x => x.Name).NotEmpty().Length(min: 2, max: 100);
            RuleFor(x => x.Location).NotEmpty().Length(min: 2, max: 100);
            RuleFor(x => x.Description).NotEmpty().Length(min: 1, max: 100);
        }
    }
};

public record UpdateApiaryDto(string Name, string Location, string Description)
{
    public class UpdateApiaryDtoValidator : AbstractValidator<UpdateApiaryDto>
    {
        public UpdateApiaryDtoValidator()
        {
            RuleFor(x => x).NotNull();
            RuleFor(x => x.Name).NotEmpty().Length(min: 2, max: 100);
            RuleFor(x => x.Location).NotEmpty().Length(min: 2, max: 100);
            RuleFor(x => x.Description).NotEmpty().Length(min: 1, max: 100);
        }
    }
};