using Accounting.Shared.Models;
using Accounting.Shared.Requests;
using CreateVoucherRequest = Accounting.Shared.Requests.CreateVoucherRequest;

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
             .Produces<Guid>(StatusCodes.Status200OK)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);


        group.MapPut("/{id:guid}", UpdateVoucher)
             .WithName("UpdateVoucher")
             .Produces(StatusCodes.Status204NoContent)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
             .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/", GetVouchers)
             .WithName("GetVouchers")
             .Produces<PaginatedList<VoucherDto>>(StatusCodes.Status200OK)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetVoucherById)
             .WithName("GetVoucherById")
             .Produces<VoucherDto>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status404NotFound);


        group.MapDelete("/{id:guid}", DeleteVoucher)
             .WithName("DeleteVoucher")
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status404NotFound);
    }



    private static async Task<IResult> CreateVoucher(
        [FromBody] CreateVoucherRequest request,
        IMediator mediator)
    {
        var command = new 
            CreateVoucherCommand(request.VoucherNumber, request.Description, request.Date, request.Lines);
        var id = await mediator.Send(command);
        return TypedResults.Ok(id);
    }

    private static async Task<IResult> UpdateVoucher(
        Guid id,
        [FromBody] UpdateVoucherRequest request,
        IMediator mediator)
    {
        if (id != request.Id) return TypedResults.BadRequest("ID mismatch");

        var command = new 
            UpdateVoucherCommand(request.Id, request.Description, request.Date, request.Lines);

        try
        {
            await mediator.Send(command);
            return TypedResults.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    private static async Task<IResult> GetVouchers(
        [AsParameters] GetVouchersQuery query,
        IMediator mediator)
    {
        var result = await mediator.Send(query);
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<VoucherDto>, NotFound>> GetVoucherById(
        Guid id,
        IMediator mediator)
    {
        try
        {
            var result = await mediator.Send(new GetVoucherByIdQuery(id));
            return TypedResults.Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    private static async Task<IResult> DeleteVoucher(
        Guid id,
        IMediator mediator)
    {
        try
        {
            // فرض بر اینکه DeleteVoucherCommand ساخته شده است
            await mediator.Send(new DeleteVoucherCommand(id));
            return TypedResults.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }
}