using Accounting.Application.Features.Vouchers;
using FluentValidation;
using System.Text.RegularExpressions;

namespace Accounting.Web.Validators;

public class VoucherLineValidator : AbstractValidator<VoucherLineDto>
{
    public VoucherLineValidator()
    {
        
        RuleFor(x => x.AccountCode)
            .NotEmpty().WithMessage("کد حساب الزامی است.")
            .Must(BeValidAccountCode)
            .WithMessage("فرمت کد حساب نامعتبر است. (مثال: 1001-2001-3001)");

        
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("شرح سطر الزامی است.");

        
        RuleFor(x => x.Debit)
            .GreaterThanOrEqualTo(0).WithMessage("مبلغ بدهکار نمی‌تواند منفی باشد.");

        RuleFor(x => x.Credit)
            .GreaterThanOrEqualTo(0).WithMessage("مبلغ بستانکار نمی‌تواند منفی باشد.");

        
        RuleFor(x => x)
            .Must(x => x.Debit == 0 || x.Credit == 0)
            .WithMessage("سطر نمی‌تواند همزمان بدهکار و بستانکار باشد.")
            .When(x => x.Debit > 0 && x.Credit > 0);

        
        RuleFor(x => x)
            .Must(x => x.Debit > 0 || x.Credit > 0)
            .WithMessage("لطفا مبلغ بدهکار یا بستانکار را وارد کنید.");
    }

    private bool BeValidAccountCode(string rawCode)
    {
        if (string.IsNullOrWhiteSpace(rawCode)) return false;

        var segments = rawCode.Split('-');

        if (segments.Length < 3) return false;

        return segments.All(segment => Regex.IsMatch(segment, @"^\d{4,8}$"));
    }
}

public class VoucherValidator : AbstractValidator<VoucherDto>
{
    public VoucherValidator()
    {
        RuleFor(x => x.VoucherNumber)
            .GreaterThan(0).WithMessage("شماره سند باید بزرگتر از صفر باشد.");

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("تاریخ سند نمی‌تواند در آینده باشد.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("شرح سند الزامی است.");

        RuleFor(x => x.Lines)
            .Must(x => x != null && x.Count > 0)
            .WithMessage("سند باید حداقل دارای یک سطر باشد.");

      
        RuleForEach(x => x.Lines).SetValidator(new VoucherLineValidator());

        RuleFor(x => x.Lines)
            .Must(BeBalanced)
            .WithMessage("سند تراز نیست. جمع بدهکار و بستانکار باید برابر باشد.")
            .When(x => x.Lines is { Count: > 0 });
    }

    private bool BeBalanced(List<VoucherLineDto> lines)
    {
        var totalDebit = lines.Sum(l => l.Debit);
        var totalCredit = lines.Sum(l => l.Credit);
        return totalDebit == totalCredit;
    }
}