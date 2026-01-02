namespace Accounting.Application.Features.Vouchers;

public class VoucherDto
{
    public VoucherDto(Guid id,
        long voucherNumber,
        DateOnly date,
        string? description,
        bool isFinalized,
        List<VoucherLineDto> lines)
    {
        Id = id;
        VoucherNumber = voucherNumber;
        Date = date;
        Description = description;
        IsFinalized = isFinalized;
        Lines = lines;
    }

    public VoucherDto()
    {
        Lines = [];
    }
    public Guid Id { get; set; }
    public long VoucherNumber { get; set; }
    public DateOnly Date { get; set; } 
    public string? Description { get; set; } 
    public bool IsFinalized { get; set; }
    public List<VoucherLineDto> Lines { get; set; }

    public void Deconstruct(out Guid Id, out long VoucherNumber, out DateOnly Date, out string? Description, out bool IsPosted, out List<VoucherLineDto> Lines)
    {
        Id = this.Id;
        VoucherNumber = this.VoucherNumber;
        Date = this.Date;
        Description = this.Description;
        IsPosted = this.IsFinalized;
        Lines = this.Lines;
    }
}

public class VoucherLineDto
{
    public VoucherLineDto(string accountCode,
        string description,
        decimal debit,
        decimal credit)
    {
        AccountCode = accountCode;
        Description = description;
        Debit = debit;
        Credit = credit;
    }
    public VoucherLineDto()
    {
    }
    public string AccountCode { get; set; }
    public string Description { get; set; } 
    public decimal Debit { get; set; } 
    public decimal Credit { get; set; }

    public void Deconstruct(out string AccountCode, out string Description, out decimal Debit, out decimal Credit)
    {
        AccountCode = this.AccountCode;
        Description = this.Description;
        Debit = this.Debit;
        Credit = this.Credit;
    }
}