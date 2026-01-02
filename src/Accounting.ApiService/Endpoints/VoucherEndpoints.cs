using Accounting.Application.Features.Vouchers.FinalizeVoucher;

namespace Accounting.ApiService.Endpoints;

public static class VoucherEndpoints
{
    public static void MapVoucherEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vouchers")
                        .WithTags("Vouchers")
                        .WithOpenApi();

        group.MapPost("/", CreateVoucher)
             .WithName("CreateVoucher")
             .Produces<Result<Guid>>(StatusCodes.Status200OK);

        group.MapPut("/{id:guid}", UpdateVoucher)
             .WithName("UpdateVoucher")
             .Produces<Result>(StatusCodes.Status200OK);

        group.MapGet("/", GetVouchers)
             .WithName("GetVouchers")
             .Produces<Result<PaginatedList<VoucherDto>>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetVoucherById)
             .WithName("GetVoucherById")
             .Produces<Result<VoucherDto>>(StatusCodes.Status200OK);

        group.MapDelete("/{id:guid}", DeleteVoucher)
             .WithName("DeleteVoucher")
             .Produces<Result>(StatusCodes.Status200OK);

        group.MapPost("finalize/{id:guid}", FinalizeVoucher)
             .WithName("FinalizeVoucher")
             .Produces<Result>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateVoucher(
        [FromBody] CreateVoucherRequest request,
        IMediator mediator)
    {
        var command = new CreateVoucherCommand(request.Description, request.Date, request.Lines);
        var id = await mediator.Send(command);

        // Wrap response in Result
        return TypedResults.Ok(Result<Guid>.Success(id, "سند با موفقیت ساخته شد"));
    }

    private static async Task<IResult> UpdateVoucher(
        Guid id,
        [FromBody] UpdateVoucherRequest request,
        IMediator mediator)
    {
        if (id != request.Id)
        {
            // Even validation errors should follow the unified structure
            return TypedResults.Ok(Result.Failure("شناسه همخوانی ندارد"));
        }

        var command = new UpdateVoucherCommand(request.Id, request.Description, request.Date, request.Lines);

        await mediator.Send(command);

        // Return 200 OK with Success wrapper
        return TypedResults.Ok(Result.Success("سند با موفقیت بروز شد."));
    }

    private static async Task<IResult> GetVouchers(
        [AsParameters] GetVouchersQuery query,
        IMediator mediator)
    {
        var result = await mediator.Send(query);
        return TypedResults.Ok(Result<PaginatedList<VoucherDto>>.Success(result));
    }

    private static async Task<IResult> GetVoucherById(
        Guid id,
        IMediator mediator)
    {
        var result = await mediator.Send(new GetVoucherByIdQuery(id));

        if (result is null)
            throw new NotFoundException($"سند با شناسه {id} یافت نشد");

        return TypedResults.Ok(Result<VoucherDto>.Success(result));
    }

    private static async Task<IResult> DeleteVoucher(
        Guid id,
        IMediator mediator)
    {
        await mediator.Send(new DeleteVoucherCommand(id));
        return TypedResults.Ok(Result.Success("سند حذف شد."));
    }
    private static async Task<IResult> FinalizeVoucher(
        Guid id,
        IMediator mediator)
    {
        await mediator.Send(new FinalizeVoucherCommand(id));
        return TypedResults.Ok(Result.Success("سند نهایی شد."));
    }
}