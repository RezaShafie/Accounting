using Accounting.Domain.Abstractions;
using Accounting.Domain.Exceptions;
using Accounting.Domain.ValueObjects;

namespace Accounting.Domain.Entities;

public class Voucher : AggregateRoot
{
    public long VoucherNumber { get; private set; }
    public DateOnly Date { get; private set; }
    public string? Description { get; private set; }
    public bool IsFinalized { get; private set; }

    private readonly List<VoucherLine> _lines = new();
    public IReadOnlyCollection<VoucherLine> Lines => _lines.AsReadOnly();

    private Voucher() { }

    private Voucher(long voucherNumber, string description, DateOnly date) : base(Guid.NewGuid())
    {
        VoucherNumber = voucherNumber;
        Description = description;
        Date = date;
        IsFinalized = false;
    }

    public static Voucher Create(long voucherNumber, string description, DateOnly date)
    {
        if (voucherNumber <= 0)
        {
            throw new DomainException("شماره سند باید بزرگتر از صفر باشد.");
        }

        if (date > DateOnly.FromDateTime(DateTime.Now))
        {
            throw new DomainException("تاریخ سند نمی‌تواند در آینده باشد.");
        }

        return new Voucher(voucherNumber, description, date);
    }


    public void Update(string description, DateOnly date)
    {
        if (IsFinalized)
            throw new DomainException("سند تایید شده (Posted) قابل ویرایش نیست.");

        Description = description;
        Date = date;
    }

    public void ClearLines()
    {
        if (IsFinalized)
            throw new DomainException("سند تایید شده قابل ویرایش نیست.");

        _lines.Clear();
    }

    public void AddLine(string accountCodeRaw, string description, decimal debit, decimal credit)
    {
        if (IsFinalized)
            throw new DomainException("سند تایید شده قابل ویرایش نیست.");

        var coding = AccountCoding.Create(accountCodeRaw);

        var line = new VoucherLine(Id, coding, debit, credit, description);
        _lines.Add(line);
    }
    public void UpdateLine(Guid id, string accountCodeRaw, string description, decimal debit, decimal credit)
    {
        if (IsFinalized)
            throw new DomainException("سند تایید شده قابل ویرایش نیست.");

        var line = _lines.FirstOrDefault(l => l.Id == id);

        line?.Update(AccountCoding.Create(accountCodeRaw)
            , debit
            , credit
            , description);
    }
    public void RemoveLine(Guid lineId)
    {
        var line = _lines.FirstOrDefault(x => x.Id == lineId);
        if (line != null)
        {
            _lines.Remove(line);
        }
    }

    public bool IsBalanced()
    {
        var totalDebit = _lines.Sum(l => l.Debit);
        var totalCredit = _lines.Sum(l => l.Credit);
        return totalDebit == totalCredit && _lines.Any();
    }

    public void FinalizeVoucher()
    {
        if (!IsBalanced())
            throw new DomainException("سند تراز نیست و نمی‌تواند ثبت نهایی شود.");

        IsFinalized = true;
    }
}