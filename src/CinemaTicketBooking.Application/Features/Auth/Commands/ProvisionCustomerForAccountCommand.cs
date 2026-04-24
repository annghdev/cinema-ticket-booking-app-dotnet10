using CinemaTicketBooking.Application.Abstractions;
using FluentValidation;

namespace CinemaTicketBooking.Application.Features.Auth.Commands;

/// <summary>
/// Creates a registered <see cref="Domain.Customer"/> and links it to the Identity account.
/// Called after <c>UserManager.CreateAsync</c> succeeds.
/// </summary>
public class ProvisionCustomerForAccountCommand : ICommand
{
    public Guid AccountId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

/// <summary>
/// Provisions customer aggregate and updates account linkage.
/// </summary>
public class ProvisionCustomerForAccountHandler(IUnitOfWork uow, IAccountCustomerLinker accountLinker)
{
    /// <summary>
    /// Creates or upgrades customer, persists, and sets Account.CustomerId.
    /// </summary>
    public async Task<Guid> Handle(ProvisionCustomerForAccountCommand command, CancellationToken ct)
    {
        // 1. Reuse guest customer by session id when available; otherwise create a new registered profile.
        var customer = string.IsNullOrWhiteSpace(command.SessionId)
            ? null
            : await uow.Customers.GetTrackedBySessionIdAsync(command.SessionId, ct);

        if (customer is null)
        {
            customer = Customer.Create(
                name: command.Name,
                sessionId: string.Empty,
                phoneNumber: command.PhoneNumber,
                email: command.Email,
                isRegistered: false);
            customer.Register();
            uow.Customers.Add(customer);
        }
        else
        {
            customer.Name = command.Name;
            customer.Email = command.Email;
            customer.PhoneNumber = command.PhoneNumber;
            customer.SessionId = string.Empty;
            if (!customer.IsRegistered)
            {
                customer.Register();
            }
            // Customer is loaded as tracked entity; explicit Update() can break xmin concurrency snapshot.
        }

        // 2. Link Identity account to customer profile.
        await accountLinker.SetCustomerIdAsync(command.AccountId, customer.Id, ct);

        await uow.CommitAsync(ct);
        
        return customer.Id;
    }
}

/// <summary>
/// Validates provisioning input.
/// </summary>
public class ProvisionCustomerForAccountValidator : AbstractValidator<ProvisionCustomerForAccountCommand>
{
    public ProvisionCustomerForAccountValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().MaximumLength(MaxLengthConsts.Email);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(MaxLengthConsts.Name);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(MaxLengthConsts.PhoneNumber);
        RuleFor(x => x.SessionId)
            .MaximumLength(MaxLengthConsts.SessionId)
            .When(x => !string.IsNullOrWhiteSpace(x.SessionId));
    }
}
