namespace Accounting.Application.Features.Vouchers.GetVouchers;

public class GetVouchersValidator : AbstractValidator<GetVouchersQuery>
{
    public GetVouchersValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).WithMessage("شماره صفحه باید حداقل ۱ باشد.");
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1).LessThanOrEqualTo(100).WithMessage("تعداد رکورد در صفحه باید بین ۱ تا ۱۰۰ باشد.");
    }
}
