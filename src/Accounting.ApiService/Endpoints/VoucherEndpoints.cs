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
        // Note: Unified API usually returns 200 OK with success=true, not 204 No Content

        group.MapGet("/", GetVouchers)
             .WithName("GetVouchers")
             .Produces<Result<PaginatedList<VoucherDto>>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetVoucherById)
             .WithName("GetVoucherById")
             .Produces<Result<VoucherDto>>(StatusCodes.Status200OK);

        group.MapDelete("/{id:guid}", DeleteVoucher)
             .WithName("DeleteVoucher")
             .Produces<Result>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreateVoucher(
        [FromBody] CreateVoucherRequest request,
        IMediator mediator)
    {
        var command = new CreateVoucherCommand(request.VoucherNumber, request.Description, request.Date, request.Lines);
        var id = await mediator.Send(command);

        // Wrap response in Result
        return TypedResults.Ok(Result<Guid>.Success(id, "Voucher created successfully."));
    }

    private static async Task<IResult> UpdateVoucher(
        Guid id,
        [FromBody] UpdateVoucherRequest request,
        IMediator mediator)
    {
        if (id != request.Id)
        {
            // Even validation errors should follow the unified structure
            return TypedResults.Ok(Result.Failure("Voucher ID mismatch in body and URL."));
        }

        var command = new UpdateVoucherCommand(request.Id, request.Description, request.Date, request.Lines);

        await mediator.Send(command);

        // Return 200 OK with Success wrapper
        return TypedResults.Ok(Result.Success("Voucher updated successfully."));
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

        // Assuming your Query throws NotFoundException if null, 
        // global handler catches it. If it returns null, handle it here:
        if (result is null)
            throw new KeyNotFoundException($"Voucher with ID {id} not found.");

        return TypedResults.Ok(Result<VoucherDto>.Success(result));
    }

    private static async Task<IResult> DeleteVoucher(
        Guid id,
        IMediator mediator)
    {
        await mediator.Send(new DeleteVoucherCommand(id));
        return TypedResults.Ok(Result.Success("Voucher deleted successfully."));
    }
}