namespace Accounting.Application.Features.Vouchers.CreateVoucher;

public class CreateVoucherCommandValidator : AbstractValidator<CreateVoucherCommand>
{
    public CreateVoucherCommandValidator()
    {
        RuleFor(v => v.VoucherNumber).GreaterThan(0).WithMessage("شماره سند نامعتبر است.");
        RuleFor(v => v.Description).NotEmpty().WithMessage("شرح سند الزامی است.");
        RuleFor(v => v.Lines).NotEmpty().WithMessage("سند باید حداقل یک ردیف داشته باشد.");

        RuleForEach(v => v.Lines).ChildRules(line => {
            line.RuleFor(l => l.AccountCode).NotEmpty();
            line.RuleFor(l => l.Debit).GreaterThanOrEqualTo(0);
            line.RuleFor(l => l.Credit).GreaterThanOrEqualTo(0);
        });
    }
}