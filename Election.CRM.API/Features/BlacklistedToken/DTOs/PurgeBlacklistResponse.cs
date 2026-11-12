namespace Election.CRM.API.Features.Auth.DTOs;

public class PurgeBlacklistResponse
{
    public int PurgedCount { get; set; }
    public DateTime ExecutedAt { get; set; }
}