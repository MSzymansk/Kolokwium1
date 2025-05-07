namespace Kolokwium1.Models.DTOs;

public class VisitPostDto
{
    public int VisitId { get; set; }
    public int ClientId { get; set; }
    public string mechanicLicenceNumber { get; set; }
    public List<ServicesPostDto> Services { get; set; }
}

public class ServicesPostDto
{
    public string serviceName { get; set; }
    public decimal serviceFee { get; set; }

}