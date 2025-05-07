namespace Kolokwium1.Models.DTOs;

public class VisitDto
{
    public DateTime Date { get; set; }

    public ClientDto Client { get; set; } 
    public MechanicDto Mechanic { get; set; }
    public List<VisitServicesDto> VisitServices { get; set; }
    
    
}

public class VisitServicesDto
{
    public string Name { get; set; }
    public decimal ServiceFee { get; set; }
    
}

public class MechanicDto
{
    public int MechanicId { get; set; }
    public string LicenceNumber { get; set; }
    
    
}

public class ClientDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }

}