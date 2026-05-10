using Microsoft.Data.SqlClient;
using USUN2.Common.Models;
using USUN2.Data.Abstractions;
using USUN2.Data.Infrastructure;

namespace USUN2.Data.Repositories;

public sealed class LikeListRepository(SqlConnectionFactory connectionFactory) : ILikeListRepository
{
    public async Task<LikeListListItemDto?> GetBySnAsync(int sn, string userId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("dbo.sp_LikeList_GetBySn", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@Sn", sn);
        command.Parameters.AddWithValue("@UserId", userId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new LikeListListItemDto
        {
            Sn = reader.GetInt32(0),
            UserId = reader.GetString(1),
            UserName = reader.GetString(2),
            Email = reader.GetString(3),
            ProductName = reader.GetString(4),
            Price = reader.GetDecimal(5),
            FeeRate = reader.GetDecimal(6),
            OrderQty = reader.GetInt32(7),
            DebitAccount = reader.GetString(8),
            TotalAmount = reader.GetDecimal(9),
            TotalFee = reader.GetDecimal(10)
        };
    }

    public async Task<IReadOnlyList<LikeListListItemDto>> GetByUserAsync(
        string userId,
        string? productNameContains = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("dbo.sp_LikeList_SelectByUser", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue(
            "@ProductNameContains",
            string.IsNullOrWhiteSpace(productNameContains) ? DBNull.Value : productNameContains.Trim());

        var list = new List<LikeListListItemDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(new LikeListListItemDto
            {
                Sn = reader.GetInt32(0),
                UserId = reader.GetString(1),
                UserName = reader.GetString(2),
                Email = reader.GetString(3),
                ProductName = reader.GetString(4),
                Price = reader.GetDecimal(5),
                FeeRate = reader.GetDecimal(6),
                OrderQty = reader.GetInt32(7),
                DebitAccount = reader.GetString(8),
                TotalAmount = reader.GetDecimal(9),
                TotalFee = reader.GetDecimal(10)
            });
        }

        return list;
    }

    public async Task<int> InsertWithProductAsync(LikeListMutationRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("dbo.sp_LikeList_InsertWithProduct", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@UserId", request.UserId);
        command.Parameters.AddWithValue("@ProductName", request.ProductName);
        command.Parameters.AddWithValue("@Price", request.Price);
        command.Parameters.AddWithValue("@FeeRate", request.FeeRate);
        command.Parameters.AddWithValue("@OrderQty", request.OrderQty);
        command.Parameters.AddWithValue("@DebitAccount", request.DebitAccount);

        var newSnParam = new SqlParameter("@NewSn", System.Data.SqlDbType.Int)
        {
            Direction = System.Data.ParameterDirection.Output
        };
        command.Parameters.Add(newSnParam);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return (int)newSnParam.Value!;
    }

    public async Task UpdateWithProductAsync(int sn, string userId, LikeListMutationRequest request, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("dbo.sp_LikeList_UpdateWithProduct", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@Sn", sn);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@ProductName", request.ProductName);
        command.Parameters.AddWithValue("@Price", request.Price);
        command.Parameters.AddWithValue("@FeeRate", request.FeeRate);
        command.Parameters.AddWithValue("@OrderQty", request.OrderQty);
        command.Parameters.AddWithValue("@DebitAccount", request.DebitAccount);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(int sn, string userId, CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("dbo.sp_LikeList_DeleteWithOrphanProduct", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@Sn", sn);
        command.Parameters.AddWithValue("@UserId", userId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
