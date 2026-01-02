namespace Accounting.Application.Features.Vouchers.UpdateVoucher;

public class UpdateVoucherHandler(IAppDbContext context) : IRequestHandler<UpdateVoucherCommand>
{

    public async Task Handle(UpdateVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await context.Vouchers
            .Include(v => v.Lines)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (voucher == null)
        {
            throw new NotFoundException($"سند با شناسه {request.Id} یافت نشد.");
        }

        voucher.Update(request.Description, request.Date);

        var requestLineIds = request.Lines
            .Select(l => l.Id)
            .Where(id => id != Guid.Empty)
            .ToHashSet();

    
        var linesToDelete = voucher.Lines
            .Where(l => !requestLineIds.Contains(l.Id))
            .ToList();

        foreach (var line in linesToDelete)
        {
            voucher.RemoveLine(line.Id);
        }

        foreach (var lineDto in request.Lines)
        {
            var existsInDb = lineDto.Id != Guid.Empty &&
                             voucher.Lines.Any(l => l.Id == lineDto.Id);

            if (existsInDb)
            {
                voucher.UpdateLine(
                    lineDto.Id,
                    lineDto.AccountCode,
                    lineDto.Description,
                    lineDto.Debit,
                    lineDto.Credit
                );
            }
            else
            {
                voucher.AddLine(
                    lineDto.AccountCode,
                    lineDto.Description,
                    lineDto.Debit,
                    lineDto.Credit
                );
            }
        }

        if (!voucher.IsBalanced())
        {
            throw new InvalidOperationException("سند تراز نیست.");
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}