using CinemaTicketBooking.Application.Abstractions;
using CinemaTicketBooking.Application.Common.Auth;

namespace CinemaTicketBooking.Application.Features;

public class GetPagedAccountsQuery : IQuery<PagedResult<AccountDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public bool IsCustomerGroup { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
}

public class GetPagedAccountsHandler(IQueryService queryService)
{
    public async Task<PagedResult<AccountDto>> Handle(GetPagedAccountsQuery query, CancellationToken ct)
    {
        var offset = (query.PageNumber - 1) * query.PageSize;
        var searchTerm = string.IsNullOrWhiteSpace(query.SearchTerm) ? null : $"%{query.SearchTerm}%";

        // 1. Get count
        var countSql = @"
            SELECT COUNT(*)
            FROM accounts a
            WHERE a.""DeletedAt"" IS NULL
              AND (@SearchTerm IS NULL OR a.""UserName"" ILIKE @SearchTerm OR a.""Email"" ILIKE @SearchTerm)
              AND (
                  (@IsCustomerGroup = TRUE AND EXISTS (
                      SELECT 1 FROM account_roles ur 
                      JOIN roles r ON ur.""RoleId"" = r.""Id"" 
                      WHERE ur.""UserId"" = a.""Id"" AND r.""Name"" = 'Customer'
                  ))
                  OR 
                  (@IsCustomerGroup = FALSE AND NOT EXISTS (
                      SELECT 1 FROM account_roles ur 
                      JOIN roles r ON ur.""RoleId"" = r.""Id"" 
                      WHERE ur.""UserId"" = a.""Id"" AND r.""Name"" = 'Customer'
                  ))
              )";

        var totalItems = await queryService.QueryFirstOrDefaultAsync<int>(countSql, new
        {
            SearchTerm = searchTerm,
            IsCustomerGroup = query.IsCustomerGroup
        }, ct);

        if (totalItems == 0)
            return new PagedResult<AccountDto>([], 0, query.PageNumber, query.PageSize);

        // 2. Get items with roles
        var sql = @"
            SELECT 
                a.""Id"", 
                a.""UserName"", 
                a.""Email"", 
                a.""PhoneNumber"", 
                a.""AvatarUrl"", 
                a.""LockoutEnd"",
                a.""CreatedAt"",
                c.""Id"" as ""CustomerId"",
                c.""Name"" as ""CustomerName"",
                array_to_string(array(
                    SELECT r.""Name"" 
                    FROM account_roles ur 
                    JOIN roles r ON ur.""RoleId"" = r.""Id"" 
                    WHERE ur.""UserId"" = a.""Id""
                ), ',') as ""RolesCsv""
            FROM accounts a
            LEFT JOIN customers c ON a.""CustomerId"" = c.""Id""
            WHERE a.""DeletedAt"" IS NULL
              AND (@SearchTerm IS NULL OR a.""UserName"" ILIKE @SearchTerm OR a.""Email"" ILIKE @SearchTerm)
              AND (
                  (@IsCustomerGroup = TRUE AND EXISTS (
                      SELECT 1 FROM account_roles ur 
                      JOIN roles r ON ur.""RoleId"" = r.""Id"" 
                      WHERE ur.""UserId"" = a.""Id"" AND r.""Name"" = 'Customer'
                  ))
                  OR 
                  (@IsCustomerGroup = FALSE AND NOT EXISTS (
                      SELECT 1 FROM account_roles ur 
                      JOIN roles r ON ur.""RoleId"" = r.""Id"" 
                      WHERE ur.""UserId"" = a.""Id"" AND r.""Name"" = 'Customer'
                  ))
              )
            ORDER BY a.""CreatedAt"" DESC
            LIMIT @PageSize OFFSET @Offset";

        var rawItems = await queryService.QueryAsync<dynamic>(sql, new
        {
            SearchTerm = searchTerm,
            IsCustomerGroup = query.IsCustomerGroup,
            PageSize = query.PageSize,
            Offset = offset
        }, ct);

        var now = DateTimeOffset.UtcNow;
        var items = rawItems.Select(x => new AccountDto(
            Id: x.Id,
            UserName: x.UserName,
            Email: x.Email,
            PhoneNumber: x.PhoneNumber,
            AvatarUrl: x.AvatarUrl,
            IsLockedOut: x.LockoutEnd != null && (DateTimeOffset)x.LockoutEnd > now,
            LockoutEnd: x.LockoutEnd,
            CreatedAt: x.CreatedAt,
            Roles: ((string)(x.RolesCsv ?? "")).Split(',', StringSplitOptions.RemoveEmptyEntries),
            CustomerId: x.CustomerId,
            CustomerName: x.CustomerName
        )).ToList();

        return new PagedResult<AccountDto>(items, totalItems, query.PageNumber, query.PageSize);
    }
}
