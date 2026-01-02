using Accounting.Domain.Abstractions;

namespace Accounting.Domain.Entities;

public class VoucherSequence : BaseEntity
{
    public int FiscalYear { get; private set; }
    public long LastNumber { get; private set; }

    private VoucherSequence() { }

    public VoucherSequence(int fiscalYear) : base(Guid.NewGuid())
    {
        FiscalYear = fiscalYear;
        LastNumber = 0;
    }

    public long Increment()
    {
        LastNumber++;
        return LastNumber;
    }
}