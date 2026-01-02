namespace Accounting.Application.Features.Vouchers.UpdateVoucher;

public class UpdateVoucherCommandValidator : AbstractValidator<UpdateVoucherCommand>
{
    public UpdateVoucherCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Lines).NotEmpty().WithMessage("سند باید حداقل یک ردیف داشته باشد.");

        RuleForEach(v => v.Lines).ChildRules(line => {
            line.RuleFor(l => l.AccountCode).NotEmpty();
            line.RuleFor(l => l.Debit).GreaterThanOrEqualTo(0);
            line.RuleFor(l => l.Credit).GreaterThanOrEqualTo(0);
        });
    }
}