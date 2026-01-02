using Accounting.ApiService.Grpc;
using Google.Protobuf.WellKnownTypes;
using CreateVoucherRequest = Accounting.ApiService.Grpc.CreateVoucherRequest;
using UpdateVoucherRequest = Accounting.ApiService.Grpc.UpdateVoucherRequest;

namespace Accounting.ApiService.GrpcServices;

public class VoucherGrpcService(IMediator mediator) : VoucherService.VoucherServiceBase
{
    public override async Task<CreateVoucherReply> CreateVoucher(CreateVoucherRequest request, ServerCallContext context)
    {
        var voucherDate = request.VoucherDate is not null
            ? DateOnly.FromDateTime(request.VoucherDate.ToDateTime())
            : DateOnly.FromDateTime(DateTime.UtcNow);

        var command = new CreateVoucherCommand(
            request.Description,
            voucherDate,
            request.Lines.Select(l => new VoucherLineDto()
            {
                AccountCode = l.AccountCodeRaw,
                Description = l.Description,
                Debit = l.Debit,
                Credit = l.Credit
            }).ToList());

        try
        {
            var voucherId = await mediator.Send(command);

            return new CreateVoucherReply { VoucherId = voucherId.ToString() };
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<UpdateVoucherReply> UpdateVoucher(UpdateVoucherRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.VoucherId, out var voucherId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Voucher ID"));
        }

        var voucherDate = request.VoucherDate?.ToDateTime() ?? DateTime.UtcNow;

        var command = new UpdateVoucherCommand(
            voucherId,
            request.Description,
            DateOnly.FromDateTime(voucherDate),
            request.Lines.Select(l => new VoucherLineDto
            {
                AccountCode = l.AccountCodeRaw,
                Description = l.Description,
                Debit = l.Debit,
                Credit = l.Credit
            }).ToList()
        );

        try
        {
            await mediator.Send(command);
            return new UpdateVoucherReply { Success = true };
        }
        catch (Exception ex) // Catch specific NotFoundException if you have one
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }

    public override async Task<Empty> DeleteVoucher(DeleteVoucherRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.VoucherId, out var voucherId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Voucher ID"));
        }

        try
        {
            await mediator.Send(new DeleteVoucherCommand(voucherId));
            return new Empty();
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<VoucherReply> GetVoucherById(GetVoucherByIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.VoucherId, out var voucherId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid Voucher ID"));
        }

        var voucher = await mediator.Send(new GetVoucherByIdQuery(voucherId));

        if (voucher is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Voucher not found"));
        }

        return MapToVoucherReply(voucher);
    }

    public override async Task<ListVouchersReply> ListVouchers(ListVouchersRequest request, ServerCallContext context)
    {
        var query = new GetVouchersQuery()
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
        var vouchers = await mediator.Send(query);

        var response = new ListVouchersReply();
        response.Vouchers.AddRange(vouchers.Items.Select(MapToVoucherReply));

        return response;
    }

    // Helper to map Domain -> Proto
    private static VoucherReply MapToVoucherReply(VoucherDto voucherDto)
    {
        return new VoucherReply
        {
            VoucherId = voucherDto.Id.ToString(),
            VoucherNumber = voucherDto.VoucherNumber,
            Description = voucherDto.Description,
            VoucherDate = Timestamp.FromDateTime(voucherDto.Date.ToDateTime(TimeOnly.MinValue).ToUniversalTime()),
            Lines =
            {
                voucherDto.Lines.Select(l => new VoucherLineDtoGrpc()
                {
                    AccountCodeRaw = l.AccountCode,
                    Description = l.Description,
                    Debit = (long)l.Debit,
                    Credit = (long)l.Credit
                })
            }
        };
    }
}