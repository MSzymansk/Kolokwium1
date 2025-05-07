using Kolokwium1.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace Kolokwium1.Services;

public class VisitService : IVisitService
{
    
    private readonly string _connectionString;
    public VisitService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }
    
    public async Task<VisitDto> GetVisit(int id)
    {

        var visitDto = new VisitDto()
        {
            Client = new ClientDto(),
            Mechanic = new MechanicDto(),
            VisitServices = new List<VisitServicesDto>()
        };
        var visitServices = new Dictionary<string, VisitServicesDto>();
        
        string query = @"
        SELECT V.date,
        C.first_name,
        C.last_name,
        C.date_of_birth,
        M.mechanic_id,
        M.licence_number,
        S.name,
        S.base_fee
        FROM Visit V
         INNER JOIN Client C on V.client_id = C.client_id
         INNER JOIN Mechanic M on V.mechanic_id = M.mechanic_id
         INNER JOIN Visit_Service VS on V.visit_id = VS.visit_id
         INNER JOIN Service S on VS.service_id = S.service_id
        WHERE V.visit_id = @id;
        ";

        await using var conn = new SqlConnection(_connectionString);
        await using (var cmd = new SqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            
            await conn.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                visitDto.Date = reader.GetDateTime(reader.GetOrdinal("date"));

                if (string.IsNullOrEmpty(visitDto.Client.FirstName))
                {
                    visitDto.Client = new ClientDto()
                    {
                        FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                        LastName = reader.GetString(reader.GetOrdinal("last_name")),
                        DateOfBirth = reader.GetDateTime(reader.GetOrdinal("date_of_birth")),
                    };
                }

                if (string.IsNullOrEmpty(visitDto.Mechanic.LicenceNumber))
                {
                    visitDto.Mechanic = new MechanicDto()
                    {
                        MechanicId = reader.GetInt32(reader.GetOrdinal("mechanic_id")),
                        LicenceNumber = reader.GetString(reader.GetOrdinal("licence_number")),
                    };
                }
                
                var serviceName = reader.GetString(reader.GetOrdinal("name"));
                if (!visitServices.ContainsKey(serviceName))
                {
                    var service = new VisitServicesDto()
                    {
                        Name = serviceName,
                        ServiceFee = reader.GetDecimal(reader.GetOrdinal("base_fee"))
                    };

                    visitServices[serviceName] = service;
                    visitDto.VisitServices.Add(service);
                }
                
            }           
        }


        if (visitDto is null)
        {
            throw new Exception();
        }
        
        return visitDto;
    }

    public async Task PostVisit(VisitPostDto visitDto)
    {
        await using var conn = new SqlConnection(_connectionString);
        await using SqlCommand cmd = new SqlCommand("", conn);

        await conn.OpenAsync();
        var transaction = conn.BeginTransaction();
        cmd.Transaction = transaction as SqlTransaction;

        try
        {
            cmd.CommandText = @"SELECT mechanic_id FROM Mechanic WHERE licence_number = @number;";
            cmd.Parameters.AddWithValue("@number", visitDto.mechanicLicenceNumber);
            
            var result = await cmd.ExecuteScalarAsync();
                
            if (result == null || result == DBNull.Value)
            {
                throw new Exception($"Mechanic with '{visitDto.mechanicLicenceNumber}' not found");
            }

            var mechanicId = (int)result;
            
            
            
            cmd.Parameters.Clear();
            cmd.CommandText = @"
            INSER INTO Visit(visit_id, client_id, mechanic_id, date)
            VALUES (@visit_id, @client_id, @mechanic_id, @date)
            ";
            
            cmd.Parameters.AddWithValue("@visit_id", visitDto.VisitId);
            cmd.Parameters.AddWithValue("@client_id", visitDto.ClientId);
            cmd.Parameters.AddWithValue("mechanic_id", mechanicId);
            cmd.Parameters.AddWithValue("date", DateTime.Now);


            foreach (var service in visitDto.Services)
            {
                cmd.Parameters.Clear();
                cmd.CommandText = @"SELECT service_id FROM Service S WHERE S.name = @name";
                cmd.Parameters.AddWithValue("@name", service.serviceName);

                result = await cmd.ExecuteScalarAsync();
                
                if (result == null || result == DBNull.Value)
                {
                    throw new Exception($"Service with name '{service.serviceName}' not found");
                }
                
                int serviceId = Convert.ToInt32(result);
                cmd.Parameters.Clear();
                cmd.CommandText = @"
                     INSER INTO Visit_Service(visit_id, service_id, service_fee)
                    VALUES (@visitId, @serviceId, @serviceFee)
                ";

                cmd.Parameters.AddWithValue("@visitId", visitDto.VisitId);
                cmd.Parameters.AddWithValue("@serviceId", serviceId);
                cmd.Parameters.AddWithValue("@serviceFee", service.serviceFee);
            }
            
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
}