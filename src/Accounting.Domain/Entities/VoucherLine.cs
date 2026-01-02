using Accounting.Domain.Abstractions;
using Accounting.Domain.Exceptions;
using Accounting.Domain.ValueObjects;

namespace Accounting.Domain.Entities;

public class VoucherLine : BaseEntity
{
    public Guid VoucherId { get; private set; }

    public AccountCoding AccountCoding { get; private set; }

    public string Description { get; private set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }

    // EF Core Constructor
    private VoucherLine() { }

    internal VoucherLine(Guid voucherId, AccountCoding accountCoding, decimal debit, decimal credit, string description)
        : base(Guid.NewGuid())
    {
        // گاردها (Guards)
        if (debit < 0 || credit < 0) throw new DomainException("مبالغ منفی مجاز نیستند.");
        if (debit > 0 && credit > 0) throw new DomainException("سطر نمی‌تواند همزمان بدهکار و بستانکار باشد.");

        VoucherId = voucherId;
        AccountCoding = accountCoding;
        Description = description;
        Debit = debit;
        Credit = credit;
    }
}